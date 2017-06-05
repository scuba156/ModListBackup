using ModListBackup.Mods.Notifications;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup.Mods {

    internal class ModMetaDataEnhanced {
        private ModSettingsController modSettings;

        private List<NotificationBase> notifications = new List<NotificationBase>();

        private ModMetaData original;

        public ModMetaDataEnhanced(ModMetaData mod) {
            original = mod;
            Init();
        }

        public bool Active { get { return original.Active; } set { original.Active = value; } }
        public string AltName { get { return modSettings.AltName; } set { modSettings.AltName = value; } }
        public bool Enabled { get { return original.enabled; } set { original.enabled = value; } }
        public bool HasNotifications { get { return notifications.Count > 0; } }
        public string Identifier { get { return original.Identifier; } }
        public string InstallName { get { return modSettings.InstallName; } internal set { modSettings.InstallName = value; } }
        public bool IsCoreMod { get { return original.IsCoreMod; } }
        public string ModName { get { return original.Name; } }
        public string Name { get { if (modSettings.AltName.NullOrEmpty() && modSettings.InstallName.NullOrEmpty()) return original.Name; if (modSettings.AltName.NullOrEmpty()) return modSettings.InstallName; return modSettings.AltName; } }
        public List<NotificationBase> Notifications { get { return notifications; } internal set { notifications = value; } }
        public ModMetaData OriginalMetaData { get { return original; } }
        public DirectoryInfo RootDir { get { return original.RootDir; } }
        public ContentSource Source { get { return original.Source; } }
        public Color TextColor { get { return modSettings.TextColor; } set { modSettings.TextColor = value; } }
        public string Url { get { return original.Url; } }
        public bool VersionCompatible { get { return original.VersionCompatible; } }

        public void Delete() {
            original.RootDir.Delete(true);
            modSettings.Delete();
        }

        private void Init() {
            this.modSettings = new ModSettingsController(original.Identifier);

            if (!original.VersionCompatible) {
                notifications.Add(new NotificationVersionIncompatible());
            }
        }
    }
}