using ModListBackup.Core;
using ModListBackup.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace ModListBackup.Utils {

    /// <summary>
    /// Class for handling ModsConfig
    /// </summary>
    internal static class ModsConfigUtils {

        /// <summary>
        /// Backup the current ModsConfig.xml for safekeeping
        /// </summary>
        internal static void BackupCurrent() {
            if (!Directory.Exists(PathUtils.DirBackupsDefault)) {
                Directory.CreateDirectory(PathUtils.DirBackupsDefault);
            }
            File.Copy(GenFilePaths.ModsConfigFilePath, Path.Combine(PathUtils.DirBackupsDefault, PathUtils.Filename_ModsConfigBackup), true);
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

        public static List<string> ConvertModListToActiveList(List<ModMetaDataEnhanced> mods) {
            List<string> result = new List<string>();
            foreach (ModMetaDataEnhanced mod in mods.Where(m=>m.Active == true))
                result.Add(mod.Identifier);
            return result;
        }

        /// <summary>
        /// Restores the ModsConfig.xml Backup
        /// </summary>
        internal static void RestoreBackup() {
            File.Copy(Path.Combine(PathUtils.DirBackupsDefault, PathUtils.Filename_ModsConfigBackup), GenFilePaths.ModsConfigFilePath, true);
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