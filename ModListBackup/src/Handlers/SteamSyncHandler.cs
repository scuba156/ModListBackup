using ModListBackup.Handlers.Settings;
using RimWorldHandler;

namespace ModListBackup.Handlers
{
    /// <summary>
    /// Class to handle syncing to steam cloud
    /// </summary>
    internal static class SteamSyncHandler
    {
        /// <summary>
        /// Gets the steam sync setting
        /// </summary>
        internal static bool SYNC_TO_STEAM { get { return SettingsHandler.SteamSyncSetting; } }

        /// <summary>
        /// Sets all state files to use steam sync or not, depending on what setting is selected
        /// </summary>
        internal static void UpdateAllStates()
        {
            if (SYNC_TO_STEAM)
            {
                foreach (string stateFilepath in PathHandler.GetFiles(PathHandler.DIR_BACKUPS, "*" + PathHandler.XML_FILE_PREFIX))
                {
                    if (PathHandler.GetFileName(stateFilepath) != PathHandler.FILE_MODSCONFIG_NAME)
                    {
                        string newFilepath = stateFilepath + GenFilePathsAPI.SavedGameExtension;
                        if (PathHandler.FileExists(newFilepath))
                        {
                            if (PathHandler.GetFileName(stateFilepath).Length != PathHandler.GetFileName(newFilepath).Length)
                            {
                                PathHandler.FileCopy(stateFilepath, newFilepath, true);
                                PathHandler.FileDelete(stateFilepath);
                            }
                        }
                        else
                            PathHandler.FileCopy(stateFilepath, newFilepath, true);
                    }
                }
            }
            else
            {
                foreach (string syncFilepath in PathHandler.GetFiles(PathHandler.DIR_BACKUPS, "*" + GenFilePathsAPI.SavedGameExtension))
                {
                    string newFilepath = syncFilepath.Replace(GenFilePathsAPI.SavedGameExtension, "");

                    PathHandler.FileCopy(syncFilepath, newFilepath, true);
                    if (PathHandler.FileExists(syncFilepath))
                        PathHandler.FileDelete(syncFilepath);
                }
            }
        }
    }
}
