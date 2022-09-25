using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace MyResource.ScriptedPhysics
{
    internal class TorqueManager : BaseScript
    {
        Vehicle pVehicle;

        List<float> Mults = new List<float>();
        Dictionary<string, int> boneToIndex = new Dictionary<string, int>();


        float ESCAggression = 0;
        float LCSAggression = 0;
        public TorqueManager()
        {
            Mults.Add(1f);
            boneToIndex.Add("wheel_lf", 0);
            boneToIndex.Add("wheel_rf", 1);
            boneToIndex.Add("wheel_lr", 2);
            boneToIndex.Add("wheel_rr", 3);
            


            AddCommands();
        }

        private void AddCommands()
        {
            //Only enable client commands if server allows
            if (GetConvarInt("lcs_allow_client_toggle", 1) == 1)
            {

                RegisterCommand("lcs", new Action<int, List<object>, object>((source, args, raw) =>
                {
                    if (args.Count == 1)
                    {
                        if (args[0].ToString() == "on") LCSAggression = 1;
                        if (args[0].ToString() == "off") LCSAggression = 0;

                    }
                    else { if (LCSAggression == 1) LCSAggression = 0; else LCSAggression = 1; }

                    if (LCSAggression == 1) Util.Notify("~b~Launch Control~w~ is ~g~enabled~w~.");
                    else Util.Notify("~b~Launch Control~w~ is ~o~disabled~w~.");

                }), false);

                TriggerEvent("chat:addSuggestion", "/lcs", "Manage the Launch Control System.", new[]
                {
                    new { name="(nothing)/on/off", help="Toggle / enable / disable." },                
                });
            }

            if (GetConvarInt("esc_allow_client_toggle", 1) == 1)
            {

                TriggerEvent("chat:addSuggestion", "/esc", "Manage the Electronic Stability System.", new[]
                {
                    new { name="(nothing)/on/off/XX", help="Toggle / enable / disable / finetune." },
                });

                RegisterCommand("esc", new Action<int, List<object>, object>((source, args, raw) =>
                {
                    if (args.Count == 1)
                    {
                        if (args[0].ToString() == "on") ESCAggression = 1;
                        if (args[0].ToString() == "off") ESCAggression = 0;
                    }
                    else { if (ESCAggression == 1) ESCAggression = 0; else ESCAggression = 1; }


                    float parsed = 0;
                    if (args.Count == 1 && float.TryParse(args[0].ToString(), out parsed))
                    {
                        if (parsed <= 0) parsed = 0;
                        if (parsed > 100) parsed = 100;

                        ESCAggression = parsed / 100;
                        Util.Notify("~b~ESC~w~ is at ~g~" + parsed + "%~w~.");


                    }
                    else
                    {
                        if (ESCAggression == 1) Util.Notify("~b~ESC~w~ is ~g~enabled~w~.");
                        else Util.Notify("~b~ESC~w~ is ~o~disabled~w~.");
                    }


                }), false);
            }
        }

        VehicleClass[] InnapropiateToManage = { VehicleClass.Boats, VehicleClass.Helicopters, VehicleClass.Planes, VehicleClass.Cycles, VehicleClass.Motorcycles };

        [Tick]
        public Task OnTick()
        {
            if (Game.Player == null) return Task.FromResult(0);
            if (Game.PlayerPed.SeatIndex == VehicleSeat.Driver) pVehicle = Game.PlayerPed.CurrentVehicle;


            if (API.GetConvarInt("it_on", 0) == 0) return Task.FromResult(0);

            float finalMult = 1;
            foreach (float m in Mults)
            {
                finalMult *= m;
            }

            if (pVehicle!=null)
            {
                if (InnapropiateToManage.Contains(pVehicle.ClassType)) return Task.FromResult(0);

                HandleTorqueControl(Game.PlayerPed.CurrentVehicle);


                if (finalMult != 1.0f)
                {
                    Game.PlayerPed.CurrentVehicle.EngineTorqueMultiplier = finalMult;
                }

            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Calculates the intended torque multiplier.
        /// </summary>
        /// <param name="v"></param>
        private void HandleTorqueControl(Vehicle v)
        {
            if (v.CurrentGear <= 0 || Game.GetControlNormal(0, Control.VehicleAccelerate) == 0f) return;

            float angleScale = (float)(API.GetConvarInt("it_mult_at_trlat_end", 300) / 100);
            float trlatStart = (float)(API.GetConvarInt("it_trlat_start", 50)) / 100;
            float trlatEnd = (float)(API.GetConvarInt("it_trlat_end", 150)) / 100;
            float trlat = API.GetVehicleHandlingFloat(v.Handle, "CHandlingData", "fTractionCurveLateral");
            if (trlat <= 0) trlat = 22; //Vanilla default

            float slideAngle = (float)Math.Round(Math.Abs(Util.AngleBetween(Util.Normalized(v.Velocity), v.ForwardVector)), 3);
            float treshold = trlat * trlatStart;

            //LCS and ECS are mutually exclusive.
            //If the vehicle is going straight LCS engages. Otherwise, ECS does.
            //TODO - Mix them through a gradual treshold.

            //Launch Control System
            //TODO - AWD tweaks - Make LCS Agression level configurable (like ESC is right now)
            if (Math.Abs(slideAngle) < treshold || v.Velocity.Length() < 0.25f)
            {
                Mults[0] = 1f;
                if (LCSAggression > 0f && v.HighGear > 1 )
                {
                    float SkidVectorLength = 0;

                    foreach (string r in boneToIndex.Keys)
                    {
                        int BoneIndex = API.GetEntityBoneIndexByName(v.Handle, r);

                        if (BoneIndex != -1)
                        {
                            SkidVectorLength += (float)Math.Round(API.GetVehicleWheelTractionVectorLength(v.Handle, boneToIndex[r]), 2) * 0.5f;
                        }
                    }
                    if (v.CurrentGear == 1)
                    {
                        float X = Util.Map(SkidVectorLength, 8, 2, 0.25f, 1, true);
                        Mults[0] = X;
                    }
                }
            }
            else //Inverse Torque
            {
                Mults[0] = 1f;
                if (ESCAggression < 1f)
                {
                    float AWDPenalty = Util.Map(API.GetVehicleHandlingFloat(v.Handle, "CHandlingData", "fDriveBiasFront"), 0.5f, 0f, 1 - (API.GetConvarInt("it_awd_penalty", 0) / 100), 1, true);                    
                    float multCap = (float)(API.GetConvarInt("it_max_mult", 200) / 100) - 1;
                    
                    float final = 1 + (float)Math.Round(Util.Map(slideAngle, trlat * trlatStart, trlat * trlatEnd, 0.0f, angleScale, false), 3); //Main slide math
                    final *= AWDPenalty;                    
                    final = Util.Clamp(final, 0, multCap) * (1 - ESCAggression);  //Cap and Electronic Stability Control modifier

                    //Limit the multiplier when at low speeds to avoid slide glitches at 0.001mph
                    final = Util.Clamp(final, 0, Util.Map(Util.MStoMPH(v.Velocity.Length()), 0.0f, 5f, 0.0f, 99f, true)); 

                    Mults[0] = final;
                }
            }
        }
    }
}
