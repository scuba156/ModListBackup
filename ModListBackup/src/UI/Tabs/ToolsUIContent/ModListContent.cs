using System;
using Verse;

namespace ModListBackup.UI.Tools {

    internal class CleanUninstalledFromSave : ToolUIContentBase {
        internal override string ButtonText => "CleanUninstalledFromSaveButtonText".Translate();
        internal override string Description => "CleanUninstalledFromSaveDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "CleanUninstalledFromSaveSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class DeleteAllMods : ToolUIContentBase {
        internal override string ButtonText => "DeleteAllModsButtonText".Translate();
        internal override string Description => "DeleteAllModsDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "DeleteAllModsSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class EditColors : ToolUIContentBase {
        internal override string ButtonText => "EditColorsButtonText".Translate();
        internal override string Description => "EditColorsDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "EditColorsSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class EnableOnlyCore : ToolUIContentBase {
        internal override string ButtonText => "EnableOnlyCoreButtonText".Translate();
        internal override string Description => "EnableOnlyCoreDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "EnableOnlyCoreSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class EnableOnlyCoreWithModList : ToolUIContentBase {
        internal override string ButtonText => "EnableOnlyCoreWithModListButtonText".Translate();
        internal override string Description => "EnableOnlyCoreWithModListDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "EnableOnlyCoreWithModListSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class ImportFromSave : ToolUIContentBase {
        internal override string ButtonText => "ImportFromSaveButtonText".Translate();
        internal override string Description => "ImportFromSaveDescription".Translate();

        internal override Action OnStartAction => () => {
            Dialogs.Dialog_Import importWindow = new Dialogs.Dialog_Import();
            Find.WindowStack.Add(importWindow);
        };

        internal override string SuccessMessage => "ImportFromSaveSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }

    internal class RemoveAllColors : ToolUIContentBase {
        internal override string ButtonText => "RemoveAllColorsButtonText".Translate();
        internal override string Description => "RemoveAllColorsDescription".Translate();
        internal override Action OnStartAction => () => { };
        internal override string SuccessMessage => "RemoveAllColorsSuccessMessage".Translate();
        internal override ToolCategory Category => ToolCategory.ModList;
    }
}