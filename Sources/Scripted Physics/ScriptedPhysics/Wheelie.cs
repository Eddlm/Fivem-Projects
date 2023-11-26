using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyResource.ScriptedPhysics
{
    internal enum WheelieState
    {
        None,
        Ready,
        Wheelie,
    }

    internal class Wheelie : BaseScript
    {
        private WheelieState WState = WheelieState.None;

        public Wheelie()
        {
        }

        private VehicleClass[] InnapropiateToManage = { VehicleClass.Boats, VehicleClass.Helicopters, VehicleClass.Planes, VehicleClass.Cycles, VehicleClass.Motorcycles };

        [Tick]
        public Task OnTick()
        {
            if (Game.Player == null) return Task.FromResult(0);

            if (Game.PlayerPed.SeatIndex == VehicleSeat.Driver)
            {
                Vehicle v = Game.PlayerPed.CurrentVehicle;
                if (InnapropiateToManage.Contains(v.ClassType) == false)
                {
                    if (API.GetConvarInt("cw_disable_vanilla", 1) == 1) DisableVanillaWheelie(v);
                    if (API.GetConvarInt("cw_enable_custom_wheelies", 1) == 1) HandleWheelie(v);
                }
            }

            return Task.FromResult(0);
        }

        private void DisableVanillaWheelie(Vehicle v)
        {
            if (API.GetVehicleWheelieState(v.Handle) == 129) API.SetVehicleWheelieState(v.Handle, 1);
        }

        /// <summary>
        /// Manages custom Wheelie states and handles the physical wheelie.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private void HandleWheelie(Vehicle vehicle)
        {
            switch (this.WState)
            {
                case WheelieState.None:
                    {
                        //Only Muscles check
                        if (API.GetConvarInt("cw_only_muscle", 1) == 1 && vehicle.ClassType != VehicleClass.Muscle) break;

                        //Stopped, accelerating and on handbrake
                        if (vehicle.Velocity.Length() < 1f
                             && Game.IsControlPressed(0, Control.VehicleAccelerate)
                             && Game.IsControlPressed(0, Control.VehicleHandbrake))
                            SetWheelieState(WheelieState.Ready);

                        break;
                    }
                case WheelieState.Ready:
                    {
                        //Not accelerating or not pitching up
                        if (Game.GetControlNormal(0, Control.VehicleAccelerate) <= 0 || Game.GetControlNormal(0, Control.VehicleMoveDown) <= 0.0)
                        {
                            SetWheelieState(WheelieState.None);
                            break;
                        }

                        if (Game.IsControlJustReleased(0, Control.VehicleHandbrake)
                            && vehicle.CurrentRPM > 0.8f
                            && Game.GetControlNormal(0, Control.VehicleMoveDown) > 0.0f
                            && Game.GetControlNormal(0, Control.VehicleAccelerate) > 0.0f)
                            SetWheelieState(WheelieState.Wheelie);

                        break;
                    }
                case WheelieState.Wheelie: //TODO - Make it account for torque changes (nitro etc). Already accounts for power changes
                    {
                        //Not accelerating or not pitching up
                        if (Game.GetControlNormal(0, Control.VehicleAccelerate) <= 0.0 || Game.GetControlNormal(0, Control.VehicleMoveDown) <= 0.0)
                        {
                            SetWheelieState(WheelieState.None);
                            break;
                        }

                        if (API.HasEntityCollidedWithAnything(vehicle.Handle)) break;

                        Vector3 spdVector = API.GetEntitySpeedVector(vehicle.Handle, true);
                        spdVector.Normalize();

                        float biasFront = API.GetVehicleHandlingFloat(vehicle.Handle, "CHandlingData", "fDriveBiasFront");
                        float Accel = (API.GetVehicleAcceleration(vehicle.Handle) * 10) * ((float)API.GetConvarInt("cw_power_scale ", 200) / 100);

                        float RWDBias = Accel * (Game.GetControlNormal(0, Control.VehicleAccelerate) * (1f - biasFront));
                        float finalForce = (float)Math.Round(RWDBias * Game.GetControlNormal(0, Control.VehicleMoveDown), 3);

                        float Max= API.GetConvarInt("cw_power_max", 400) / 100;
                        if (finalForce > Max) finalForce = Max;

                        //Decay when off angle
                        float spdVectorPenalty = Util.Map(Math.Abs(spdVector.X) * 90f, 90f, 5f, 0, 1, true);

                        //Decay with speed
                        float topSpeed = Util.MStoMPH(API.GetVehicleEstimatedMaxSpeed(vehicle.Handle) / 0.75f);
                        float spdMult = Util.Map(Util.MStoMPH(vehicle.Velocity.Length()), topSpeed, topSpeed * 0.25f, 0.75f, 1, true);

                        finalForce *= spdMult;
                        finalForce *= spdVectorPenalty;

                        finalForce *= 0.01f;
                        API.ApplyForceToEntity(vehicle.Handle, 3, 0.0f, 0.0f, finalForce, 0f, 4f, 0.0f, 0, true, true, true, false, false);
                        

                        //Twist
                        //API.ApplyForceToEntity(vehicle.Handle, 3, torque, 0.0f, 0.0f, 0f, 0f, 2f, 0, true, true, true, false, false);
                        //API.ApplyForceToEntity(vehicle.Handle, 3, -torque, 0.0f, 0.0f, 0f, 0f, -2f, 0, true, true, true, false, false);

                        //Side Stabilizer
                        //API.ApplyForceToEntity(vehicle.Handle, 3, spdVector.X * -0.01f, 0.0f, 0.0f, 0f, -10f, 0f, 0, true, true, true, true, true);

                        if (vehicle.HeightAboveGround > 2f || finalForce <= 0.0f) this.SetWheelieState(WheelieState.None);
                    }
                    break;
            }
        }

        private void SetWheelieState(WheelieState state)
        {
            if (this.WState == state) return;
            this.WState = state;
        }
    }
}