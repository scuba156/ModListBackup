using RimWorldHandler;
using System.IO;
using Verse;

namespace ModListBackup.Handlers {

    [StaticConstructorOnStartup]
    internal static class PathHandler {

        /// <summary>
        /// The extension prefix for an xml file
        /// </summary>
        internal const string XML_FILE_PREFIX = ".xml";

        /// <summary>
        /// Holds the path to store mod list backup files
        /// </summary>
        internal static string DIR_BACKUPS = GenFilePathsAPI.FolderUnderSaveData("ModListBackup");

        /// <summary>
        /// Holds the path to where our images are stored
        /// </summary>
        internal static string DIR_IMAGES = Path.Combine(Main.GetModContentPack.RootDir, "img");

        /// <summary>
        /// Holds the filename of RimWorld's ModsConfig file
        /// </summary>
        internal static string FILE_MODSCONFIG_NAME = "ModsConfig" + XML_FILE_PREFIX;

        /// <summary>
        /// Generates a filename for a state
        /// </summary>
        /// <param name="state">The state to generate a filename for</param>
        /// <returns>The filename for the state</returns>
        internal static string GenBackupStateFile(int state) {
            if (SteamSyncHandler.SYNC_TO_STEAM)
                return Path.Combine(DIR_BACKUPS, state.ToString() + XML_FILE_PREFIX + GenFilePathsAPI.SavedGameExtension);
            else
                return Path.Combine(DIR_BACKUPS, state.ToString() + XML_FILE_PREFIX);
        }
    }
}