using ModListBackup.Mods;
using ModListBackup.Settings;
using ModListBackup.StorageContainers;
using ModListBackup.UI;
using ModListBackup.UI.SearchBars;
using ModListBackup.Utils;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ModListBackup.Core {

    public class ModListController {

        public ModListController() {
            ActiveModList = new List<ModMetaDataEnhanced>();
            UpdateLoadedMods();
        }

        public static ModListController Instance {
            get {
                if (_instance == null) {
                    _instance = new ModListController();
                }
                return _instance;
            }
        }

        public List<ModMetaDataEnhanced> ActiveModList { get; internal set; }

        internal bool CanRestorePreviousState { get { return PreviousStateMetaData != null; } }
        private static ModListController _instance { get; set; }

        private ModListStateStorageData CurrentStateMetaData { get; set; }
        private ModListStateStorageData PreviousStateMetaData { get; set; }

        public static void Refresh() {
            Instance.UpdateLoadedMods();
        }

        public ModMetaDataEnhanced GetModEnhanced(ModMetaData mod) {
            return ActiveModList.First(m => m.OriginalMetaData == mod);
        }

        public ModMetaDataEnhanced GetModEnhancedByIdentifier(string identifier) {
            return ActiveModList.First(m => m.Identifier == identifier);
        }

        public IEnumerable<ModMetaDataEnhanced> LoadState(int stateId) {
            ModListStateStorageContainer file = new ModListStateStorageContainer(stateId);
            file.Load();
            SetActiveMods(file.Data);
            Page_ModsConfig_Controller.SetMessage("Loaded Modlist from state " + stateId, MessageTypeDefOf.NeutralEvent);
            return ActiveModList;
        }

        public void RestorePreviousState() {
            DebugHelper.DebugMessage("Trying to restore previous state");
            DebugHelper.DebugMessage("Undo:There are currently {0} active mods, previously there was {1} active.", CurrentStateMetaData.ActiveMods.Count, PreviousStateMetaData.ActiveMods.Count);
            SetActiveMods(PreviousStateMetaData);
            Page_ModsConfig_Controller.SetMessage("Status_Message_Undone".Translate(), MessageTypeDefOf.NeutralEvent);
        }

        public void SaveState(int stateId, List<string> mods = null) {
            if (mods == null) {
                DebugHelper.DebugMessage("mods to save was null, setting default");
                mods = CurrentStateMetaData.ActiveMods;
            }
            ModListStateStorageContainer file = new ModListStateStorageContainer(stateId) {
                Data = new ModListStateStorageData {
                    ActiveMods = mods,
                    BuildNumber = VersionControl.CurrentBuild
                }
            };
            file.Save();
            Page_ModsConfig_Controller.SetMessage("Saved Modlist to state " + stateId, MessageTypeDefOf.PositiveEvent);
        }

        internal static string GetFormattedModListName(int state) {
            return string.Format("{0}. {1}{2}", state, GetModListNamePretty(state), (ModListIsSet(state)) ? null : " [*]");
        }

        internal static string GetModListNamePretty(int state) {
            if (SettingsHandler.StateNamesSetting.Value == null || SettingsHandler.StateNamesSetting.Value.StateNames[state - 1].Trim() == "")
                return "Default_State_Name".Translate();
            else
                return SettingsHandler.StateNamesSetting.Value.GetStateName(state).Trim();
        }

        internal static bool ModListIsSet(int state) {
            return false;//File.Exists(PathUtils.GenModListFile(state));
        }

        internal List<ModMetaDataEnhanced> ModsInSortedOrder(ModListSearchBarOptions options) {
            List<ModMetaDataEnhanced> result = new List<ModMetaDataEnhanced>();
            ModListFilterOptions filter = (ModListFilterOptions)options.Filter;
            ModListSortOptions sortBy = (ModListSortOptions)options.SortOptions;

            if (filter == null) {
                DebugHelper.DebugMessage("filter null");
            }
            if (sortBy == null) {
                DebugHelper.DebugMessage("sort by null");
            }

            if (sortBy.SortByAlphabetical) {
                foreach (var item in from m in ActiveModList orderby m.Name select m) {
                    result.Add(item);
                }
            } else if (sortBy.SortByLoadOrder || sortBy.SortBy.Count == 0) {
                foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder) {
                    result.Add(GetModEnhancedByIdentifier(mod.Identifier));
                }
                foreach (ModMetaDataEnhanced mod2 in from m in ActiveModList
                                                     orderby m.Name ascending
                                                     orderby m.VersionCompatible descending
                                                     //orderby m.OriginalMetaData.OnSteamWorkshop ascending
                                                     where !m.Active
                                                     select m) {
                    result.Add(mod2);
                }
            } else if (sortBy.SortByColor) {
                foreach (var item in from m in ActiveModList orderby m.TextColor.ToString() select m) {
                    result.Add(item);
                }
            } else if (sortBy.SortBySource) {
                foreach (var item in from m in ActiveModList orderby m.Source select m) {
                    result.Add(item);
                }
            }

            if (!options.SearchValue.NullOrEmpty()) {
                result = result.FindAll(m => m.Name.ToLower().Contains(options.SearchValue.ToLower()) || m.Identifier.ToLower().Contains(options.SearchValue.ToLower()));
            }

            if (!filter.ShowDisabled) {
                result.RemoveAll(m => m.Active == false);
            }

            if (!filter.ShowEnabled) {
                result.RemoveAll(m => m.Active == true);
            }

            if (!filter.ShowIncompatible) {
                result.RemoveAll(m => m.OriginalMetaData.VersionCompatible == false);
            }

            if (!filter.ShowLocal) {
                result.RemoveAll(m => m.Source == ContentSource.LocalFolder);
            }

            if (!filter.ShowSteam) {
                result.RemoveAll(m => m.Source == ContentSource.SteamWorkshop);
            }

            //if (SortOptions.SortDescending) {
            //    result.Reverse();
            //}

            return result;
        }

        private void SetActiveMods(ModListStateStorageData data) {
            foreach (var item in ActiveModList) {
                item.Active = data.ActiveMods.Contains(item.Identifier);
            }
            PreviousStateMetaData = CurrentStateMetaData;
            CurrentStateMetaData = data;
            DebugHelper.DebugMessage("Undo:There are currently {0} active mods, previously there was {1} active.", CurrentStateMetaData.ActiveMods.Count, PreviousStateMetaData.ActiveMods.Count);

            Page_ModsConfig_Controller.Notify_ModsListChanged();
        }

        private void UpdateLoadedMods() {
            ActiveModList.Clear();
            foreach (var mod in ModLister.AllInstalledMods.Where(m => m.enabled)) {
                ActiveModList.Add(new ModMetaDataEnhanced(mod));
            }

            DebugHelper.DebugMessage("Updated with {0} loaded mods from {1} mods.", ActiveModList.Count, ModLister.AllInstalledMods.Count());
        }
    }
}