using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace MyResource.ScriptedPhysics
{
    public class ClientMain : BaseScript
    {


        public string sp_nitro_enabled = "sp_nitro_enabled";
        public string sp_nitro_duration = "sp_nitro_duration";
        public string sp_nitro_cooldown = "sp_nitro_cooldown";


        public ClientMain()
        {


        }

        
        [Tick]
        public Task OnTick()
        {
            if(Game.Player.Character.CurrentVehicle!=null) ProcessNitroShots(Game.Player.Character.CurrentVehicle);
            return Task.FromResult(0);
        }

        private float Timer = 0;
        private bool FirstTime = false;
        private bool NitroActive = false;

        private void ProcessNitroShots(Vehicle v)
        {
            if (((int)API.GetConvarInt(sp_nitro_enabled, 0)) != 1) return;

            int NitroDurationSeconds = ((int)API.GetConvarInt(sp_nitro_duration, 4));
            int NitroCooldownSeconds = ((int)API.GetConvarInt(sp_nitro_cooldown, 4));

            //Rocket boost screws with nitro
            if (new string[] { "SCRAMJET", "TOREADOR", "VOLTIC2" }.Contains(v.DisplayName)) return;
            if (v.ClassType == VehicleClass.Planes || v.ClassType == VehicleClass.Helicopters) return;

            if (!HasNamedPtfxAssetLoaded("veh_xs_vehicle_mods")) RequestNamedPtfxAsset("veh_xs_vehicle_mods");

            if (Timer != 0)
            {
                float timeStat = (Timer - Game.GameTime) / 1000;

                if (timeStat < 0)
                {
                    if (NitroActive)
                    {
                        NitroActive = false;
                        Function.Call((Hash)0xC8E9B6B71B8E660D, v.Handle, false, 10f, 0f, 100f, true);
                    }
                }

                if (timeStat < -NitroCooldownSeconds)
                {
                    Timer = 0;
                    Util.DisplayHelpTextTimed("Nitrous ~g~ready~w~.", 1000);
                }
            }

            if (Game.IsEnabledControlJustPressed(2, Control.VehicleDuck))
            {
                if (Timer == 0)
                {
                    if (!FirstTime)
                    {
                        Function.Call((Hash)0xC8E9B6B71B8E660D, v.Handle, true, 0.15f, 0.5f, 100f, false);
                        FirstTime = true;
                    }
                    else
                    {
                        Function.Call((Hash)0xC8E9B6B71B8E660D, v.Handle, true, 1.25f, 0.5f, 100f, false);
                    }

                    NitroActive = true;
                    SetVehicleHudSpecialAbilityBarActive(v.Handle, false);
                    Game.SetControlNormal(2, Control.VehicleFlyTransform, 1);
                    Timer = Game.GameTime + (NitroDurationSeconds * 1000);
                }
                else Util.DisplayHelpTextTimed("Nitrous is ~o~not ready~w~.", 2000);
            }

            if (NitroActive)
            {
                //Game.SetControlNormal(2, Control.VehicleAccelerate, 255);
                //if (UseForce && v.IsOnAllWheels) API.ApplyForceToEntity(v.Handle, 3, 0.0f, 2 * Game.LastFrameTime, 0f, 0f, 0.5f, 0.0f, 0, true, true, true, false, false);
            }
        }
    }
}