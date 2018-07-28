using RimWorld;
using System;
using Verse;

namespace ModListBackup.UI {

    internal enum ToolCategory { Debug, ModList, Backups, Other }

    internal abstract class ToolUIContentBase {
        internal abstract string ButtonText { get; }
        internal virtual string Description { get; }
        internal virtual bool InstantStart { get; }
        internal string Name { get { return ButtonText; } }
        internal abstract Action OnStartAction { get; }
        internal virtual string OnFinishMessage { get; }
        internal bool IsVisible { get; set; }
        internal abstract ToolCategory Category { get; }

        internal ToolUIContentBase() {
            this.IsVisible = true;
        }

        internal void Start() {
            OnStartAction.Invoke();
            Verse.Messages.Message(OnFinishMessage, MessageTypeDefOf.NeutralEvent);
        }
    }
}