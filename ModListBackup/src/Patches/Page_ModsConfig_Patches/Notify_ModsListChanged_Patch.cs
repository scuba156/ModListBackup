using Harmony;
using ModListBackup.UI;
using RimWorld;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("Notify_ModsListChanged")]
    public static class Notify_ModsListChanged_Patch {

        public static bool Prefix() {
            Page_ModsConfig_Controller.Notify_ModsListChanged();
            return false;
        }
    }
}