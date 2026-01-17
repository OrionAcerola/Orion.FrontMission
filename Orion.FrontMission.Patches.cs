using HarmonyLib;
using Orion.FrontMission.Config;
using Orion.FrontMission.Utils;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Walker;
using Walker.CombatReverse;
using Walker.Data;
using static Orion.FrontMission.Utils.Utils;

namespace Orion.FrontMission.Patches
{
    internal static class CombatReversePatches
    {
        [HarmonyPatch(typeof(CombatReverse), "HitCheck")]
        internal static class Patch_HitCheck
        {
            private static readonly AccessTools.FieldRef<CombatReverse, Machine> AttackerRef =
                AccessTools.FieldRefAccess<CombatReverse, Machine>("machineAttacker");
            [HarmonyPrefix]
            static bool HitCheck_Prefix(CombatReverse __instance, ref bool __result)
            {
                var machine = AttackerRef(__instance);
                var wanzer = machine.Wanzer;

                if (!Configs.PlayerAlwaysHit.Value || !wanzer.IsPlayer()) return true;
                __result = true;
                if (Configs.DebugMode.Value)
                    ModLog.Info($"HitCheck: Pilot={machine.Pilot.GetName()}");
                return false;
            }
        }

        [HarmonyPatch(typeof(CombatReverse), "DamageEnemy")]
        internal static class Patch_DamageEnemy
        {
            private static readonly AccessTools.FieldRef<CombatReverse, Machine> AttackerRef =
                AccessTools.FieldRefAccess<CombatReverse, Machine>("machineAttacker");
            private static readonly AccessTools.FieldRef<CombatReverse, Machine> DefenderRef =
                AccessTools.FieldRefAccess<CombatReverse, Machine>("machineDefender");
            [HarmonyPrefix]
            static void DamageEnemy_Prefix(CombatReverse __instance)
            {
                if (!Configs.PlayerAlwaysHit.Value) return;
                var atk = AttackerRef(__instance);
                var def = DefenderRef(__instance);
                AlwaysHitContext.InDamageEnemy = true;
                AlwaysHitContext.Defender = def;
                AlwaysHitContext.AttackerIsPlayer = atk?.Wanzer != null && atk.Wanzer.IsPlayer();
            }
            [HarmonyPostfix]
            private static void Postfix()
            {
                AlwaysHitContext.InDamageEnemy = false;
                AlwaysHitContext.AttackerIsPlayer = false;
                AlwaysHitContext.Defender = null;
            }
        }

        [HarmonyPatch(typeof(CombatReverse), "callMachineMethod")]
        internal static class Patch_callMachineMethod_ForceDPOS_ToAPS
        {
            [HarmonyPostfix]
            static void Postfix(Machine machine, MachineMethodID method, object[] optionalParameters, ref object __result)
            {
                // Only intervene in DamageEnemy flow, and only for player attacks
                if (!Configs.PlayerAlwaysHit.Value) return;
                if (!AlwaysHitContext.InDamageEnemy || !AlwaysHitContext.AttackerIsPlayer) return;
                // We only care about the defender's DPOS roll (target part selection)
                if (method != MachineMethodID.DPOS) return;
                var def = AlwaysHitContext.Defender;
                if (def == null || machine != def) return;
                // If "target mode" is active (APS set), force DPOS to be APS.
                var aps = def.APS;
                if (aps == PartPositionID.None_POS__ || aps == PartPositionID.POS_NH) return;
                __result = aps; // enum boxed as object; matches (PartPositionID) cast in DamageEnemy
                if (Configs.DebugMode.Value)
                    ModLog.Info($"ForceDPOS: APS={aps} Defender={def.Pilot.GetName()}");
            }
        }

        [HarmonyPatch(typeof(UIWeaponInfoPanel), "PrepareData")]
        internal static class Patch_UIWeaponInfoPanel
        {
            [HarmonyPostfix]
            static void PrepareData_Postfix(UIWeaponInfoPanel __instance, Weapon weapon, Weapon enemyWeapon, Walker.Wanzer wanzer, Walker.Wanzer enemy)
            {
                if (!Configs.PlayerAlwaysHit.Value || weapon.WeaponInfo.IsShield) return;
                var hitText = __instance.m_Hitvalue;
                hitText.text = "100";
            }
        }

        //[HarmonyPatch(typeof(CombatReverse), "PS_SKLC")]
        //internal static class Patch_PS_SKLC
        //{
        //    private static readonly MethodInfo ratio_Cor =
        //        AccessTools.Method(typeof(CombatReverse), "_ratio_cor", [typeof(Machine), typeof(ushort)]);
        //    private static readonly MethodInfo invokeSkill =
        //        AccessTools.Method(typeof(CombatReverse), "InvokeSkill", [typeof(Machine), typeof(Skill)]);
        //    [HarmonyPrefix]
        //    static bool PS_SKLC_Prefix(CombatReverse __instance, Machine machine, ref bool __result)
        //    {
        //        if (!Configs.FastSkillLevel.Value) return true; // <-- run original
        //        var wanzer = machine.Wanzer;
        //        var machineStatus = machine.MachineStatus;
        //        var arg0_AG = (TriggerTimingFlag)machine.Arg0_AG0;
        //        __instance.sequenceMoved = false;
        //        var skills = machineStatus.State.PilotStatus.skills;
        //        for (int i = 0; i < skills.Count; i++)
        //        {
        //            if (skills[i].Definition.Skill != SkillType.PI_SKL_NONE && arg0_AG == (TriggerTimingFlag)skills[i].Definition.ActivationTime && WeaponData.Weapons[machine.WeaponID_WEP].EXM == (PilotSkillExperienceType)skills[i].Definition.AttackType)
        //            {
        //                ushort num = (ushort)ratio_Cor.Invoke(__instance, [machine, skills[i].Definition.ActivationRate]);
        //                if (CombatReverse.GetRandom(10000) < (int)num)
        //                {
        //                    if (wanzer.IsPlayer())
        //                    {
        //                        if (Configs.DebugMode.Value)
        //                        {
        //                            var def = skills[i].Definition;
        //                            ModLog.Info($"Skill Level Up: Pilot={machine.Pilot.GetName()} Skill={def.Skill} LevelUpRate={def.LevelUpRate} ActivationRate={def.ActivationRate}");
        //                        }
        //                        __instance.CurrentSkillLevelUp = true;
        //                        skills[i].LevelUp();
        //                    }
        //                    else
        //                    {
        //                        __instance.CurrentSkillLevelUp = false;
        //                    }
        //                    if ((bool)invokeSkill.Invoke(__instance, [machine, skills[i]]))
        //                    {
        //                        __instance.sequenceMoved = true;
        //                    }
        //                    __result = __instance.sequenceMoved;
        //                    return false; // <-- skip original
        //                }
        //            }
        //        }
        //        __result = false;
        //        return false; // <-- skip original
        //    }
        //}

        //[HarmonyPatch(typeof(CombatReverse), "_ratio_cor")]
        //internal static class Patch_SkillsAlwaysProc
        //{
        //    [HarmonyPrefix]
        //    static bool SkillsAlwaysProc_Prefix(Machine machine, int activationRate, ref ushort __result)
        //    {
        //        var wanzer = machine.Wanzer;
        //        if (!Configs.SkillsAlwaysProc.Value || !wanzer.IsPlayer()) return true;
        //        var oldValue = __result;
        //        __result = 10000;
        //        if (Configs.DebugMode.Value)
        //            ModLog.Info($"SkillProc: Pilot={machine.Pilot.GetName()} Result={oldValue}=>{__result}");
        //        return false;
        //    }
        //}

        [HarmonyPatch(typeof(CombatReverse), "AddExperience")]
        internal static class Patch_AddExperience
        {
            [HarmonyPrefix]
            static void AddExperience_Prefix(Machine machine, PilotSkillExperienceType expType, ref ushort expVal)
            {
                var wanzer = machine.Wanzer;
                if (!wanzer.IsPlayer()) return;
                float mult = Configs.EXPMultiplier.Value;
                if (mult < 0f || expVal == 0) return;
                ushort old = expVal;
                float boosted = old * mult;
                if (boosted > ushort.MaxValue)
                    boosted = ushort.MaxValue;
                if (boosted < 0f)
                    boosted = 0f;
                expVal = (ushort)boosted;
                if (Configs.DebugMode.Value)
                    ModLog.Info($"SkillXP Pilot={machine.Pilot.GetName()} Type={expType} ExpValue={old}->{expVal}");
            }
        }

        [HarmonyPatch(typeof(CombatReverse), "SetBulletsNow")]
        internal static class Patch_SetBulletsNow
        {
            [HarmonyPrefix]
            static void SetBulletsNow_Prefix(Machine machine, PartPositionID pos, ref ushort bullets)
            {
                if (!Configs.InfiniteAmmo.Value) return;
                var wanzer = machine.Wanzer;
                if (!wanzer.IsPlayer()) return;
                var state = machine.MachineStatus.State;
                WeaponPartsInfo wep = null;
                switch (pos)
                {
                    case PartPositionID.POS_BD:
                        wep = state.wepBody;
                        break;
                    case PartPositionID.POS_GL:
                        wep = state.wepGripL;
                        break;
                    case PartPositionID.POS_GR:
                        wep = state.wepGripR;
                        break;
                    case PartPositionID.POS_SL:
                        wep = state.wepShoulderL;
                        break;
                    case PartPositionID.POS_SR:
                        wep = state.wepShoulderR;
                        break;
                }
                if (wep == null || wep.Bul_Max == ushort.MaxValue || wep.Bul_Now == ushort.MaxValue) return;
                bullets = wep.Bul_Max;
                if (Configs.DebugMode.Value)
                    ModLog.Info($"Refill Ammo: Pilot={machine.Pilot.GetName()} Weapon={wep.Code} {wep.Bul_Now}->{bullets}");
            }
        }

        [HarmonyPatch(typeof(CombatReverse), "CheckSkillGet")]
        internal static class Patch_CheckSkillGet
        {
            [HarmonyPrefix]
            static bool CheckSkillGet_Prefix(CombatReverse __instance, PilotSkillExperienceType experienceType, ref bool __result)
            {
                if (!Configs.ForceSkills.Value || experienceType != PilotSkillExperienceType.Evasion_EVA) return true;
                var player = __instance.GetPlayerMachine();
                var state = player.MachineStatus.State;
                var pilot = state.Pilot;
                int used = state.PilotStatus.skills.Count;

                bool usePilotLimit = !Configs.PilotMaxSkillSlots.Value;
                int skillLimit = usePilotLimit
                    ? (int)pilot.m_MaxSkills
                    : PilotData.PILOT_SKILL_MAX;
                if (used >= skillLimit)
                {
                    __result = false;
                    return false;
                }
                var list = new List<SkillType>();
                if (!Configs.LearnAllSkills.Value)
                {
                    for (int i = 0; i < pilot.m_LearnableSkills.Length; i++)
                    {
                        var st = pilot.m_LearnableSkills[i];
                        if (st == SkillType.PI_SKL_NONE) continue;
                        var def = Skill_Def.GetSkill(st);
                        bool already = false;
                        for (int s = 0; s < state.PilotStatus.skills.Count; s++)
                            if (state.PilotStatus.skills[s].Definition.ActualType == def.ActualType)
                            {
                                already = true;
                                break;
                            }
                        if (already) continue;
                        list.Add(st);
                    }
                }
                else
                {
                    foreach (var root in AllRoots)
                    {
                        var def = Skill_Def.GetSkill(root);
                        bool already = false;
                        for (int s = 0; s < state.PilotStatus.skills.Count; s++)
                            if (state.PilotStatus.skills[s].Definition.ActualType == def.ActualType)
                            {
                                already = true;
                                break;
                            }
                        if (already) continue;
                        list.Add(root);
                    }
                }
                if (list.Count > 0)
                {
                    __instance.AddEvent(CombatEvent.CreateSkillAcquiredEvent(player.IsDefending_DAD, list));
                    __instance.SetUserInteraction(player);
                    __result = true;
                    return false;
                }
                __result = false;
                return false;
            }
        }
    }
    internal static class MoneyPatches
    {
        [HarmonyPatch(typeof(MissionInfo), "SubMoney")]
        internal static class Patch_SubMoney
        {
            private static readonly AccessTools.FieldRef<MissionInfo, int> myMoney = AccessTools.FieldRefAccess<MissionInfo, int>("m_Money");
            [HarmonyPrefix]
            static void SubMoney_Prefix(MissionInfo __instance, ref int value)
            {
                if (!Configs.FreeCost.Value || IsArenaContext.InArenaBet) return;
                value = 0;
                if (Configs.DebugMode.Value)
                    ModLog.Info($"My Money: {myMoney(__instance)} Cost={value}");
            }
        }

        [HarmonyPatch(typeof(UIArenaEnemySelectMenu), "CreateBetOnValue")]
        internal static class Patch_CreateBetOnValue
        {
            [HarmonyPrefix]
            static void CreateBetOnValue_Prefix()
            {
                IsArenaContext.InArenaBet = true;
            }
            [HarmonyFinalizer]
            static void CreateBetOnValue_Finalizer()
            {
                IsArenaContext.InArenaBet = false;
            }
        }
    }
    internal static class MiscPatches
    {
        [HarmonyPatch(typeof(ScenarioScene), "IsGodMode")]
        internal static class Patch_IsGodMode
        {
            [HarmonyPrefix]
            static bool IsGodMode_Prefix(ref bool __result)
            {
                if (Configs.GodMode.Value)
                {
                    __result = true;
                    if (Configs.DebugMode.Value)
                        ModLog.Info("GodMode ENABLED");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Walker.Wanzer), "OnTurnEnded")]
        internal static class Patch_Repair
        {
            private static PhaseManager pm;
            private static readonly MethodInfo supply_hp =
                AccessTools.Method(typeof(PhaseManager), "_supply_hp", [typeof(bool), typeof(Walker.Wanzer), typeof(PartPosition)]);
            [HarmonyPostfix]
            static void Repair_Postfix(Walker.Wanzer __instance)
            {
                if (!Configs.Regenerate.Value || !__instance.Data.IsPlayer()) return;
                if (pm == null)
                    pm = PhaseManager.Instance;
                if (pm == null) return;
                supply_hp.Invoke(pm, [true, __instance, PartPosition.Body]);
                supply_hp.Invoke(pm, [true, __instance, PartPosition.ArmLeft]);
                supply_hp.Invoke(pm, [true, __instance, PartPosition.ArmRight]);
                supply_hp.Invoke(pm, [true, __instance, PartPosition.LegLeft]);
                supply_hp.Invoke(pm, [true, __instance, PartPosition.LegRight]);
                if (Configs.DebugMode.Value)
                    ModLog.Info($"Repair: Pilot={__instance.Data.m_Pilot.GetName()}");
            }
        }

        [HarmonyPatch(typeof(Walker.Data.Wanzer), "SetupUnit")]
        internal static class Patch_MovementPoints
        {
            private static readonly AccessTools.FieldRef<Walker.Data.Wanzer, int> MovePts =
                AccessTools.FieldRefAccess<Walker.Data.Wanzer, int>("m_MovementPoints");
            [HarmonyPostfix]
            static void MovementPoints_Postfix(Walker.Data.Wanzer __instance)
            {
                if (Configs.MoveSpeed.Value <= 0 || !__instance.IsPlayer()) return;
                int configValue = Configs.MoveSpeed.Value;
                var wanzer = __instance;
                int oldValue = MovePts(wanzer);
                if (configValue > 999)
                    configValue = 999;
                if (wanzer.GetHP(PartPosition.LegLeft) <= 0 && wanzer.GetMaxHP(PartPosition.LegLeft) > 0)
                    configValue /= 2;
                MovePts(wanzer) = configValue;
                if (Configs.DebugMode.Value)
                    ModLog.Info($"MovementPoints SET: {wanzer.m_Pilot.GetName()} {oldValue} -> {configValue}");
            }
        }

        [HarmonyPatch(typeof(UIRootSkillController), "UpdateInfo")]
        internal static class Patch_UIRootSkillController_UpdateInfo
        {
            private static readonly AccessTools.FieldRef<UIRootSkillController, TextMeshProUGUI[]> m_allskils =
                AccessTools.FieldRefAccess<UIRootSkillController, TextMeshProUGUI[]>("m_allskils");
            private static readonly AccessTools.FieldRef<UIRootSkillController, LocalizedUIText[]> m_LocalizedAllSkills =
                AccessTools.FieldRefAccess<UIRootSkillController, LocalizedUIText[]>("m_LocalizedAllSkills");
            [HarmonyPostfix]
            static void UpdateInfo_Postfix(UIRootSkillController __instance, Walker.Data.Wanzer wanzer)
            {
                if (!EnabledSkillsContext.FastSkillLevelEnabled) return;
                var AllSkillsField = m_allskils(__instance);
                var LocalizedAllSkillsField = m_LocalizedAllSkills(__instance);
                for (int i = 0; i < AllSkillsField.Length; i++)
                {
                    if (wanzer.m_Skills.Count > i && wanzer.m_Skills[i] != null && wanzer.m_Skills[i].Definition.NextLevel != SkillType.PI_SKL_NONE)
                    {
                        while (wanzer.m_Skills[i].Definition.NextLevel != SkillType.PI_SKL_NONE)
                            wanzer.m_Skills[i].LevelUp();
                        LocalizedAllSkillsField[i].SetLabel(wanzer.m_Skills[i].GetName());
                        var def = wanzer.m_Skills[i].Definition;
                        if (Configs.DebugMode.Value)
                            ModLog.Info($"UIPilotStatusMenu: Pilot={wanzer.m_Pilot.GetName()} Skill={def.Skill} LevelUpRate={def.LevelUpRate} ActivationRate={def.ActivationRate} Level={def.Level}");
                    }
                }
                EnabledSkillsContext.FastSkillLevelEnabled = false;
            }

            [HarmonyPatch(typeof(UIPilotStatusMenu), "UpdateInfo")]
            internal static class Patch_UIPilotStatusMenu_UpdateInfo
            {
                [HarmonyPrefix]
                static void UpdateInfo_Prefix()
                {
                    if (!Configs.FastSkillLevel.Value) return;
                    if (EnabledSkillsContext.FastSkillLevelEnabled)
                        ModLog.Info("FastSkillLevelEnabled was still TRUE before re-arming!");
                    EnabledSkillsContext.FastSkillLevelEnabled = true;
                }
            }
        }
    }
}