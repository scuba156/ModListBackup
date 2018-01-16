using ModListBackup.Mods;
using ModListBackup.Settings;
using ModListBackup.StorageContainers;
using ModListBackup.UI;
using ModListBackup.UI.SearchBars;
using ModListBackup.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Verse;

namespace ModListBackup.Core {

    public class BackupController {
        private BackupDataStorageContainer modsContainer;

        public BackupController() {
            modsContainer = new BackupDataStorageContainer();
            Load();
        }

        public static BackupController Instance {
            get {
                if (_Instance == null) {
                    _Instance = new BackupController();
                }
                return _Instance;
            }
        }

        public List<BackupListStorageData> Mods { get { if (modsContainer == null || modsContainer.Data == null) Load(); return modsContainer.Data; } internal set { modsContainer.Data = value; Save(); } }
        private static BackupController _Instance { get; set; }

        public void DeleteAllModsBackups(BackupListStorageData backup) {

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DeleteAllBackupsDialog".Translate(backup.Name), () => {
                LongEventHandler.QueueLongEvent(delegate {
                    LongEventHandler.SetCurrentEventText(string.Format("Please Wait\nDeleting all backups for\n{0}", backup.Name));

                    Directory.Delete(backup.Location, true);
                    Mods.Remove(backup);

                    LongEventHandler.ExecuteWhenFinished(delegate {
                        Save();
                        Page_ModsConfig_Controller.Notify_BackupListChanged();
                    });
                }, "Deleting all mods", true, null);
            }, true));
        }

        public BackupResult Backup(ModMetaDataEnhanced mod, bool verifyBackup = false) {
            BackupResult result = new BackupResult(mod.Identifier);
            BackupListStorageData existing = GetBackupData(mod.Identifier);
            if (existing == null) {
                existing = new BackupListStorageData(mod.Identifier) {
                    Source = mod.Source,
                    Name = mod.Name,
                    Location = Path.Combine(SettingsHandler.ModBackupDirectory, mod.Identifier),
                };
            } else {
                Mods.Remove(existing);
            }

            try {
                BackupStorageData backup = existing.BackupNow(mod, true);
                result.Success = true;

                if (verifyBackup) {
                    if (VerifyBackup(backup, mod)) {
                        result.Verified = true;
                        DebugHelper.DebugMessage("Verified " + mod.Identifier);
                    } else {
                        result.FailureReason = "Failed to verify backup";
                        Log.Message("Verify failed for " + mod.Identifier);
                    }
                } else {
                    Log.Message("Skipped verify");
                }
            } catch (Exception e) {
                result.FailureReason = e.Message;
            }

            Mods.Add(existing);
            Save();
            Page_ModsConfig_Controller.Notify_BackupListChanged();
            return result;
        }

        public IEnumerable<BackupListStorageData> GetAllMods(BackupsSearchBarOptions options) {
            foreach (var item in Mods) {
                if (options.SearchValue.NullOrEmpty()
                                || item.ModIdentifier.ToLower().Contains(options.SearchValue.ToLower())
                                || (!item.Name.NullOrEmpty() && item.Name.ToLower().Contains(options.SearchValue))) {
                    yield return item;
                }
            }
        }

        public BackupListStorageData GetBackupData(string modIdentifier) {
            return Mods.Find(backup => backup.ModIdentifier == modIdentifier);
        }

        public void Load() {
            modsContainer.Load();
            DebugHelper.DebugMessage("Loaded {0} backups", modsContainer.Data.Count);
        }

        public void Save() {
            modsContainer.Save();
        }

        public void StartBackupMods(List<ModMetaDataEnhanced> mods, bool verifyBackups = false) {
            LongEventHandler.QueueLongEvent(delegate {
                List<BackupResult> results = new List<BackupResult>();
                int currentIndex = 0;
                foreach (ModMetaDataEnhanced mod in mods) {
                    LongEventHandler.SetCurrentEventText(string.Format("Please Wait\nBacking up {0}\n({1}/{2})", mod.Name, currentIndex, mods.Count));
                    results.Add(Backup(mod, verifyBackups));
                    currentIndex++;
                }
                LongEventHandler.ExecuteWhenFinished(delegate {
                    Save();
                    Page_ModsConfig_Controller.Notify_BackupListChanged();
                    Page_ModsConfig_Controller.SetMessage("Created " + currentIndex + " backups", MessageTypeDefOf.PositiveEvent);
                    //Show backup results
                });
            }, "Backing up all mods", true, null);
        }

        public bool VerifyBackup(BackupStorageData backup, ModMetaDataEnhanced mod) {
            if (backup.ModHash == PathUtils.CreateDirectoryMd5(mod.RootDir))
                return true;
            return false;
        }

        internal void BackupAll(bool verifyOnFinish, bool includeCore = false, bool activeonly = true) {
            List<ModMetaDataEnhanced> mods = ModListController.Instance.ActiveModList;

            if (activeonly) {
                mods.RemoveAll(m => !m.Active);
            }

            if (!includeCore) {
                mods.RemoveAll(m => m.IsCoreMod);
            }
            StartBackupMods(mods, verifyOnFinish);
        }

        internal void ShowDeleteAllBackupsDialog() {
            int count = 0;
            foreach (var item in Mods) {
                foreach (var backup in item.ModBackupsList) {
                    count++;
                }
            }
            if (count == 0) {
                Page_ModsConfig_Controller.SetMessage("No backups to delete", MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DeleteAllBackupsDialog".Translate(count), () => {
                LongEventHandler.QueueLongEvent(delegate {
                    long totalLength = 0;
                    long remainingLength = 0;
                    foreach (var m in Mods) {
                        totalLength += m.TotalSize;
                    }
                    //remainingLength = totalLength;
                    foreach (var mod in Mods) {
                        LongEventHandler.SetCurrentEventText(string.Format("Please Wait\nDeleting All Mods\n({0}/{1})", PathUtils.GetBytesReadable(remainingLength), PathUtils.GetBytesReadable(totalLength)));
                        Directory.Delete(mod.Location, true);
                        remainingLength += mod.TotalSize;
                    }
                    LongEventHandler.ExecuteWhenFinished(delegate {
                        Mods.Clear();
                        Save();
                        Page_ModsConfig_Controller.Notify_BackupListChanged();
                    });
                }, "Deleting all mods", true, null);
            }, true));
        }
    }

    public class BackupResult {
        internal BackupResult(string modIdentifier, string failureReason = null, bool verified = false) {
            this.ModIdentifier = modIdentifier;
            this.Verified = verified;
            if (failureReason.NullOrEmpty()) {
                this.Success = true;
            } else {
                this.FailureReason = failureReason;
            }
        }

        public string FailureReason { get; internal set; }
        public string ModIdentifier { get; private set; }
        public long Size { get; internal set; }
        public bool Success { get; internal set; }
        public bool Verified { get; internal set; }
    }
}