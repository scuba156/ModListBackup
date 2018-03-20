using System;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace ModListBackup.Utils {

    internal static class DebugHelper {

        internal static bool BoxesVisible { get; set; }

        [Conditional("DEBUG")]
        internal static void DebugMessage(string message, params object[] substitutions) {
            Main.Log.Message(string.Format("Debug:{0}", message), substitutions);
        }

        [Conditional("DEBUG")]
        internal static void DrawBoxAroundRect(Rect rect) {
            if (BoxesVisible) {
                DebugMessage("Drawing boxes around rects");
                Widgets.DrawBox(rect);
            }
        }
    }
}