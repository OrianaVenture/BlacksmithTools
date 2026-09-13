using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlacksmithTools
{
    [HarmonyPatch]
    public static class BoneReorder
    {
        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachItem))]
        [HarmonyPostfix]
        static void AttachItemPatch(VisEquipment __instance, GameObject __result, int itemHash)
        {
            if (!Main.reorderEnabled.Value) return;

            if (__result == null) return;
            if (__result.name.StartsWith("attach_skin") && ObjectDB.instance.GetItemPrefab(itemHash) != null)
            {
                SetSMRBones(__instance, __result, itemHash);
            }
        }

        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachArmor))]
        [HarmonyPostfix]
        static void AttachArmorPatch(VisEquipment __instance, List<GameObject> __result, int itemHash)
        {
            if (!Main.reorderEnabled.Value || __result == null) return;

            foreach (GameObject result in __result)
            {
                if (!result.name.StartsWith("attach_skin")) continue;
                SetSMRBones(__instance, result, itemHash);
            }
        }

        public static void SetSMRBones(VisEquipment ve, GameObject instance, int hash)
        {
            Util.LogMessage("Trying to reorder bones...");

            if (ve == null || ve.m_bodyModel == null || ve.m_bodyModel.rootBone == null) return;

            try
            {
                SkinnedMeshRenderer origsmr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                SkinnedMeshRenderer[] smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                if (origsmr == null) return;

                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    if (smr == null) continue;
                    SetBones(smr, GetBoneNames(origsmr), ve.m_bodyModel.rootBone);
                }

                Util.LogMessage("Success!");
            }
            catch(Exception e)
            {
                Util.LogMessage(e.Message, BepInEx.Logging.LogLevel.Error);
            }
        }

        public static string[] GetBoneNames(SkinnedMeshRenderer smr)
        {
            List<string> boneNames = new List<string>();

            foreach (Transform bone in smr.bones)
            {
                boneNames.Add(bone.name);
            }

            return boneNames.ToArray();
        }

        public static void SetBones(SkinnedMeshRenderer smr, string[] boneNames, Transform skeletonRoot)
        {
            if (smr.bones.Length != boneNames.Length) return;

            Transform[] bones = new Transform[smr.bones.Length];
            for (int j = 0; j < bones.Length; j++)
            {
                bones[j] = Util.FindInChildren(skeletonRoot, boneNames[j]);
                if (bones[j] == null) return;
            }

            smr.bones = bones;
            smr.rootBone = skeletonRoot;
        }
    }
}
