using Harmony;
using ModListBackup.UI;
using RimWorld;
using UnityEngine;

namespace ModListBackup.Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("DoWindowContents")]
    public static class DoWindowContents_Patch {

        public static void Postfix(Rect rect) {
            Page_ModsConfig_Controller.DoWindowContents(rect);
        }
    }
}