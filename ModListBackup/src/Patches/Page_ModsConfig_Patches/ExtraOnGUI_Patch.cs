using Harmony;
using ModListBackup.UI;
using RimWorld;
using Verse;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("ExtraOnGUI")]
    public static class ExtraOnGUI_Patch {

        public static void Prefix(this Window __instance) {
            if (__instance.GetType() == typeof(Page_ModsConfig)) {
                Page_ModsConfig_Controller.ExtraOnGUI((Page_ModsConfig)__instance);
            }
        }
    }
}