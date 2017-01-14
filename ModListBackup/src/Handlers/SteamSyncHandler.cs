using System.IO;

namespace ModListBackup.Handlers
{
    /// <summary>
    /// Class to handle syncing to steam cloud
    /// </summary>
    internal static class SteamSyncHandler
    {
        /// <summary>
        /// Sets all state files to use steam sync or not, depending on what setting is selected
        /// </summary>
        internal static void UpdateAllStates()
        {
            if (Globals.SYNC_TO_STEAM)
            {
                foreach (string stateFilepath in Directory.GetFiles(Globals.DIR_MODLIST_BACKUP, "*" + Globals.XML_FILE_PREFIX))
                {
                    if (new FileInfo(stateFilepath).Name != Globals.FILE_MODSCONFIG_NAME)
                    {
                        string newFilepath = stateFilepath + Globals.RWS_FILE_PREFIX;
                        if (File.Exists(newFilepath))
                        {
                            if (new FileInfo(stateFilepath).Length != new FileInfo(newFilepath).Length)
                            {
                                File.Copy(stateFilepath, newFilepath, true);
                                File.Delete(stateFilepath);
                            }
                        }
                        else
                            File.Move(stateFilepath, newFilepath);
                    }
                }
            }
            else
            {
                foreach (string syncFilepath in Directory.GetFiles(Globals.DIR_MODLIST_BACKUP, "*" + Globals.RWS_FILE_PREFIX))
                {
                    string newFilepath = syncFilepath.Replace(Globals.RWS_FILE_PREFIX, "");

                    File.Copy(syncFilepath, newFilepath, true);
                    if (File.Exists(syncFilepath))
                        File.Delete(syncFilepath);
                }
            }
        }
    }
}
