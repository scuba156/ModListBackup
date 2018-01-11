using HugsLib.Settings;
using ModListBackup.Core;
using ModListBackup.Utils;
using System;
using UnityEngine;
using Verse;

namespace ModListBackup.Settings {

    /// <summary>
    /// Class to handle our settings
    /// </summary>
    internal static class SettingsHandler {

        /// <summary>
        /// The limit for how may states are available
        /// </summary>
        internal const int ModListStateLimit = 20;

        private static float showNamesListButtonHeight = 30f;
        private static int showRevertTick;

        internal static int ModBackupLimit = 3;

        /// <summary>
        /// Holds the State Names setting
        /// </summary>
        internal static SettingHandle<StateNamesHandleType> StateNamesSetting { get; set; }

        private static SettingHandle<string> modBackupDir;
        internal static SettingHandle<string> ModBackupDirectory {
            get {
                if (modBackupDir.Value.NullOrEmpty()) {
                    modBackupDir.Value = PathUtils.GenModBackupsFile();
                }
                return modBackupDir;
            }
            set {
                modBackupDir = value;
            }
        }

        /// <summary>
        /// Holds the steam sync setting for HugsLib
        /// </summary>
        internal static SettingHandle<bool> SteamSyncSetting { get; set; }

        private static bool ShowNamesList { get; set; }

        /// <summary>
        /// Draw the state names button and list
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        /// <returns>True if values changed</returns>
        internal static bool DoStateNamesDrawerContents(Rect rect) {
            if (StateNamesSetting.Value == null)
                StateNamesSetting.Value = new StateNamesHandleType();

            Rect inRect = new Rect(rect.x - 18f, rect.y + 40f, rect.width, rect.height - 30);

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            for (int i = 0; i <= SettingsHandler.ModListStateLimit - 1; i++) {
                string label = String.Format("{2}{0} {1}: ", "Settings_Label_State_Name".Translate(), i + 1, (ModListController.ModListIsSet(i)) ? null : "* ");
                string oldName = StateNamesSetting.Value.StateNames[i];
                string newName = listing_Standard.TextEntryLabeled(label, oldName);

                listing_Standard.Gap(16f);

                if (newName != oldName) {
                    StateNamesSetting.Value.StateNames[i] = newName;
                    return true;
                }
            }

            listing_Standard.End();

            return false;
        }

        internal static void RefreshStateNameSettings() {
            StateNamesSetting = Main.GetSettingsPack.GetHandle<StateNamesHandleType>("StateNames", "Settings_State_Names_Title".Translate(), "Settings_State_Names_Desc".Translate(), null);
        }

        //private static Vector2 scrollPosition = Vector2.zero;
        /// <summary>
        /// Sets up our settings
        /// </summary>
        internal static void Update() {
            SteamSyncSetting = Main.GetSettingsPack.GetHandle<bool>("SteamSync", "Settings_Steam_Sync_Title".Translate(), "Settings_Steam_Sync_Desc".Translate(), true);

            SettingHandle revertButtonHandle = Main.GetSettingsPack.GetHandle<bool>("RevertButton", "Settings_State_Revert_Title".Translate(), "Settings_State_Revert_Desc".Translate());

            StateNamesSetting = Main.GetSettingsPack.GetHandle<StateNamesHandleType>("StateNames", "Settings_State_Names_Title".Translate(), "Settings_State_Names_Desc".Translate(), null);

            StateNamesSetting.CustomDrawerHeight = 30f;
            revertButtonHandle.CustomDrawerHeight = 50f;

            revertButtonHandle.CustomDrawer = DoRevertButtonDrawerContents;

            StateNamesSetting.CustomDrawer = rect => {
                string buttonText;

                if (ShowNamesList)
                    buttonText = "Settings_Button_Hide_Text".Translate();
                else
                    buttonText = "Settings_Button_Show_Text".Translate();

                if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, showNamesListButtonHeight), buttonText)) {
                    ShowNamesList = !ShowNamesList;
                    StateNamesSetting.CustomDrawerHeight = !ShowNamesList ? 30f : SettingsHandler.ModListStateLimit * 42f;//400f;
                }

                if (ShowNamesList)
                    return DoStateNamesDrawerContents(rect);
                else
                    return false;
            };

            ModBackupDirectory = Main.GetSettingsPack.GetHandle<string>("BackupDir", "Backup Directory", "", PathUtils.DirBackupsDefault);
        }

        /// <summary>
        /// Draw The revert backup button
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        /// <returns>True if value changed(will always be false)</returns>
        private static bool DoRevertButtonDrawerContents(Rect rect) {
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, rect.width, 30f), "Button_Revert_Text".Translate())) {
                ModsConfigUtils.RestoreBackup();
                showRevertTick = 220;
            }

            string label = "";
            if (showRevertTick > 0) {
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
    }
}