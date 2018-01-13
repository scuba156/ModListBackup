using ModListBackup.Core;
using System;
using Verse;

namespace ModListBackup.UI.Tools {

    internal class BackupActiveMods : ToolUIContentBase {
        internal override string ButtonText => "BackupActiveModsButtonText".Translate();
        internal override string Description => "BackupActiveModsDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string OnFinishMessage => "BackupActiveModsSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.Backups;
    }

    internal class BackupAllMods : ToolUIContentBase {
        internal override string ButtonText => "BackupAllButtonText".Translate();
        internal override string Description => "BackupAllDescription".Translate();

        internal override Action OnStartAction => () => {
            Core.BackupController.Instance.BackupAll(false, false, false);
        };

        internal override string OnFinishMessage => "BackupAllSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.Backups;
    }

    internal class DeleteAllBackups : ToolUIContentBase {
        internal override string ButtonText => "DeleteAllBackupsButtonText".Translate();
        internal override string Description => "DeleteAllBackupsDescription".Translate();
        internal override Action OnStartAction => BackupController.Instance.ShowDeleteAllBackupsDialog;
        internal override string OnFinishMessage => "DeleteAllBackupsSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.Backups;

    }

    internal class DeleteBackupsOlderThan : ToolUIContentBase {
        private readonly int Month;

        internal DeleteBackupsOlderThan(int month) {
            Month = month;
        }

        internal override string ButtonText => "DeleteBackupsOlderThanButtonText".Translate(Month);
        internal override string Description => "DeleteBackupsOlderThanDescription".Translate(Month);
        internal override Action OnStartAction => () => { };
        internal override string OnFinishMessage => "DeleteBackupsOlderThanSuccessMessage".Translate(Month);
        internal override ToolCategory Category => ToolCategory.Backups;

    }

    internal class MoveBackupsToDefaultPath : ToolUIContentBase {
        internal override string ButtonText => "MoveBackupsToDefaultPathButtonText".Translate();
        internal override string Description => "MoveBackupsToDefaultPathDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string OnFinishMessage => "MoveBackupsToDefaultPathSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.Backups;
    }
}