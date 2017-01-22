using HugsLib.Settings;
using ModListBackup.Detours;
using ModListBackup.Handlers;
using System;
using UnityEngine;
using Verse;

namespace ModListBackup.Settings
{
    /// <summary>
    /// Class to handle our settings
    /// </summary>
    internal static class SettingsHandler
    {
        private static bool ShowNamesList { get; set; }
        internal static SettingHandle<bool> LastRestartOnClose { get; set; }

        private static float showNamesListButtonHeight = 30f;
        private static int showRevertTick = 0;
        //private static Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Holds the steam sync setting for HugsLib
        /// </summary>
        internal static SettingHandle<bool> SteamSyncSetting { get; set; }

        /// <summary>
        /// Holds the State Names setting
        /// </summary>
        internal static SettingHandle<StateNamesHandleType> StateNamesSetting { get; set; }

        /// <summary>
        /// Sets up our settings
        /// </summary>
        internal static void Update()
        {
            SteamSyncSetting = Main.GetSettings.GetHandle<bool>("SteamSync", "Settings_Steam_Sync_Title".Translate(), "Settings_Steam_Sync_Desc".Translate(), true);

            SettingHandle revertButtonHandle = Main.GetSettings.GetHandle<bool>("RevertButton", "Settings_State_Revert_Title".Translate(), "Settings_State_Revert_Desc".Translate());

            StateNamesSetting = Main.GetSettings.GetHandle<StateNamesHandleType>("StateNames", "Settings_State_Names_Title".Translate(), "Settings_State_Names_Desc".Translate(), null);

            LastRestartOnClose = Main.GetSettings.GetHandle<bool>("LastRestartOnExit", "", "", false);

            LastRestartOnClose.NeverVisible = true;

            Page_ModsConfig_Detours.RestartOnClose = LastRestartOnClose;

            StateNamesSetting.CustomDrawerHeight = 30f;
            revertButtonHandle.CustomDrawerHeight = 50f;

            revertButtonHandle.CustomDrawer = rect =>
            {
                return DoRevertButtonDrawerContents(rect);
            };

            StateNamesSetting.CustomDrawer = rect =>
            {
                string buttonText;

                if (ShowNamesList)
                    buttonText = "Settings_Button_Hide_Text".Translate();
                else
                    buttonText = "Settings_Button_Show_Text".Translate();

                if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, showNamesListButtonHeight), buttonText))
                {
                    ShowNamesList = !ShowNamesList;
                    StateNamesSetting.CustomDrawerHeight = !ShowNamesList ? 30f : Globals.STATE_LIMIT * 42f;//400f;
                }

                if (ShowNamesList)
                    return DoStateNamesDrawerContents(rect);
                else
                    return false;
            };
        }

        internal static void RefreshStateNameSettings()
        {
            StateNamesSetting = Main.GetSettings.GetHandle<StateNamesHandleType>("StateNames", "Settings_State_Names_Title".Translate(), "Settings_State_Names_Desc".Translate(), null);
        }

        /// <summary>
        /// Draw The revert backup button
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        /// <returns>True if value changed(will always be false)</returns>
        private static bool DoRevertButtonDrawerContents(Rect rect)
        {
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, 30f), "Button_Revert_Text".Translate()))
            {
                Handlers.ModsConfigHandler.RestoreCurrent();
                showRevertTick = Globals.STATUS_DELAY_TICKS_SHORT;
            }

            string label = "";
            if (showRevertTick > 0)
            {
                label = "Status_Message_Restored".Translate();
                showRevertTick--;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.y + 32f, rect.width, 18f), label);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            return false;
        }

        /// <summary>
        /// Draw the state names button and list
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        /// <returns>True if values changed</returns>
        internal static bool DoStateNamesDrawerContents(Rect rect)
        {
            if (StateNamesSetting.Value == null)
                StateNamesSetting.Value = new StateNamesHandleType();

            Rect inRect = new Rect(rect.x - 18f, rect.y + 40f, rect.width, rect.height - 30);

            Listing_Standard listing_Standard = new Listing_Standard(inRect);

            for (int i = 0; i <= Globals.STATE_LIMIT - 1; i++)
            {
                string label = String.Format("{2}{0} {1}: ", "Settings_Label_State_Name".Translate(), i + 1, (ModsConfigHandler.StateIsSet(i)) ? null : "* ");
                string oldName = StateNamesSetting.Value.StateNames[i];
                string newName = listing_Standard.TextEntryLabeled(label, oldName);

                listing_Standard.Gap(16f);

                if (newName != oldName)
                {
                    StateNamesSetting.Value.StateNames[i] = newName;
                    return true;
                }
            }

            listing_Standard.End();

            return false;
        }
    }
}