using Harmony;
using HugsLib.Utils;
using ModListBackup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModListBackup.Patches {

    internal static class Patcher {

        /// <summary>
        /// Harmony on Unix has a Mono bug, so we have to apply some patches
        /// manually only on supported platforms as a work-around.
        ///
        /// Some features may be disabled until a better solution is found.
        /// </summary>
        internal static void ApplyPlatformPatches() {
            if (PlatformUtility.GetCurrentPlatform() == PlatformType.Windows) {
                try {
                    var original = typeof(RimWorld.Page_ModsConfig).GetMethod("ExtraOnGUI");
                    var postfix = new HarmonyMethod(typeof(Page_ModsConfig_Patches.ExtraOnGUI_Patch).GetMethod("Prefix"));

                    if (original == null) {
                        throw new System.Exception("ExtraOnGUI: Could not find original method!");
                    } else if (postfix == null) {
                        throw new System.Exception("ExtraOnGUI: Could not find patched method!");
                    }

                    Main.GetHarmonyInstance.Patch(original, null, postfix);
                    DebugHelper.DebugMessage("Applied Windows Platform Patches Successfully!");
                } catch (System.Exception ex) {
                    Main.Log.Error("Failed to apply platform patch!\n{0}", ex.Message);
                }
            } else {
                DebugHelper.DebugMessage("{0} Platform Detected, No Patches Applied", PlatformUtility.GetCurrentPlatform());
            }
        }
    }
}