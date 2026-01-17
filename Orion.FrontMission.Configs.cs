using BepInEx.Configuration;

namespace Orion.FrontMission.Config
{
    internal static class Configs
    {
        internal static ConfigEntry<bool> PlayerAlwaysHit;
        internal static ConfigEntry<float> EXPMultiplier;
        internal static ConfigEntry<bool> GodMode;
        internal static ConfigEntry<int> MoveSpeed;
        internal static ConfigEntry<bool> FreeCost;
        internal static ConfigEntry<bool> InfiniteAmmo;
        internal static ConfigEntry<bool> DebugMode;
        internal static ConfigEntry<bool> ForceSkills;
        internal static ConfigEntry<bool> FastSkillLevel;
        //internal static ConfigEntry<bool> SkillsAlwaysProc;
        internal static ConfigEntry<bool> Regenerate;


        internal static void ReadConfig(ConfigFile Config)
        {
            PlayerAlwaysHit = Config.Bind("Cheats", "PlayerAlwaysHit", true, "Player attacks always hit");
            EXPMultiplier = Config.Bind<float>("Cheats", "EXPMultiplier", -1, "Multiply EXP gain by a factor. -1.0 to disable");
            GodMode = Config.Bind("Cheats", "GodMode", false, "Enable God Mode");
            MoveSpeed = Config.Bind("Cheats", "MoveSpeed", 0, "Set Move Speed. 0 to disable");
            FreeCost = Config.Bind("Cheats", "FreeCost", false, "All shop wares cost 0 credits");
            InfiniteAmmo = Config.Bind("Cheats", "InfiniteAmmo", false, "Player weapons have infinite ammo");
            DebugMode = Config.Bind("Debug", "DebugMode", false, "Enable debug logging");
            ForceSkills = Config.Bind("Cheats", "ForceSkills", false, "Enable all skills for player pilots");
            FastSkillLevel = Config.Bind("Cheats", "FastSkillLevel", true, "Instant skill level up rate");
            //SkillsAlwaysProc = Config.Bind("Cheats", "SkillsAlwaysProc", true, "Player skills always proc");
            Regenerate = Config.Bind("Cheats", "Regenerate", false, "Player wanzers regenerate HP each turn");
        }
    }
}
