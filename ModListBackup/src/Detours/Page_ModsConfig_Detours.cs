using HugsLib.GuiInject;
using ModListBackup.Handlers;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ModListBackup.Detours
{
    /// <summary>
    /// Class to Handle our code injection
    /// </summary>
    class Page_ModsConfig_Detours
    {
        /// <summary>
        /// Holds the currently selected save state, default to 1
        /// </summary>
        private static int selectedState = 1;

        /// <summary>
        /// Holds the status message to display
        /// </summary>
        private static string StatusMessage { get; set; }

        /// <summary>
        /// Holds how long the status message should stay visible in ticks
        /// </summary>
        private static int StatusMessageDelay { get; set; }

        /// <summary>
        /// HugsLib window injection that loads AFTER the Page_ModsConfig DoWindowContents method is called
        /// </summary>
        /// <param name="window">Page_ModsConfig's Window</param>
        /// <param name="rect">Page_ModConfig's Rect</param>
        [WindowInjection(typeof(Page_ModsConfig))]
        private static void DoWindowContents(Window window, Rect rect)
        {
            // Backup Button
            Rect BackupRect = new Rect(rect.xMax - 400, rect.yMax - 38, 120f, 38f);
            if(Widgets.ButtonText(BackupRect, "Backup_Button_Text".Translate()))
                BackupModList();

            // '>>' Label
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect toRect = new Rect(BackupRect.xMax + 5f, BackupRect.y, 20f, BackupRect.height);
            Widgets.Label(toRect, ">>");

            // State button and Float menu
            Rect StateRect = new Rect(BackupRect.xMax + 32f, BackupRect.y, BackupRect.width - 60f, BackupRect.height);
            if (Widgets.ButtonText(StateRect, selectedState.ToString()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 1; i <= Globals.STATE_LIMIT; i++)
                {
                    //set a new variable here, otherwise the selected state and button text will change when int i next iterates
                    int n = i;
                    options.Add(new FloatMenuOption(i.ToString(), (Action)(() => selectedState = n), MenuOptionPriority.Default, (Action)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                }

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            // '<<' Label
            Rect fromRect = new Rect(StateRect.xMax + 5f, StateRect.y, 20f, BackupRect.height);
            Widgets.Label(fromRect, "<<");

            // Restore Button
            Rect RestoreRect = new Rect(StateRect.xMax + 32f, StateRect.y, BackupRect.width, BackupRect.height);
            if (Widgets.ButtonText(RestoreRect, "Restore_Button_Text".Translate()))
                RestoreModList();

            // Status Label
            UpdateStatus();
            Text.Font = GameFont.Tiny;
            Rect StatusRect = new Rect(StateRect.x - 22f, StateRect.yMax - 58f, 100f, 18f);
            Widgets.Label(StatusRect, StatusMessage);

            //Reset text anchor
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Calls the ModHandler SaveState function
        /// </summary>
        private static void BackupModList()
        {
            ModsConfigHandler.SaveState(selectedState);
            SetStatus("Status_Message_Backup".Translate());
        }

        /// <summary>
        /// Calls the ModHandler LoadState function
        /// </summary>
        private static void RestoreModList()
        {
            ModsConfigHandler.LoadState(selectedState);
            SetStatus("Status_Message_Restore".Translate());
        }

        /// <summary>
        /// Sets a status message to be displayed
        /// </summary>
        /// <param name="message">The messge to display</param>
        /// <param name="delay">How long the message should stay visable (Default:longDelay)</param>
        private static void SetStatus(string message, Status_Delay delay = Status_Delay.longDelay)
        {
            StatusMessage = message;
            if (delay == Status_Delay.longDelay)
                StatusMessageDelay = Globals.STATUS_DELAY_TICKS_LONG;
            else
                StatusMessageDelay = Globals.STATUS_DELAY_TICKS_SHORT;
        }

        /// <summary>
        /// Updates status message delay ticks
        /// </summary>
        private static void UpdateStatus()
        {
            if (StatusMessageDelay > 0)
                StatusMessageDelay--;
            if (StatusMessageDelay == 0)
                ClearStatus();
        }

        /// <summary>
        /// Enum for easily setting status message delay
        /// </summary>
        internal enum Status_Delay { longDelay, shortDelay }

        /// <summary>
        /// Clears the status message
        /// </summary>
        private static void ClearStatus()
        {
            StatusMessage = "";
            StatusMessageDelay = -1;
        }
    }
}
