using BepInEx.Logging;
using System.Runtime.CompilerServices;
using Walker.CombatReverse;
using Walker.Data;

namespace Orion.FrontMission.Utils
{
    internal static class Utils
    {
        internal static class ModLog
        {
            internal static ManualLogSource Log;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Info(object msg) => Log?.LogInfo(msg);
        }

        internal static bool IsPlayer(this Wanzer wanzer)
        {
            return wanzer != null && !wanzer.IsEnemy && !wanzer.IsAlly;
        }

        internal static class IsArenaContext
        {
            internal static bool InArenaBet;
        }

        //internal static void ApplyFastSkillLevel(bool enabled)
        //{
        //    if (!enabled) return;
        //    foreach (var def in Skill_Def.Skills)
        //    {
        //        if (def.Skill == SkillType.PI_SKL_NONE) continue;
        //        def.LevelUpRate = 10000;
        //    }
        //}
        internal static class EnabledSkillsContext
        {
            internal static bool FastSkillLevelEnabled;
        }

        internal static class AlwaysHitContext
        {
            [System.ThreadStatic] public static bool InDamageEnemy;
            [System.ThreadStatic] public static bool AttackerIsPlayer;
            [System.ThreadStatic] public static Machine Defender;
        }

        internal static readonly SkillType[] AllRoots =
            [
                SkillType.PI_SKL_FSTUN1,
                SkillType.PI_SKL_FCYCL1,
                SkillType.PI_SKL_SDUEL1,
                SkillType.PI_SKL_SCYCL1,
                SkillType.PI_SKL_SSPED1,
                SkillType.PI_SKL_DDUEL1,
                SkillType.PI_SKL_FFRST1,
            ];
    }
}
