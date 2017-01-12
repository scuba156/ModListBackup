using RimWorldHandler;
using System.Collections.Generic;
using Verse;

namespace ModListBackup.Handlers
{
    /// <summary>
    /// Class for handling ModsConfig
    /// </summary>
    class ModsConfigHandler
    {
        /// <summary>
        /// Holds the config handler mode
        /// </summary>
        private static Mode CurrentMode { get; set; }

        /// <summary>
        /// Holds a copy of the loaded/saved data
        /// </summary>
        private static ModsConfigData Data { get; set; }

        /// <summary>
        /// Save a state
        /// </summary>
        /// <param name="state">The state to save it into</param>
        internal static void SaveState(int state)
        {
            CurrentMode = Mode.Saving;
            if (Globals.SYNC_TO_STEAM)
                ExposeData(GenBackupStateFileSteamSync(state));
            else
                ExposeData(GenBackupStateFile(state));
        }

        /// <summary>
        /// Load a state
        /// </summary>
        /// <param name="state">The state to load from</param>
        internal static void LoadState(int state)
        {
            CurrentMode = Mode.Loading;
            if(Globals.SYNC_TO_STEAM)
                ExposeData(GenBackupStateFileSteamSync(state));
            else
                ExposeData(GenBackupStateFile(state));
        }

        /// <summary>
        /// Generates a filename for a state
        /// </summary>
        /// <param name="state">The state to generate a filename for</param>
        /// <returns>The filename for the state</returns>
        private static string GenBackupStateFile(int state)
        {
            return Globals.DIR_MODLIST_BACKUP + state.ToString() + Globals.XML_FILE_PREFIX;
        }

        /// <summary>
        /// Generates a filename for a state to sync with steam
        /// </summary>
        /// <param name="state">The state to generate a filename for</param>
        /// <returns>The filename for the state</returns>
        private static string GenBackupStateFileSteamSync(int state)
        {
            return Globals.DIR_MODLIST_BACKUP + state.ToString() + Globals.XML_FILE_PREFIX + Globals.RWS_FILE_PREFIX;
        }

        /// <summary>
        /// Gets a list of active mods in load order
        /// </summary>
        /// <returns>A list of the active mods identifier</returns>
        private static List<string> GetActiveMods()
        {
            List<string> result = new List<string>();
            foreach (ModMetaData mod in ModsConfigAPI.ActiveModsInLoadOrder())
                result.Add(mod.Identifier);
            return result;
        }

        /// <summary>
        /// Clears the current active mods
        /// </summary>
        /// <param name="removeCore">Set to true to remove core [Optional](Default:false)</param>
        private static void ClearLoadedMods(bool removeCore = false)
        {
            ModsConfigAPI.Reset();
            if(removeCore)
                ModsConfigAPI.SetActive(ModContentPack.CoreModIdentifier, false);
        }

        /// <summary>
        /// Saves or Loads the state
        /// </summary>
        /// <param name="filepath">The filepath to the state</param>
        private static void ExposeData(string filepath)
        {
            try
            {
                if (CurrentMode == Mode.Saving)
                {
                    Main.LogDebug("Saving state to {0}", filepath);
                    Data = new ModsConfigData
                    {
                        buildNumber = RimWorld.VersionControl.CurrentBuild,
                        activeMods = GetActiveMods()
                    };
                    XmlSaverAPI.SaveDataObject((object)Data, filepath);
                }
                else if (CurrentMode == Mode.Loading)
                {
                    Main.LogDebug("Loading state from {0}", filepath);
                    Data = XmlLoaderAPI.ItemFromXmlFile<ModsConfigData>(filepath, true);
                    ClearLoadedMods(true);
                    foreach (string modID in Data.activeMods)
                        ModsConfigAPI.SetActive(modID, true);
                }
            }
            catch (System.Exception e)
            {
                //An error occurred, output to log and reset loaded mods
                Main.Log.ReportException(e, Globals.MOD_IDENTIFIER, true, "ExposeData");
                ClearLoadedMods();
            }
        }

        /// <summary>
        /// A Class to define the ModsConfig data to hold
        /// </summary>
        private class ModsConfigData
        {
            public int buildNumber = -1;
            public List<string> activeMods = new List<string>();
        }

        /// <summary>
        /// Enum for easily setting current mode
        /// </summary>
        private enum Mode { Saving, Loading }
    }
}
