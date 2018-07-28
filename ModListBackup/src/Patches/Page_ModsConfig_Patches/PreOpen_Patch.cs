using Harmony;
using ModListBackup.UI;
using RimWorld;
using Verse;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("PreOpen")]
    public static class PreOpen_Patch {

        public static void Postfix(this Page_ModsConfig __instance) {
            Page_ModsConfig_Controller.PreOpen(__instance);
        }
    }
}