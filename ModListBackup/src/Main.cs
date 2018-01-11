using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using ModListBackup.Settings;
using ModListBackup.Core;
using ModListBackup.Utils;

namespace ModListBackup {

    /// <summary>
    /// Holds HugsLib ModBase information and mod initialization
    /// </summary>
    public class Main : ModBase {

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

    }
}