using Harmony;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using ModListBackup.Core;
using ModListBackup.Settings;
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

        public Main() {
            Instance = this;
        }

        /// <summary>
        /// Identifies the mod to HugsLib
        /// </summary>
        public override string ModIdentifier { get { return MOD_IDENTIFIER; } }

        internal static HarmonyInstance GetHarmonyInstance { get { return Instance.HarmonyInst; } }

        internal static ModSettingsPack GetSettingsPack { get { return Instance.Settings; } }

        /// <summary>
        /// An Instance of this class
        /// </summary>
        internal static Main Instance { get; private set; }

        /// <summary>
        /// An easy way to access the logger
        /// </summary>
        internal static ModLogger Log { get { return Instance.Logger; } }

        public override void Initialize() {
            Patches.Patcher.ApplyPlatformPatches();
        }
    }
}