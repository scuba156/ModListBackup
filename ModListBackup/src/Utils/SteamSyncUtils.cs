using ModListBackup.Settings;
using System.IO;
using Verse;

namespace ModListBackup.Utils {

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
                foreach (string stateFilepath in Directory.GetFiles(PathUtils.DirModSettings, "*" + PathUtils.FileExtensionXML, SearchOption.AllDirectories)) {
                    if (new FileInfo(stateFilepath).Name != PathUtils.Filename_ModsConfig) {
                        string newFilepath = stateFilepath + GenFilePaths.SavedGameExtension;
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
                foreach (string syncFilepath in Directory.GetFiles(PathUtils.DirModSettings, "*" + GenFilePaths.SavedGameExtension, SearchOption.AllDirectories)) {
                    string newFilepath = syncFilepath.Replace(GenFilePaths.SavedGameExtension, "");

                    File.Copy(syncFilepath, newFilepath, true);
                    if (File.Exists(syncFilepath))
                        File.Delete(syncFilepath);
                }
            }
        }
    }
}