using Harmony;
using ModListBackup.UI;
using RimWorld;
using UnityEngine;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("DoWindowContents")]
    public static class DoWindowContents_Patch {

        public static bool Prefix(this Page_ModsConfig __instance, ref Rect rect) {
            Page_ModsConfig_Controller.DoWindowContents(__instance, rect);
            return false;
        }
    }
}