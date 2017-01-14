using System.Collections.Generic;
using Verse;

namespace RimWorldHandler
{
    /// <summary>
    /// API Handler for Verse.ModsConfig
    /// </summary>
    public static class ModsConfigAPI
    {
        /// <summary>
        /// Sets a mods active state
        /// </summary>
        /// <param name="modIdentifier">The identifier for the mod</param>
        /// <param name="active">The state to set</param>
        public static void SetActive(string modIdentifier, bool active)
        {
            ModsConfig.SetActive(modIdentifier, active);
            ModsConfig.DeactivateNotInstalledMods();
        }

        /// <summary>
        /// Sets a mods active state
        /// </summary>
        /// <param name="modIdentifier">The mod meta data for the mod</param>
        /// <param name="active">The state to set</param>
        public static void SetActive(ModMetaData modIdentifier, bool active)
        {
            ModsConfig.SetActive(modIdentifier, active);
            ModsConfig.DeactivateNotInstalledMods();
        }

        /// <summary>
        /// Saves the current active mods
        /// </summary>
        public static void Save()
        {
            ModsConfig.Save();
        }

        /// <summary>
        /// Deactivate all mods except core
        /// </summary>
        public static void Reset()
        {
            ModsConfig.Reset();
        }

        /// <summary>
        /// Returns a list of active mods in their current load order
        /// </summary>
        /// <returns>The list of active mods meta data</returns>
        public static IEnumerable<ModMetaData> ActiveModsInLoadOrder()
        {
            return ModsConfig.ActiveModsInLoadOrder;
        }
    }
}
