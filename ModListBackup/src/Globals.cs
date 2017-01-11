using Verse;

namespace ModListBackup
{
    internal static class Globals
    {
        //Configurable settings
        internal static int STATE_LIMIT = 5;

        internal static int STATUS_DELAY_TICKS_SHORT = 220;
        internal static int STATUS_DELAY_TICKS_LONG = 420;

        //Pre-defined paths
        internal static string DIR_RIMWORLD_USER = GenFilePaths.SaveDataFolderPath + @"\";
        internal static string DIR_MODLIST_BACKUP = DIR_RIMWORLD_USER + @"ModListBackup\";

        //Global enum for easily passing delay type
        internal enum Status_Delay
        {
            longDelay,
            shortDelay
        }
    }
}
