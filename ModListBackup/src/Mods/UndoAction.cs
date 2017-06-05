using System.IO;
using Verse;
using static ModListBackup.Controllers.ModsConfigHandler;

namespace ModListBackup.Controllers {

    internal enum LastActionType { restore, backup, none }

    internal class UndoActionType {

        public UndoActionType(int state, LastActionType lastAction = LastActionType.none) {
            Main.DebugMessage("Creating last action type {1} for state {0}", state, lastAction.ToString());
            State = state;
            LastAction = lastAction;
            switch (LastAction) {
                case LastActionType.restore:
                    ModData = new ModsConfigData {
                        buildNumber = RimWorld.VersionControl.CurrentBuild,
                        activeMods = GetActiveMods()
                    };
                    break;

                case LastActionType.backup:
                    if (!ModListController.ModListIsSet(state))
                        ModData = null;
                    else
                        ModData = ModListController.ReadModList(PathHandler.GenBackupModListFile(state));
                    break;

                case LastActionType.none:
                default:
                    Main.Log.Warning("Last Undo Action was not set with a type");
                    break;
            }
        }

        private LastActionType LastAction { get; set; }
        private ModsConfigData ModData { get; set; }
        private int State { get; set; }

        internal bool UndoLastAction() {
            Main.DebugMessage("Undoing last action type {1} for state {0}", State, LastAction.ToString());
            switch (LastAction) {
                case LastActionType.restore:
                    Main.DebugMessage("Restoring {0} active mods", ModData.activeMods.Count);
                    SetActiveMods(ModData.activeMods);
                    return true;

                case LastActionType.backup:
                    if (ModData != null) {
                        Main.DebugMessage("Restored {0}'s last state", State);
                        DirectXmlSaver.SaveDataObject((object)ModData, PathHandler.GenBackupModListFile(State));
                    }
                    else
                        File.Delete(PathHandler.GenBackupModListFile(State));
                    return true;

                case LastActionType.none:
                default:
                    Main.Log.Warning("Last Undo Action was not set with a type, cannot undo last action");
                    return false;
            }
        }
    }
}