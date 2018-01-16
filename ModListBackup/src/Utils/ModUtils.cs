using Harmony;
using ModListBackup.Core;
using ModListBackup.Mods;
using ModListBackup.UI;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Verse;
using Verse.Steam;

namespace ModListBackup.Utils {

    internal static class ModUtils {

        internal static void InstallMod(ModMetaDataEnhanced mod, string newName) {
            string dest = Path.Combine(GenFilePaths.CoreModsFolderPath, newName.Replace(" ", ""));

            PathUtils.CopyDirectory(mod.RootDir.FullName, dest);

            ModMetaData newMod = new ModMetaData(dest);
            ModMetaDataEnhanced mmde = new ModMetaDataEnhanced(newMod) {
                InstallName = newName
            };

            AccessTools.Method(typeof(ModLister), "RebuildModList").Invoke(null, null);
            ModListController.Refresh();
            Page_ModsConfig_Controller.Notify_ModsListChanged();
        }

        internal static void OverwriteMod(ModMetaDataEnhanced originalMod, string newDataLocation) {
        }

        internal static void UnInstallMod(ModMetaDataEnhanced mod) {
            mod.OriginalMetaData.enabled = false;

            if (mod.RootDir.Exists) {
                mod.RootDir.Delete(true);
            }
            if (mod.OriginalMetaData.OnSteamWorkshop) {
                Unsubscribe(mod);
            }

            AccessTools.Method(typeof(ModLister), "RebuildModList").Invoke(null, null);
            ModListController.Refresh();
            Page_ModsConfig_Controller.Notify_ModsListChanged();
        }

        internal static bool VerifyModFiles(string hash) {
            return true;
        }

        private static void Unsubscribe(ModMetaDataEnhanced mod) {
            typeof(Workshop).GetMethod("Unsubscribe", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { mod.OriginalMetaData });

            ModListController.Refresh();
            Page_ModsConfig_Controller.Notify_SteamItemUnsubscribed(mod.OriginalMetaData.GetPublishedFileId());
        }
    }
}