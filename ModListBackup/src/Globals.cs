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
        /// Holds a value to use debug mode, mainly used for logging purposes
        /// </summary>
        internal static bool DEBUG_MODE = true;

        /// <summary>
        /// The limit for how may states are available
        /// </summary>
        internal const int STATE_LIMIT = 5;

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
        /// Holds the path to RimWorlds user directories
        /// </summary>
        private static string DIR_RIMWORLD_USER = GenFilePaths.SaveDataFolderPath + @"\";

        /// <summary>
        /// Holds the path to store mod list backup files
        /// </summary>
        internal static string DIR_MODLIST_BACKUP = DIR_RIMWORLD_USER + @"ModListBackup\";

    }
}
