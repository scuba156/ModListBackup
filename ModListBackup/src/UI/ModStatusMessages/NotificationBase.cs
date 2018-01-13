using System;

namespace ModListBackup.Mods.Notifications {

    public abstract class NotificationBase {
        public abstract Action Action { get; }

        public abstract string MessageText { get; }

        public abstract NotificationType MessageType { get; }
    }

    public enum NotificationType {
        Error, Info, Warning
    }
}