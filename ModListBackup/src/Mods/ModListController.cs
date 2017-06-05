using ModListBackup.Controllers.Settings;
using ModListBackup.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace ModListBackup.Controllers {

    internal static class ModListController {

        static ModListController() {
            Data = new ModsConfigData {
                buildNumber = RimWorld.VersionControl.CurrentBuild,
                activeMods = ModsConfigHandler.GetActiveMods()
            };
        }

        private enum Mode { Saving, Loading, Inactive }

        internal static bool CanUndo { get; private set; }

        private static Mode CurrentMode { get; set; }
        private static ModsConfigData Data { get; set; }
        private static UndoActionType UndoAction { get; set; }

        internal static bool DoUndoAction() {
            if (UndoAction != null && CanUndo) {
                return UndoAction.UndoLastAction();
            }
            else {
                Main.Log.Error("DoUndoAction was called but no UndoAction was set");
                return false;
            }
        }

        internal static string GetFormattedModListName(int state) {
            return String.Format("{0}. {1}{2}", state, GetModListNamePretty(state), (ModListIsSet(state)) ? null : " [*]");
        }

        internal static string GetModListNamePretty(int state) {
            if (SettingsHandler.StateNamesSetting.Value == null || SettingsHandler.StateNamesSetting.Value.StateNames[state - 1].Trim() == "")
                return "Default_State_Name".Translate();
            else
                return SettingsHandler.StateNamesSetting.Value.GetStateName(state).Trim();
        }

        /// <summary>
        /// Load a state
        /// </summary>
        /// <param name="state">The state to load from</param>
        internal static void LoadModList(int state) {
            //TODO: check if no mods were loaded
            CurrentMode = Mode.Loading;
            UndoAction = new UndoActionType(state, LastActionType.restore);
            CanUndo = true;
            ExposeData(PathHandler.GenBackupModListFile(state));
        }

        /// <summary>
        /// Check if a state is empty
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static bool ModListIsSet(int state) {
            return File.Exists(PathHandler.GenBackupModListFile(state));
        }

        internal static ModsConfigData ReadModList(string filepath) {
            return DirectXmlLoader.ItemFromXmlFile<ModsConfigData>(filepath, true);
        }

        internal static void RestoreModList(int Modlist) {
            if (!ModListIsSet(Modlist)) {
                Page_ModsConfig_Controller.SetStatus("Status_Message_Null".Translate());
            }
            else {
                LoadModList(Modlist);
                Page_ModsConfig_Controller.SetStatus("Status_Message_Restore".Translate());
            }
        }

        /// <summary>
        /// Save a state
        /// </summary>
        /// <param name="state">The state to save it into</param>
        internal static void SaveModList(int state) {
            CurrentMode = Mode.Saving;
            UndoAction = new UndoActionType(state, LastActionType.backup);
            CanUndo = true;
            ExposeData(PathHandler.GenBackupModListFile(state));
        }

        private static void ExposeData(string filepath, bool useUndoAction = false) {
            try {
                if (CurrentMode == Mode.Saving) {
                    Data = new ModsConfigData {
                        buildNumber = RimWorld.VersionControl.CurrentBuild,
                        activeMods = ModsConfigHandler.GetActiveMods()
                    };
                    DirectXmlSaver.SaveDataObject((object)Data, filepath);
                }
                else if (CurrentMode == Mode.Loading) {
                    List<string> current = new List<string>();

                    Data = ReadModList(filepath);

                    ModsConfigHandler.ClearLoadedMods(true);
                    foreach (string modID in Data.activeMods)
                        ModsConfig.SetActive(modID, true);
                }
            }
            catch (Exception e) {
                Main.Log.ReportException(e);
                ModsConfigHandler.ClearLoadedMods();
            }
        }
    }
}