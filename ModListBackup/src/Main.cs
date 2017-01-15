using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using ModListBackup.Handlers;
using ModListBackup.Settings;
using System.IO;

namespace ModListBackup
{
    /// <summary>
    /// Holds HugsLib ModBase information and mod initialization
    /// </summary>
    public class Main : ModBase
    {
        /// <summary>
        /// An Instance of this class
        /// </summary>
        internal static Main Instance { get; private set; }

        /// <summary>
        /// Identifies the mod to HugsLib
        /// </summary>
        public override string ModIdentifier
        {
            get { return Globals.MOD_IDENTIFIER; }
        }

        /// <summary>
        /// An easy way to access the logger
        /// </summary>
        internal static ModLogger Log
        {
            get { return Instance.Logger; }
        }

        /// <summary>
        /// Returns the ModBase settings
        /// </summary>
        internal static ModSettingsPack GetSettings
        {
            get
            {
                return Instance.Settings;
            }
        }
        /// <summary>
        /// Sets the Instance
        /// </summary>
        public Main()
        {
            Instance = this;
        }

        /// <summary>
        /// Add our own Initialization for HugsLib
        /// </summary>
        public override void Initialize()
        {
            InitDirectories();
            ModsConfigHandler.BackupCurrent();
        }

        /// <summary>
        /// Log a message only when DEBUG_MODE is true
        /// </summary>
        /// <param name="message">The message to log (will be prefixed with "Debug:")</param>
        /// <param name="substitutions">The string substitions [Optional]</param>
        internal static void LogDebug(string message, params object[] substitutions)
        {
            if (Globals.DEBUG_MODE)
                Log.Message(string.Format("Debug:{0}", message), substitutions);
        }

        /// <summary>
        /// Creates the Directories our mod list backup files will go into
        /// </summary>
        private static void InitDirectories()
        {
            Directory.CreateDirectory(Globals.DIR_BACKUPs);
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
