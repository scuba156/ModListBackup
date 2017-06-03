using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace ModListBackup.Patches {
    [HarmonyPatch]
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("DoWindowContents")]
    public static class DoWindowContents_Patch {
        public static void Postfix(Rect rect) {
            Page_ModsConfig_Controller.DoWindowContents(rect);
        }
    }
}
