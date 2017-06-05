using Harmony;
using System.IO;
using Verse;

namespace ModListBackup.Controllers {

    [StaticConstructorOnStartup]
    internal static class PathHandler {
        internal const string FileExtensionXML = ".xml";

        internal static readonly string DirHome = FolderUnderSaveData("ModListBackup");

        internal static readonly string DirModSettings = Path.Combine(DirHome, "Mod");

        internal static readonly string FILE_MODSCONFIG_NAME = "ModsConfig" + FileExtensionXML;

        internal static string FolderUnderSaveData(string folder) {
            //var meth = AccessTools.Method(typeof(GenFilePaths), "FolderUnderSaveData");
            //if (meth == null) {
            //    Main.Log.Error("could not get FolderUnderSaveData");
            //    return folder;
            //} else {
            //    return (string)meth.Invoke(null, new object[] { folder });
            //}
            return (string)AccessTools.Method(typeof(GenFilePaths), "FolderUnderSaveData").Invoke(null, new object[] { folder });
        }

        internal static string GenBackupModListFile(int state) {
            if (SteamSyncHandler.SYNC_TO_STEAM)
                return Path.Combine(DirHome, state.ToString() + FileExtensionXML + GenFilePaths.SavedGameExtension);
            else
                return Path.Combine(DirHome, state.ToString() + FileExtensionXML);
        }

        internal static string GenModSettingsFile(string modID) {
            return Path.Combine(DirModSettings, Path.Combine(modID, "Settings" + FileExtensionXML));
        }
    }
}