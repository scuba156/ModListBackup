using ExtraWidgets;
using ModListBackup.Core;
using ModListBackup.StorageContainers;
using ModListBackup.Utils;
using RimWorld;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Dialogs {

    internal class Dialog_RestoreBackup : Window {
        private BackupListStorageData BackupData;
        private ModMetaDataEnhanced ExistingMod;
        private bool OverwriteCurrent = false;
        private int SelectedBackup;
        private string NewName;
        private bool verifiedFiles = false;
        private Thread verifyFilesThread;

        internal Dialog_RestoreBackup(BackupListStorageData backupData, int selectedBackup) {
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.onlyOneOfTypeAllowed = true;

            ExistingMod = Core.ModListController.Instance.GetModEnhancedByIdentifier(backupData.ModIdentifier);
            BackupData = backupData;
            SelectedBackup = selectedBackup;
            NewName = BackupData.Name;
        }

        public override Vector2 InitialSize => new Vector2(500f, 700f);

        public override void DoWindowContents(Rect inRect) {
            //Title
            Rect TitleRect = new Rect(0f, 0f, inRect.width, 48f);
            Rect TitleRect2 = new Rect(0f, 48f, inRect.width, 48f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(TitleRect, "Restore Backup");
            //Widgets.Label(TitleRect2, BackupData.Name + " " + SelectedBackup);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Tiny;

            Text.Font = GameFont.Small;
            Rect DescRect = new Rect(inRect.xMin, TitleRect.yMax + 20f, inRect.width, 80f);
            Widgets.Label(DescRect, "InstallModDesc".Translate());

            //Options
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(new Rect(inRect.xMin, DescRect.yMax + 10f, inRect.width, inRect.height - DescRect.yMax + 10f));

            // Install as new
            var origColor = GUI.color;
            if (OverwriteCurrent) {
                GUI.color = Color.gray;
            }
            NewName = listing.TextEntryLabeled("Name:", NewName);
            GUI.color = origColor;
            if (listing.RadioButton("Install As New", !OverwriteCurrent)) {
                OverwriteCurrent = !OverwriteCurrent;
            }

            // Overwrite Current
            bool enabled = false;

            if(ExistingMod != null && ExistingMod.OriginalMetaData.Source == ContentSource.LocalFolder) {
                enabled = true;
            }

            if (listing.RadioButton("Override current", OverwriteCurrent, enabled)) {
                OverwriteCurrent = !OverwriteCurrent;
            }
            

            listing.End();

            // backup details
            Listing_Standard detailsListing = new Listing_Standard();
            detailsListing.Begin(new Rect(inRect.xMin, inRect.yMax - 150f, inRect.width, 150f));
            Text.Font = GameFont.Tiny;

            detailsListing.Label("Name: " + BackupData.Name);
            detailsListing.Label("Created: " + BackupData.ModBackupsList[SelectedBackup].CreationDate.ToLongDateString());
            //detailsListing.Label("Location: " + BackupData.Location);
            detailsListing.Label("MD5: " + BackupData.ModBackupsList[SelectedBackup].ModHash);

            string verifiedLabel;

            if (verifyFilesThread.IsAlive) {
                verifiedLabel = "checking...";
            } else {
                verifiedLabel = verifiedFiles.ToString();
            }

            detailsListing.Label(String.Format("Size: {0} | Verified: {1}", PathUtils.GetBytesReadable(BackupData.ModBackupsList[SelectedBackup].Size), verifiedLabel));
            detailsListing.GapLine();
            detailsListing.End();
            Text.Font = GameFont.Small;

            Rect bottomRect = new Rect(inRect.xMin, inRect.yMax - 40f, inRect.width, Page.BottomButHeight);
            DrawBottomButtons(bottomRect);
        }

        public override void PreOpen() {
            base.PreOpen();

            StartVerifyFilesThread();
        }

        internal static void Create(BackupListStorageData backupData, int selectedBackup) {
            Find.WindowStack.Add(new Dialog_RestoreBackup(backupData, selectedBackup));
        }

        private void DrawBottomButtons(Rect rect) {
            Rect closeRect = new Rect((rect.width / 2) - (ButtonWidgets.ButtonSmallWidth / 2), rect.yMin, ButtonWidgets.ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(closeRect, "CloseButton".Translate())) {
                this.Close();
            }

            Rect startRect = new Rect(rect.xMax - ButtonWidgets.ButtonSmallWidth, rect.yMin, ButtonWidgets.ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(startRect, "RestoreButton".Translate())) {
                if (OverwriteCurrent) {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are You Sure?", () => { StartRestore(); }, true));
                } else {
                    if (ModLister.AllInstalledMods.Where(m=>m.Name == NewName || m.Identifier == NewName.Replace(" ", "")).Count() == 0) {
                        StartRestore();
                    } else {
                        Messages.Message(String.Format("'{0}' already exists.", NewName), MessageTypeDefOf.RejectInput);
                    }
                }

                DebugHelper.DrawBoxAroundRect(closeRect);
                DebugHelper.DrawBoxAroundRect(startRect);
            }
        }

        private void StartRestore() {

            if (OverwriteCurrent) {
                ModUtils.OverwriteMod(ExistingMod, Path.Combine(BackupData.Location, SelectedBackup.ToString()));
                this.Close();
            } else {
                var path = Path.Combine(BackupData.Location, SelectedBackup.ToString());
                var mod = ModUtils.InstallMod(path, NewName);
                if (BackupController.Instance.VerifyBackup(BackupData.ModBackupsList[SelectedBackup], mod) == false) {
                    Find.WindowStack.Add(new Dialog_MessageBox("Verify failed for " + NewName, "OK", null ));
                }
            }

            this.Close();
        }

        private void StartVerifyFilesThread() {
            verifyFilesThread = new Thread(() => {
                string genHash = PathUtils.GenDirectoryMd5(Path.Combine(BackupData.Location, SelectedBackup.ToString()));
                DebugHelper.DebugMessage(string.Format("generated hashes {0} | {1}", genHash, BackupData.ModBackupsList[SelectedBackup].ModHash));
                verifiedFiles = genHash.Equals(BackupData.ModBackupsList[SelectedBackup].ModHash);
            });
            verifyFilesThread.Start();
        }
    }
}