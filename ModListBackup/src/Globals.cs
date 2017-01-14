using ModListBackup.Settings;
using Verse;

namespace ModListBackup
{
    /// <summary>
    /// Class to store common, static and const global variables
    /// </summary>
    internal static class Globals
    {
        /// <summary>
        /// This mods identifier
        /// </summary>
        internal const string MOD_IDENTIFIER = "ModListBackup";

        /// <summary>
        /// Set true to use debug mode, mainly used for logging purposes
        /// </summary>
        internal static bool DEBUG_MODE = false;

        /// <summary>
        /// Gets the steam sync setting
        /// </summary>
        internal static bool SYNC_TO_STEAM
        {
            get
            {
                return SettingsHandler.SteamSyncSetting;
            }
        }

        /// <summary>
        /// The limit for how may states are available
        /// </summary>
        internal const int STATE_LIMIT = 20;

        /// <summary>
        /// Status message short delay time in ticks
        /// </summary>
        internal const int STATUS_DELAY_TICKS_SHORT = 220;

        /// <summary>
        /// Status message long delay time in ticks
        /// </summary>
        internal const int STATUS_DELAY_TICKS_LONG = 420;

        /// <summary>
        /// The extension prefix for an xml file
        /// </summary>
        internal const string XML_FILE_PREFIX = ".xml";

        /// <summary>
        /// The separator to split settings list with
        /// </summary>
        internal const char SETTINGS_LIST_SEPARATOR = '|';

        /// <summary>
        /// The extension prefix for a RimWorld save file (Add this on the end of a Backup file to force steam sync)
        /// </summary>
        internal const string RWS_FILE_PREFIX = GenFilePaths.SavedGameExtension;

        /// <summary>
        /// Holds the path to RimWorlds user directories
        /// </summary>
        private static string DIR_RIMWORLD_USER = GenFilePaths.SaveDataFolderPath + @"\";

        /// <summary>
        /// Holds the path to store mod list backup files
        /// </summary>
        internal static string DIR_MODLIST_BACKUP = DIR_RIMWORLD_USER + @"ModListBackup\";

        /// <summary>
        /// Holds the filename of RimWorld's ModsConfig file
        /// </summary>
        internal static string FILE_MODSCONFIG_NAME = "ModsConfig" + XML_FILE_PREFIX;
    }
}
