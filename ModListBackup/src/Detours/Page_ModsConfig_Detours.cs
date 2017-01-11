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
    class Page_ModsConfig_Detours
    {
        private static int selectedState = 1;

        private static string StatusMessage { get; set; }

        private static int StatusMessageDelay { get; set; }

        [WindowInjection(typeof(Page_ModsConfig))]
        private static void DoWindowContents(Window window, Rect rect)
        {
            Rect BackupRect = new Rect(rect.xMax - 400, rect.yMax - 38, 120f, 38f);
            if(Widgets.ButtonText(BackupRect, "Backup_Button_Text".Translate()))
            {
                BackupModList();
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect toRect = new Rect(BackupRect.xMax + 5f, BackupRect.y, 20f, BackupRect.height);
            Widgets.Label(toRect, ">>");

            Rect StateRect = new Rect(BackupRect.xMax + 32f, BackupRect.y, BackupRect.width - 60f, BackupRect.height);
            if (Widgets.ButtonText(StateRect, selectedState.ToString()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 1; i <= Globals.STATE_LIMIT; i++)
                {
                    //set a new variable here, otherwise the selected state will change when int i next iterates
                    int n = i;
                    options.Add(new FloatMenuOption(i.ToString(), (Action)(() => selectedState = n), MenuOptionPriority.Default, (Action)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                }

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            Rect fromRect = new Rect(StateRect.xMax + 5f, StateRect.y, 20f, BackupRect.height);
            Widgets.Label(fromRect, "<<");

            Rect RestoreRect = new Rect(StateRect.xMax + 32f, StateRect.y, BackupRect.width, BackupRect.height);
            if (Widgets.ButtonText(RestoreRect, "Restore_Button_Text".Translate()))
            {
                RestoreModList();
            }

            Text.Font = GameFont.Tiny;
            Rect StatusRect = new Rect(StateRect.x - 22f, StateRect.yMax - 58f, 100f, 18f);
            Text.Font = GameFont.Small;
            if (StatusMessageDelay > 0)
                StatusMessageDelay--;
            if (StatusMessageDelay == 0)
                ClearStatus();

            Widgets.Label(StatusRect, StatusMessage);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void BackupModList()
        {
            //TODO: Set this to output in debug mode only
            //Main.Log.Message("Backing up state {0}", selectedState);

            ModsConfigHandler.SaveState(selectedState);
            SetStatus("Status_Message_Restore".Translate());
        }

        private static void RestoreModList()
        {
            //TODO: Set this to output in debug mode only
            //Main.Log.Message("Restoring state {0}", selectedState);

            ModsConfigHandler.LoadState(selectedState);
            SetStatus("Status_Message_Backup".Translate());
        }

        private static void SetStatus(string message, Globals.Status_Delay delay = Globals.Status_Delay.longDelay)
        {
            StatusMessage = message;

            if (delay == Globals.Status_Delay.longDelay)
                StatusMessageDelay = Globals.STATUS_DELAY_TICKS_LONG;
            else
                StatusMessageDelay = Globals.STATUS_DELAY_TICKS_SHORT;
        }

        private static void ClearStatus()
        {
            StatusMessage = "";
            StatusMessageDelay = -1;
        }
    }
}
