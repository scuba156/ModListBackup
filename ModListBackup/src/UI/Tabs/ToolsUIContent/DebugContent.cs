#if DEBUG
using ModListBackup.Utils;
using System;
using Verse;

namespace ModListBackup.UI.Tools {

    internal class DebugRestartRimWorld : ToolUIContentBase {
        internal override string ButtonText => "RestartButton".Translate();
        internal override bool InstantStart => true;
        internal override Action OnStartAction => ModsConfig.RestartFromChangedMods;
        internal override ToolCategory Category => ToolCategory.Debug;
    }

    internal class DebugToggleRectLines : ToolUIContentBase {
        internal override string ButtonText => "Toggle Lines Around Rects";
        internal override bool InstantStart => true;
        internal override Action OnStartAction => () => { DebugHelper.BoxesVisible = !DebugHelper.BoxesVisible; };
        internal override ToolCategory Category => ToolCategory.Debug;
    }

    internal class DebugRefreshModList : ToolUIContentBase {
        internal override string ButtonText => "Refresh Modlist";
        internal override bool InstantStart => true;
        internal override Action OnStartAction => Core.Mods.ModListController.Refresh;
        internal override string OnFinishMessage => "Modlist Updated";
        internal override ToolCategory Category => ToolCategory.Debug;
    }
}
#endif