using HarmonyLib;
using Orion.FrontMission.Config;
using Orion.FrontMission.Utils;
using System.Collections.Generic;
using TMPro;
using Walker;
using Walker.Data;
using UnityEngine;
using DG.Tweening;

namespace Orion.FrontMission.Ui
{
    internal static class SkillsUi
    {
        [HarmonyPatch(typeof(UIUpdateSkillPanel), "SetCurrentSkills")]
        internal static class Patch_UIUpdateSkillPanel_SetCurrentSkills
        {
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, TMP_Text[]> currentSkillsText = AccessTools.FieldRefAccess<UIUpdateSkillPanel, TMP_Text[]>("currentSkillsText");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, Walker.Data.Wanzer> m_Wanzer = AccessTools.FieldRefAccess<UIUpdateSkillPanel, Walker.Data.Wanzer>("m_Wanzer");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, LocalizedUIText[]> currentLocalizedSkillsText = AccessTools.FieldRefAccess<UIUpdateSkillPanel, LocalizedUIText[]>("currentLocalizedSkillsText");
            [HarmonyPrefix]
            static bool SetCurrentSkills_Prefix(UIUpdateSkillPanel __instance)
            {
                if (!Configs.PilotMaxSkillSlots.Value) return true;
                var CurrentSkillsTextField = currentSkillsText(__instance);
                var Wanzer = m_Wanzer(__instance);
                var CurrentLocalizedSkillsTextField = currentLocalizedSkillsText(__instance);
                int maxSkills = PilotData.PILOT_SKILL_MAX;
                for (int i = 0; i < maxSkills; i++)
                {
                    CurrentSkillsTextField[i].gameObject.SetActive(true);
                    if (i < Wanzer.Machine.MachineStatus.State.PilotStatus.skills.Count)
                    {
                        CurrentLocalizedSkillsTextField[i].SetLabel(Wanzer.Machine.MachineStatus.State.PilotStatus.skills[i].GetName());
                    }
                    else
                    {
                        CurrentLocalizedSkillsTextField[i].SetLabel("ui_skill_none");
                    }
                }
                for (int j = maxSkills; j < CurrentSkillsTextField.Length; j++)
                {
                    CurrentSkillsTextField[j].gameObject.SetActive(false);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(UIUpdateSkillPanel), "SelectSkill")]
        internal static class Patch_UIUpdateSkillPanel_SelectSkill
        {
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, Walker.Data.Wanzer> m_Wanzer = AccessTools.FieldRefAccess<UIUpdateSkillPanel, Walker.Data.Wanzer>("m_Wanzer");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, SkillType> m_SelectedSkill = AccessTools.FieldRefAccess<UIUpdateSkillPanel, SkillType>("m_SelectedSkill");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, List<SkillType>> m_Skills = AccessTools.FieldRefAccess<UIUpdateSkillPanel, List<SkillType>>("m_Skills");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, LocalizedUIText[]> currentLocalizedSkillsText = AccessTools.FieldRefAccess<UIUpdateSkillPanel, LocalizedUIText[]>("currentLocalizedSkillsText");
            private static readonly AccessTools.FieldRef<UIUpdateSkillPanel, TMP_Text[]> currentSkillsText = AccessTools.FieldRefAccess<UIUpdateSkillPanel, TMP_Text[]>("currentSkillsText");
            [HarmonyPrefix]
            static bool SelectSkill_Prefix(UIUpdateSkillPanel __instance, int id)
            {
                if (!Configs.PilotMaxSkillSlots.Value) return true;
                var Wanzer = m_Wanzer(__instance);
                var SkillsField = m_Skills(__instance);
                uint maxSkills = PilotData.PILOT_SKILL_MAX;
                m_SelectedSkill(__instance) = SkillsField[id];
                int count = Wanzer.Machine.MachineStatus.State.PilotStatus.skills.Count;
                if ((long)count < (long)((ulong)maxSkills))
                {
                    var CurrentLocalizedSkillsTextField = currentLocalizedSkillsText(__instance);
                    var CurrentSkillsTextField = currentSkillsText(__instance);
                    CurrentLocalizedSkillsTextField[count].SetLabel(Skill_Def.GetSkill(SkillsField[id]).Name);
                    CurrentSkillsTextField[count].rectTransform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.15f, 4, 1f);
                }
                return false;
            }
        }
    }
}