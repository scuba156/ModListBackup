using Harmony;
using RimWorld;
using System.IO;
using System.Reflection;
using Verse;
using Verse.Steam;

namespace ModListBackup.Mods {

    internal static class ModController {

        internal static void InstallMod(ModMetaDataEnhanced mod, string newName) {
            string dest = Path.Combine(GenFilePaths.CoreModsFolderPath, newName.Replace(" ", ""));

            CopyMod(mod, dest);

            ModMetaData newMod = new ModMetaData(dest);

            AccessTools.Method(typeof(ModLister), "RebuildModList").Invoke(null, null);
            Find.WindowStack.WindowOfType<Page_ModsConfig>().Notify_ModsListChanged();
        }

        public static void UnInstallMod(ModMetaDataEnhanced mod) {
            mod.OriginalMetaData.enabled = false;
            if (mod.RootDir.Exists) {
                mod.RootDir.Delete(true);
            }
            if (mod.OriginalMetaData.OnSteamWorkshop) {
                Unsubscribe(mod);
            }
            AccessTools.Method(typeof(ModLister), "RebuildModList").Invoke(null, null);
            Find.WindowStack.WindowOfType<Page_ModsConfig>().Notify_ModsListChanged();
        }

        private static void CopyMod(ModMetaDataEnhanced mod, string DestinationPath) {
            foreach (string dirPath in Directory.GetDirectories(mod.RootDir.FullName, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(mod.RootDir.FullName, DestinationPath));

            foreach (string newPath in Directory.GetFiles(mod.RootDir.FullName, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(mod.RootDir.FullName, DestinationPath), true);
        }

        private static void Unsubscribe(ModMetaDataEnhanced mod) {
            typeof(Workshop).GetMethod("Unsubscribe", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { mod.OriginalMetaData });
        }
    }
}