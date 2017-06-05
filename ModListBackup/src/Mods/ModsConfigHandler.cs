using System.Collections.Generic;
using System.IO;
using Verse;

namespace ModListBackup.Controllers {

    /// <summary>
    /// Class for handling ModsConfig
    /// </summary>
    internal static class ModsConfigHandler {

        /// <summary>
        /// Backup the current ModsConfig.xml for safekeeping
        /// </summary>
        internal static void BackupCurrent() {
            File.Copy(GenFilePaths.ModsConfigFilePath, Path.Combine(PathHandler.DirHome, PathHandler.FILE_MODSCONFIG_NAME), true);
        }

        /// <summary>
        /// Gets a list of active mods in load order
        /// </summary>
        /// <returns>A list of the active mods identifier</returns>
        internal static List<string> GetActiveMods() {
            List<string> result = new List<string>();
            foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
                result.Add(mod.Identifier);
            return result;
        }

        /// <summary>
        /// Restores the ModsConfig.xml Backup
        /// </summary>
        internal static void RestoreBackup() {
            File.Copy(Path.Combine(PathHandler.DirHome, PathHandler.FILE_MODSCONFIG_NAME), GenFilePaths.ModsConfigFilePath, true);
        }

        /// <summary>
        /// Set the current active mods
        /// </summary>
        /// <param name="modsToActivate">The mods to set as active</param>
        internal static void SetActiveMods(List<string> modsToActivate) {
            ClearLoadedMods(true);
            foreach (string modID in modsToActivate)
                ModsConfig.SetActive(modID, true);
            ModsConfig.Save();
        }

        /// <summary>
        /// Clears the current active mods
        /// </summary>
        /// <param name="removeCore">Set to true to remove core [Optional](Default:false)</param>
        internal static void ClearLoadedMods(bool removeCore = false) {
            ModsConfig.Reset();
            if (removeCore)
                ModsConfig.SetActive(ModContentPack.CoreModIdentifier, false);
        }
    }

    /// <summary>
    /// A Class to define the ModsConfig data to hold
    /// </summary>
    internal class ModsConfigData {
        public List<string> activeMods = new List<string>();
        public int buildNumber = -1;
    }
}