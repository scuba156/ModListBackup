using Harmony;
using ModListBackup.Core;
using ModListBackup.Mods;
using ModListBackup.StorageContainers;
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
            InstallMod(mod.RootDir.FullName, newName);
        }

        internal static ModMetaDataEnhanced InstallMod(string sourcePath, string newName) {

            string dest = Path.Combine(GenFilePaths.CoreModsFolderPath, newName.Replace(" ", ""));

            PathUtils.CopyDirectory(sourcePath, dest);

            ModMetaData newMod = new ModMetaData(dest);
            ModMetaDataEnhanced result = new ModMetaDataEnhanced(newMod) {
                InstallName = newName
            };

            AccessTools.Method(typeof(ModLister), "RebuildModList").Invoke(null, null);
            ModListController.Refresh();
            Page_ModsConfig_Controller.Notify_ModsListChanged();
            return result;
        }

        internal static void OverwriteMod(ModMetaDataEnhanced originalMod, string newDataLocation) {
            try {
                ModsConfig.SetActive(originalMod.Identifier, false);
                Directory.Delete(originalMod.RootDir.FullName, true);
                PathUtils.CopyDirectory(newDataLocation, originalMod.RootDir.FullName);
                ModsConfig.SetActive(originalMod.Identifier, true);
                Page_ModsConfig_Controller.SetForceRestart();
                var messageBox = Dialog_MessageBox.CreateConfirmation("Would you like to restart RimWorld now?\nNOTE: Bugs may occur until a restart is performed.", () => { ModsConfig.RestartFromChangedMods(); }, true);
                messageBox.interactionDelay = 1f;
                messageBox.closeOnClickedOutside = false;
                Find.WindowStack.Add(messageBox);
            } catch (System.Exception ex) {
                Log.Error(ex.Message);
                throw ex;
            }
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

        private static void Unsubscribe(ModMetaDataEnhanced mod) {
            typeof(Workshop).GetMethod("Unsubscribe", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { mod.OriginalMetaData });

            ModListController.Refresh();
            Page_ModsConfig_Controller.Notify_SteamItemUnsubscribed(mod.OriginalMetaData.GetPublishedFileId());
        }
    }
}