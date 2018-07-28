using ModListBackup.Mods.Notifications;
using ModListBackup.StorageContainers;
using ModListBackup.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup.Core.Mods {

    public class ModMetaDataEnhanced {
        private ModListSettingsStorageData ModSettings { get; set; }

        private List<NotificationBase> notifications = new List<NotificationBase>();

        private readonly ModMetaData original;

        public ModMetaDataEnhanced(ModMetaData mod) {
            original = mod;
            Init();
        }

        public bool Active { get { return original.Active; } set { original.Active = value; } }
        public string CustomName { get { return ModSettings.CustomName; } set { ModSettings.CustomName = value; } }
        public bool Enabled { get { return original.enabled; } set { original.enabled = value; } }
        public bool HasNotifications { get { return notifications.Count > 0; } }
        public string Identifier { get { return original.Identifier; } }
        public string InstallName { get { return ModSettings.InstallName; } internal set { ModSettings.InstallName = value; } }
        public bool IsCoreMod { get { return original.IsCoreMod; } }
        public string ModName { get { return original.Name; } }
        public string Name { get { if (ModSettings.CustomName.NullOrEmpty() && ModSettings.InstallName.NullOrEmpty()) return original.Name; if (ModSettings.CustomName.NullOrEmpty()) return ModSettings.InstallName; return ModSettings.CustomName; } }
        public List<NotificationBase> Notifications { get { return notifications; } internal set { notifications = value; } }
        public ModMetaData OriginalMetaData { get { return original; } }
        public DirectoryInfo RootDir { get { return original.RootDir; } }
        public ContentSource Source { get { return original.Source; } }
        public Color TextColor { get { return ModSettings.TextColor; } set { ModSettings.TextColor = value; } }
        public string Url { get { return original.Url; } }
        public bool VersionCompatible { get { return original.VersionCompatible; } }


        private void Init() {
            this.ModSettings = new ModListSettingsStorageData {
                ModIdentifier = original.Identifier
            };

            //TODO: Add more notifications

            if (!original.VersionCompatible) {
                notifications.Add(new NotificationVersionIncompatible());
            }
        }
    }
}