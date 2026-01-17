using BepInEx;
using HarmonyLib;
using Orion.FrontMission.Config;
using System.Reflection;
using static Orion.FrontMission.Utils.Utils;

namespace Orion.FrontMission
{
    [BepInPlugin("Orion.FrontMission.Mod", "Orion.FrontMission", "1.0.0")]
    public class OrionFrontMissionPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            ModLog.Log = Logger;
            Configs.ReadConfig(Config);
            var harmony = new Harmony("Orion.FrontMission.Harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //ApplyFastSkillLevel(Configs.FastSkillLevel.Value);
            ModLog.Info("Orion.FrontMission loaded.");
        }
    }
}