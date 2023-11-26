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
        private Vehicle pVehicle;
        private Dictionary<string, int> boneToIndex = new Dictionary<string, int>();
        private VehicleClass[] InnapropiateToManage = { VehicleClass.Boats, VehicleClass.Helicopters, VehicleClass.Planes, VehicleClass.Cycles, VehicleClass.Motorcycles };
        
        private float PowerslideAggression = 1;

        public TorqueManager()
        {
            boneToIndex.Add("wheel_lf", 0);
            boneToIndex.Add("wheel_rf", 1);
            boneToIndex.Add("wheel_lr", 2);
            boneToIndex.Add("wheel_rr", 3);

            AddCommands();
            EventHandlers["gameEventTriggered"] += new Action<string, List<dynamic>>(OnGameEventTriggered);

        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            if (name == "CEventNetworkPlayerEnteredVehicle")
            {
            }
        }

        private void AddCommands()
        {
            if (GetConvarInt("powerslides_allow_client_toggle", 1) == 1)
            {
                TriggerEvent("chat:addSuggestion", "/powerslides", "Manage the Powerslides System.", new[]
                {
                    new { name="(nothing)/on/off/XX", help="Toggle / enable / disable / finetune." },
                });

                RegisterCommand("powerslides", new Action<int, List<object>, object>((source, args, raw) =>
                {
                    if (args.Count == 1)
                    {
                        if (args[0].ToString() == "on") PowerslideAggression = 1;
                        if (args[0].ToString() == "off") PowerslideAggression = 0;
                    }
                    else { if (PowerslideAggression == 1) PowerslideAggression = 0; else PowerslideAggression = 1; }

                    float parsed = 0;
                    if (args.Count == 1 && float.TryParse(args[0].ToString(), out parsed))
                    {
                        if (parsed <= 0) parsed = 0;
                        if (parsed > 100) parsed = 100;

                        PowerslideAggression = parsed / 100;
                        Util.Notify("~b~Powerslides~w~ is at ~g~" + PowerslideAggression * 100 + "%~w~.");
                    }
                    else
                    {
                        if (PowerslideAggression == 0) Util.Notify("~b~Powerslides~w~ is ~o~disabled~w~.");
                        else Util.Notify("~b~Powerslides~w~ is ~g~enabled~w~ at " + PowerslideAggression *100 + "%");
                    }
                }), false);
            }
        }



        [Tick]
        public Task OnTick()
        {
            if (Game.Player == null) return Task.FromResult(0);
                        
            if (Game.PlayerPed.SeatIndex == VehicleSeat.Driver) pVehicle = Game.PlayerPed.CurrentVehicle;

            if (pVehicle != null)
            {
                if (InnapropiateToManage.Contains(pVehicle.ClassType)) return Task.FromResult(0);

                float finalMult = (GetTorqueMultFromSlide(pVehicle)) * GetTractionControlMult(pVehicle) * ProcessOffroadBoost(pVehicle);                
                if (finalMult != 1.000f)
                {
                    pVehicle.EngineTorqueMultiplier = Util.Clamp(finalMult, 0.01f, 100f);
                }
            }
            return Task.FromResult(0);
        }


        Dictionary<string, float> MaterialToDrag = new Dictionary<string, float>
        {
            { "SAND_LOOSE_18", 0.115f},
            { "SAND_COMPACT_19", 0.08f},
            { "SAND_TRACK_21", 0.06f},
            { "SNOW_LOOSE_27", 0.06f},

            { "GRAVEL_SMALL_31", 0.04f},
            { "GRAVEL_LARGE_32", 0.06f},

            { "DIRT_TRACK_35", 0.03f},
            { "MUD_HARD_36", 0.02f},
            { "SOIL_43", 0.08f},
            { "CLAY_HARD_44", 0.02f},
            { "GRASS_LONG_46", 0.03f},
            { "GRASS_47", 0.02f},
            { "GRASS_SHORT_48", 0.01f},
            { "SAND_WET_DEEP_24", 0.115f},
            { "SAND_DRY_DEEP_23", 0.13f},
        };
        Dictionary<int, string> materials = new Dictionary<int, string>();
        List<string> mats = new List<string>
        {
            "DEFAULT_0",
"CONCRETE_1",
"CONCRETE_POTHOLE_2",
"CONCRETE_DUSTY_3",
"TARMAC_4",
"TARMAC_PAINTED_5",
"TARMAC_POTHOLE_6",
"RUMBLE_STRIP_7",
"BREEZE_BLOCK_8",
"ROCK_9",
"ROCK_MOSSY_10",
"STONE_11",
"COBBLESTONE_12",
"BRICK_13",
"MARBLE_14",
"PAVING_SLAB_15",
"SANDSTONE_SOLID_16",
"SANDSTONE_BRITTLE_17",
"SAND_LOOSE_18",
"SAND_COMPACT_19",
"SAND_WET_20",
"SAND_TRACK_21",
"SAND_UNDERWATER_22",
"SAND_DRY_DEEP_23",
"SAND_WET_DEEP_24",
"ICE_25",
"ICE_TARMAC_26",
"SNOW_LOOSE_27",
"SNOW_COMPACT_28",
"SNOW_DEEP_29",
"SNOW_TARMAC_30",
"GRAVEL_SMALL_31",
"GRAVEL_LARGE_32",
"GRAVEL_DEEP_33",
"GRAVEL_TRAIN_TRACK_34",
"DIRT_TRACK_35",
"MUD_HARD_36",
"MUD_POTHOLE_37",
"MUD_SOFT_38",
"MUD_UNDERWATER_39",
"MUD_DEEP_40",
"MARSH_41",
"MARSH_DEEP_42",
"SOIL_43",
"CLAY_HARD_44",
"CLAY_SOFT_45",
"GRASS_LONG_46",
"GRASS_47",
"GRASS_SHORT_48",
"HAY_49",
"BUSHES_50",
"TWIGS_51",
"LEAVES_52",
"WOODCHIPS_53",
"TREE_BARK_54",
"METAL_SOLID_SMALL_55",
"METAL_SOLID_MEDIUM_56",
"METAL_SOLID_LARGE_57",
"METAL_HOLLOW_SMALL_58",
"METAL_HOLLOW_MEDIUM_59",
"METAL_HOLLOW_LARGE_60",
"METAL_CHAINLINK_SMALL_61",
"METAL_CHAINLINK_LARGE_62",
"METAL_CORRUGATED_IRON_63",
"METAL_GRILLE_64",
"METAL_RAILING_65",
"METAL_DUCT_66",
"METAL_GARAGE_DOOR_67",
"METAL_MANHOLE_68",
"WOOD_SOLID_SMALL_69",
"WOOD_SOLID_MEDIUM_70",
"WOOD_SOLID_LARGE_71",
"WOOD_SOLID_POLISHED_72",
"WOOD_FLOOR_DUSTY_73",
"WOOD_HOLLOW_SMALL_74",
"WOOD_HOLLOW_MEDIUM_75",
"WOOD_HOLLOW_LARGE_76",
"WOOD_CHIPBOARD_77",
"WOOD_OLD_CREAKY_78",
"WOOD_HIGH_DENSITY_79",
"WOOD_LATTICE_80",
"CERAMIC_81",
"ROOF_TILE_82",
"ROOF_FELT_83",
"FIBREGLASS_84",
"TARPAULIN_85",
"PLASTIC_86",
"PLASTIC_HOLLOW_87",
"PLASTIC_HIGH_DENSITY_88",
"PLASTIC_CLEAR_89",
"PLASTIC_HOLLOW_CLEAR_90",
"PLASTIC_HIGH_DENSITY_CLEAR_91",
"FIBREGLASS_HOLLOW_92",
"RUBBER_93",
"RUBBER_HOLLOW_94",
"LINOLEUM_95",
"LAMINATE_96",
"CARPET_SOLID_97",
"CARPET_SOLID_DUSTY_98",
"CARPET_FLOORBOARD_99",
"CLOTH_100",
"PLASTER_SOLID_101",
"PLASTER_BRITTLE_102",
"CARDBOARD_SHEET_103",
"CARDBOARD_BOX_104",
"PAPER_105",
"FOAM_106",
"FEATHER_PILLOW_107",
"POLYSTYRENE_108",
"LEATHER_109",
"TVSCREEN_110",
"SLATTED_BLINDS_111",
"GLASS_SHOOT_THROUGH_112",
"GLASS_BULLETPROOF_113",
"GLASS_OPAQUE_114",
"PERSPEX_115",
"CAR_METAL_116",
"CAR_PLASTIC_117",
"CAR_SOFTTOP_118",
"CAR_SOFTTOP_CLEAR_119",
"CAR_GLASS_WEAK_120",
"CAR_GLASS_MEDIUM_121",
"CAR_GLASS_STRONG_122",
"CAR_GLASS_BULLETPROOF_123",
"CAR_GLASS_OPAQUE_124",
"WATER_125",
"BLOOD_126",
"OIL_127",
"PETROL_128",
"FRESH_MEAT_129",
"DRIED_MEAT_130",
"EMISSIVE_GLASS_131",
"EMISSIVE_PLASTIC_132",
"VFX_METAL_ELECTRIFIED_133",
"VFX_METAL_WATER_TOWER_134",
"VFX_METAL_STEAM_135",
"VFX_METAL_FLAME_136",
"PHYS_NO_FRICTION_137",
"PHYS_GOLF_BALL_138",
"PHYS_TENNIS_BALL_139",
"PHYS_CASTER_140",
"PHYS_CASTER_RUSTY_141",
"PHYS_CAR_VOID_142",
"PHYS_PED_CAPSULE_143",
"PHYS_ELECTRIC_FENCE_144",
"PHYS_ELECTRIC_METAL_145",
"PHYS_BARBED_WIRE_146",
"PHYS_POOLTABLE_SURFACE_147",
"PHYS_POOLTABLE_CUSHION_148",
"PHYS_POOLTABLE_BALL_149",
"BUTTOCKS_150",
"THIGH_LEFT_151",
"SHIN_LEFT_152",
"FOOT_LEFT_153",
"THIGH_RIGHT_154",
"SHIN_RIGHT_155",
"FOOT_RIGHT_156",
"SPINE0_157",
"SPINE1_158",
"SPINE2_159",
"SPINE3_160",
"CLAVICLE_LEFT_161",
"UPPER_ARM_LEFT_162",
"LOWER_ARM_LEFT_163",
"HAND_LEFT_164",
"CLAVICLE_RIGHT_165",
"UPPER_ARM_RIGHT_166",
"LOWER_ARM_RIGHT_167",
"HAND_RIGHT_168",
"NECK_169",
"HEAD_170",
"ANIMAL_DEFAULT_171",
"CAR_ENGINE_172",
"PUDDLE_173",
"CONCRETE_PAVEMENT_174",
"BRICK_PAVEMENT_175",
"PHYS_DYNAMIC_COVER_BOUND_176",
"VFX_WOOD_BEER_BARREL_177",
"WOOD_HIGH_FRICTION_178",
"ROCK_NOINST_179",
"BUSHES_NOINST_180",
"METAL_SOLID_ROAD_SURFACE_181",
"TEMP_01_182",
"TEMP_02_183",
"TEMP_03_184",
"TEMP_04_185",
"TEMP_05_186",
"TEMP_06_187",
"TEMP_07_188"
        };
        private Dictionary<int, float[]> gearRatios = new Dictionary<int, float[]>
{
    { 1, new float[] { 0.9f } },
    { 2, new float[] { 3.333f, 0.9f } },
    { 3, new float[] { 3.333f, 1.565f, 0.9f } },
    { 4, new float[] { 3.333f, 1.826f, 1.222f, 0.9f } },
    { 5, new float[] { 3.333f, 1.934f, 1.358f, 1.054f, 0.9f } },
    { 6, new float[] { 3.333f, 1.949f, 1.392f, 1.095f, 0.946f, 0.9f } }
};
        float ExtraPowerOffroad = 1f;
        DateTime LastTorqueUpdate = DateTime.MinValue;
        private float ProcessOffroadBoost(Vehicle veh)
        {
            if ((int)API.GetConvarInt("sp_enable_offroad_boost", 1) == 0) { return 1; }

            if (veh != null)
            {
                if (materials.Count == 0)
                {
                    int i = 0;
                    foreach (string s in mats) { materials.Add(i, s); i++; };
                }

                int m = API.GetVehicleWheelSurfaceMaterial(veh.Handle, 0);
                if (materials[m] == null) { return 1; }

                float CurrentPower = 0;
                if (veh.HighGear < 7 && gearRatios.ContainsKey(veh.HighGear))
                {
                    float[] ratios = gearRatios[veh.HighGear];
                    if (veh.CurrentGear > 0)
                    {
                        CurrentPower = (float)Math.Round(API.GetVehicleAcceleration(veh.Handle) * ratios[veh.CurrentGear - 1], 3);
                    }
                }

                float TopSpeed = (API.GetVehicleEstimatedMaxSpeed(veh.Handle));
                float VehicleVelocity = (veh.Velocity.Length());

                if (VehicleVelocity > 1 && veh.CurrentGear > 0)
                {
                    if (1 == 1)
                    {
                        float TotalDrag = 0f;
                        if (VehicleVelocity > 0)
                        {
                            float tireDrag = 0;
                            if (MaterialToDrag.ContainsKey(materials[m]))
                            {
                                tireDrag = MaterialToDrag[materials[m]];
                            }
                            if (tireDrag == 0 || Game.IsControlPressed(0, Control.Context)) return 1f;


                            TotalDrag = tireDrag * API.GetVehicleNumberOfWheels(veh.Handle);
                        }

                        float DragMinusPower = TotalDrag - CurrentPower;

                        float ExtraSpeedPastSeventyPercent = (VehicleVelocity - (TopSpeed * 0.7f)) * 0.05f;
                        ExtraSpeedPastSeventyPercent *= 1 + API.GetVehicleHandlingFloat(veh.Handle, "CHandlingData", "fTractionLossMult");


                        ExtraPowerOffroad = 1f;
                        if (DragMinusPower > 0.00f) ExtraPowerOffroad += DragMinusPower;
                        if (ExtraSpeedPastSeventyPercent > 0f) ExtraPowerOffroad += (ExtraSpeedPastSeventyPercent);

                        if (ExtraPowerOffroad > 1.001f)
                        {
                            return (float)Math.Round(ExtraPowerOffroad, 3);
                        }
                    }
                }
            }
            return 1;
        }


        float slideAngle = 1;
        /// <summary>
        /// Calculates the intended torque multiplier.
        /// </summary>
        /// <param name="v"></param>
        private float GetTorqueMultFromSlide(Vehicle v)
        {

            if ((int)API.GetConvarInt("powerslides_on", 1) == 0) { return 1; }

            if (v == null) return 1;
            if (v.CurrentGear <= 0) return 1;

            float trlatStart = (float)(API.GetConvarInt("powerslides_trlat_start", 20)) / 100;
            float trlat = API.GetVehicleHandlingFloat(v.Handle, "CHandlingData", "fTractionCurveLateral");
            if (trlat <= 0) trlat = 22; //Vanilla default, kinda

            slideAngle = (float)Math.Round(Math.Abs(Util.AngleBetween(Util.Normalized(v.Velocity), v.ForwardVector)), 1);
            float treshold = trlat * trlatStart;

            float scale = (float)(API.GetConvarInt("powerslides_scale", 100)) *0.001f;

            if (PowerslideAggression > 0f && v.Model.IsCar && Math.Abs(slideAngle) > treshold)
            {                               
                float MaxMultiplier = (float)(API.GetConvarInt("powerslides_max_mult", 800) / 100);

                float final = (float)Math.Round(1 + ((Math.Abs(slideAngle) - treshold) * scale), 1) * PowerslideAggression;
                final = Util.Clamp(final, 1, MaxMultiplier); 

                if (v.CurrentGear == 1 && final > 2) final = 2;
               return final;
            }
            return 1;
        }
        bool TC = false;

        float GetTractionControlMult(Vehicle v)
        {
            if (!TC) return 1;

            if (API.GetVehicleHandlingFloat(v.Handle, "CHandlingData", "fDriveBiasFront") == 0f)
            {
                float pwr = 0;
                int poweredWheels = 0;
                int nWheels = API.GetVehicleNumberOfWheels(v.Handle);
                for (int i = 0; i < nWheels; i++)
                {
                    float p = API.GetVehicleWheelPower(v.Handle, i);
                    if (p > 0f) poweredWheels++;
                    pwr += p;
                }

                foreach (string r in boneToIndex.Keys)
                {
                    int BoneIndex = API.GetEntityBoneIndexByName(v.Handle, r);
                    if (BoneIndex != -1)
                    {
                        float steer = API.GetVehicleWheelSteeringAngle(v.Handle, boneToIndex[r]);
                        if (steer == 0.00f && TC)
                        {
                            if (Math.Abs(slideAngle) > 20f)
                            {
                                float mintraction = API.GetVehicleHandlingFloat(v.Handle, "CHandlingData", "fTractionCurveMin") / 4;

                                float targetPower = Util.Map(Math.Abs(slideAngle), 60f, 20f, mintraction, pwr, true);
                                if (targetPower < pwr)
                                {
                                    return targetPower / pwr;
                                }
                            }
                        }
                        //SkidVectorLength += (float)Math.Round(API.GetVehicleWheelTractionVectorLength(v.Handle, boneToIndex[r]), 2) * 0.5f;
                    }
                }
                //if(TC) Util.DisplayHelpTextTimed("~y~Power: " + Math.Round(pwr * Mults[0], 1).ToString(), 10);
                // else Util.DisplayHelpTextTimed("~g~Power: " + Math.Round(pwr * Mults[0], 1).ToString(), 10);

            }
            return 1;
        }


        float RampUp = 0f;
        float KERSFill = 0;
        float KERSMax = 1;
        private void ProcessKERS(Vehicle v)
        {

            SetAllowAbilityBarInMultiplayer(true);
            SetAbilityBarVisibilityInMultiplayer(true);
            SetAbilityBarValue(KERSFill, KERSMax);

            if (KERSFill < KERSMax && !IsVehicleInBurnout(v.Handle) && v.Velocity.Length() > 1)
            {
                float totalPressure = 0;
                foreach (string r in boneToIndex.Keys)
                {
                    int BoneIndex = API.GetEntityBoneIndexByName(v.Handle, r);
                    if (BoneIndex != -1 && GetVehicleWheelTractionVectorLength(v.Handle, boneToIndex[r]) < 3)
                    {
                        totalPressure += (float)(API.GetVehicleWheelBrakePressure(v.Handle, boneToIndex[r])) * 0.1f * Game.LastFrameTime;
                    }
                }

                if (totalPressure > 0.004f) totalPressure = 0.004f;
                KERSFill += totalPressure;
            }

            if (KERSFill > 0.001f && Game.IsControlPressed(2, Control.Sprint))
            {
                RampUp += 2.5f * Game.LastFrameTime;
                if (RampUp > 1) RampUp = 1;

                v.EngineTorqueMultiplier = Util.Map(KERSFill, 0, KERSMax / 2, 1, 2, true) * RampUp;
                KERSFill -= 0.2f * Game.LastFrameTime;
            }
            else if (RampUp >= 0) RampUp = 0;
        }


    }
}