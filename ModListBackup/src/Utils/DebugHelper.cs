using System;
using UnityEngine;
using Verse;

namespace ModListBackup.Utils {

    internal static class DebugHelper {

#if DEBUG
        internal static bool BoxesVisible { get; set; }

#endif

        internal static void DebugMessage(string message, params object[] substitutions) {
#if DEBUG
            Main.Log.Message(string.Format("Debug:{0}", message), substitutions);
#endif
        }

        internal static void DrawBoxAroundRect(Rect rect) {
#if DEBUG
            if (BoxesVisible) {
                Log.Message("Drawing box");
                Widgets.DrawBox(rect);
            }
#endif
        }
    }
}