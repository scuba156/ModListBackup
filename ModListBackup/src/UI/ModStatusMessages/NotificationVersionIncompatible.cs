using System;
using Verse;

namespace ModListBackup.Mods.Notifications {

    internal class NotificationVersionIncompatible : NotificationBase {
        public override Action Action => null;

        public override string MessageText => "IncompatibleVersion".Translate();

        public override NotificationType MessageType => NotificationType.Error;
    }
}