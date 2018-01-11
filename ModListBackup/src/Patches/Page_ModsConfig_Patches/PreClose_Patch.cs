using Harmony;
using ModListBackup.UI;
using RimWorld;
using Verse;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("PreClose")]
    public static class PreClose_Patch {

        public static void Postfix() {
            Page_ModsConfig_Controller.PreClose();
        }
    }
}