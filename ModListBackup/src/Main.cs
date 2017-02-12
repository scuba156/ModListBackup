using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using ModListBackup.Handlers;
using ModListBackup.Handlers.Settings;
using RimWorldHandler;
using Verse;

namespace ModListBackup
{
    /// <summary>
    /// Holds HugsLib ModBase information and mod initialization
    /// </summary>
    public class Main : ModBase
    {
        /// <summary>
        /// This mods identifier
        /// </summary>
        private const string MOD_IDENTIFIER = "ModListBackup";

        /// <summary>
        /// Set true to use debug mode, mainly used for logging purposes
        /// </summary>
#if DEBUG
        private static bool DEBUG_MODE = true;
#else
        private static bool DEBUG_MODE = false;
#endif

        /// <summary>
        /// An Instance of this class
        /// </summary>
        internal static Main Instance { get; private set; }

        /// <summary>
        /// Identifies the mod to HugsLib
        /// </summary>
        public override string ModIdentifier { get { return MOD_IDENTIFIER; } }

        /// <summary>
        /// Gets mod identifier without needing the instance
        /// </summary>
        public static string GetModIdentifier { get { return MOD_IDENTIFIER; } }

        public static ModContentPack GetModContentPack { get { return ModsConfigAPI.GetModContentPack(MOD_IDENTIFIER); } }

        /// <summary>
        /// An easy way to access the logger
        /// </summary>
        internal static ModLogger Log { get { return Instance.Logger; } }

        /// <summary>
        /// Returns the ModBase settings
        /// </summary>
        internal static ModSettingsPack GetSettingsPack { get { return Instance.Settings; } }

        /// <summary>
        /// Constructor, sets the current Instance
        /// </summary>
        public Main() { Instance = this; if (DEBUG_MODE) { Log.Message("Debug mode"); } }

        /// <summary>
        /// Add our own Initialization for HugsLib
        /// </summary>
        public override void Initialize() { ModsConfigHandler.BackupCurrent(); }


        /// <summary>
        /// Log a message only when DEBUG_MODE is true
        /// </summary>
        /// <param name="message">The message to log (will be prefixed with "Debug:")</param>
        /// <param name="substitutions">The string substitions [Optional]</param>
        internal static void DebugMessage(string message, params object[] substitutions)
        {
            if (DEBUG_MODE)
                Log.Message(string.Format("Debug:{0}", message), substitutions);
        }

        /// <summary>
        /// Gets called after all defs are loaded
        /// </summary>
        public override void DefsLoaded()
        {
            SettingsHandler.Update();
            SteamSyncHandler.UpdateAllStates();
        }
    }
}
