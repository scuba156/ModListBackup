using ModListBackup.Handlers.Settings;
using RimWorldHandler;
using System.IO;

namespace ModListBackup.Handlers {

    /// <summary>
    /// Class to handle syncing to steam cloud
    /// </summary>
    internal static class SteamSyncHandler {

        /// <summary>
        /// Gets the steam sync setting
        /// </summary>
        internal static bool SYNC_TO_STEAM { get { return SettingsHandler.SteamSyncSetting; } }

        /// <summary>
        /// Sets all state files to use steam sync or not, depending on what setting is selected
        /// </summary>
        internal static void UpdateAllStates() {
            if (SYNC_TO_STEAM) {
                foreach (string stateFilepath in Directory.GetFiles(PathHandler.DIR_BACKUPS, "*" + PathHandler.XML_FILE_PREFIX)) {
                    if (new FileInfo(stateFilepath).Name != PathHandler.FILE_MODSCONFIG_NAME) {
                        string newFilepath = stateFilepath + GenFilePathsAPI.SavedGameExtension;
                        if (File.Exists(newFilepath)) {
                            if (new FileInfo(stateFilepath).Name.Length != new FileInfo(newFilepath).Name.Length) {
                                File.Copy(stateFilepath, newFilepath, true);
                                File.Delete(stateFilepath);
                            }
                        }
                        else
                            File.Copy(stateFilepath, newFilepath, true);
                    }
                }
            }
            else {
                foreach (string syncFilepath in Directory.GetFiles(PathHandler.DIR_BACKUPS, "*" + GenFilePathsAPI.SavedGameExtension)) {
                    string newFilepath = syncFilepath.Replace(GenFilePathsAPI.SavedGameExtension, "");

                    File.Copy(syncFilepath, newFilepath, true);
                    if (File.Exists(syncFilepath))
                        File.Delete(syncFilepath);
                }
            }
        }
    }
}