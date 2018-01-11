using HugsLib.Core;
using ModListBackup.Mods;
using ModListBackup.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace ModListBackup.StorageContainers {

    public class BackupDataStorageContainer : PersistentDataManager {
        public List<BackupListStorageData> Data { get; set; }
        protected override string FileName => PathUtils.Filename_Backups;
        protected override string FolderName => "ModListBackup";

        public void Load() {
            this.LoadData();
        }

        public void Save() {
            this.SaveData();
        }

        public BackupDataStorageContainer() {
            Data = new List<BackupListStorageData>();
        }

        protected override void LoadFromXml(XDocument xml) {
            Data = new List<BackupListStorageData>();
            if (xml.Root == null) throw new NullReferenceException("Missing root node");
            foreach (var modElem in xml.Element("ModBackups").Elements()) {
                try {
                    var backupData = new BackupListStorageData(modElem.Element("identifier").Value) {
                        Location = modElem.Element("location").Value,
                        Name = modElem.Element("name").Value,
                        Source = (ContentSource)Enum.Parse(typeof(ContentSource), modElem.Element("source").Value)
                    };
                    foreach (var backupElem in modElem.Elements("backup")) {
                        if (backupElem.Element("id") == null) {
                            Log.Error("Not a valid backup xml element");
                            continue;
                        }
                        var backup = new BackupStorageData();
                        foreach (var backupNode in backupElem.Elements()) {
                            try {
                                if (backupNode.Name.ToString().ToLower() == "id") {
                                    backup.Id = int.Parse(backupNode.Value);
                                } else if (backupNode.Name.ToString().ToLower() == "created") {
                                    backup.CreationDate = new DateTime(long.Parse(backupNode.Value));
                                } else if (backupNode.Name.ToString().ToLower() == "size") {
                                    backup.Size = long.Parse(backupNode.Value);
                                } else if (backupNode.Name.ToString().ToLower() == "hash") {
                                    backup.ModHash = backupNode.Value;
                                }
                            } catch (Exception e) {
                                Log.Notify_Exception(e);
                                throw;
                            }
                        }
                        backupData.ModBackupsList.Add(backup);
                    }
                    Data.Add(backupData);
                } catch (Exception) {
                    throw;
                }
            }
        }

        protected override void WriteXml(XDocument xml) {
            var root = new XElement("ModBackups");
            foreach (var modBackupData in Data) {

                if (modBackupData.ModBackupsList.Count == 0) return;
                var modElem = new XElement("mod");
                modElem.Add(new XElement("identifier", new XText(modBackupData.ModIdentifier)));
                modElem.Add(new XElement("name", new XText(modBackupData.Name)));
                modElem.Add(new XElement("location", new XText(modBackupData.Location)));
                modElem.Add(new XElement("source", new XText(((int)modBackupData.Source).ToString())));

                foreach (var backup in modBackupData.ModBackupsList) {
                    var backupElem = new XElement("backup");
                    backupElem.Add(new XElement("id", new XText(backup.Id.ToString())));
                    backupElem.Add(new XElement("created", new XText(backup.CreationDate.Ticks.ToString())));
                    backupElem.Add(new XElement("size", new XText(backup.Size.ToString())));
                    if (!backup.ModHash.NullOrEmpty()) {
                        backupElem.Add(new XElement("hash", new XText(backup.ModHash)));
                    }
                    modElem.Add(backupElem);
                }
                root.Add(modElem);
            }
            xml.Add(root);
        }
    }

    public class BackupStorageData {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string ModHash { get; set; }
        public long Size { get; set; }

        internal BackupStorageData(int id) {
            if (id < 0) {
                Log.Message("Failed to create Backup MetaData with id " + id);
                return;
            }
            this.Id = id;
            CreationDate = DateTime.Now;
        }

        internal BackupStorageData() { }
    }

    public class BackupListStorageData {
        public List<BackupStorageData> ModBackupsList { get; set; }

        public string Location { get; set; }
        public string ModIdentifier { get; private set; }
        public string Name { get; set; }
        public ContentSource Source { get; set; }
        public long TotalSize { get { long result = 0;  foreach (var backup in ModBackupsList) { result += backup.Size; } return result; } }
        public string TotalSizeReadable { get { return PathUtils.GetBytesReadable(TotalSize); } }


        public override string ToString() {
            string s = "Identifier: " + this.ModIdentifier;
            s += "\nName: " + this.Name;
            s += "\nPath: " + this.Location;
            s += "\nSource: " + this.Source;
            return s;
        }

        public BackupListStorageData(string Identifier) {
            this.ModIdentifier = Identifier;
            ModBackupsList = new List<BackupStorageData>();
        }

        public BackupStorageData GetLatestBackup() {
            return ModBackupsList.MaxBy(b=>b.CreationDate);
        }

        public BackupStorageData BackupNow(ModMetaDataEnhanced mod, bool dohash = false) {
            BackupStorageData backup = new BackupStorageData(ModBackupsList.Count);
            backup.CreationDate = DateTime.Now;
            backup.Size = PathUtils.GetDirectorySize(mod.RootDir);
            if (dohash) {
                backup.ModHash = PathUtils.CreateDirectoryMd5(mod.RootDir);
            }
            ModUtils.CopyMod(mod, Path.Combine(Location, backup.Id.ToString()));
            ModBackupsList.Add(backup);
            return backup;
        }
    }

    public enum SourceType { Unknown, Local, Steam }

}