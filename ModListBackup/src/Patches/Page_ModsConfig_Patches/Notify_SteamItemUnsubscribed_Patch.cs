using Harmony;
using ModListBackup.UI;
using RimWorld;
using Steamworks;

namespace ModListBackup.Patches.Page_ModsConfig_Patches {

    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("Notify_SteamItemUnsubscribed")]
    public static class Notify_SteamItemUnsubscribed_Patch {

        public static bool Prefix(ref PublishedFileId_t pfid) {
            Page_ModsConfig_Controller.Notify_SteamItemUnsubscribed(pfid);
            return false;
        }
    }
}