using System;
using Verse;

namespace ModListBackup.Mods.Notifications {

    internal class NotificationActiveDuplicate : NotificationBase {
        public override Action Action => null;

        public override string MessageText => "ActiveDuplicates".Translate();

        public override NotificationType MessageType => NotificationType.Warning;
    }
}