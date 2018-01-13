using ExtraWidgets;
using ModListBackup.Core;
using ModListBackup.StorageContainers;
using ModListBackup.Utils;
using RimWorld;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Dialogs {

    internal class Dialog_RestoreBackup : Window {
        private BackupListStorageData BackupData;
        private ModMetaDataEnhanced ExistingMod;
        private int SelectedBackup;
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

            if (ExistingMod != null) {
                if (listing.RadioButton("Override current", true)) {
                }
            }

            if (listing.RadioButton("Install As New", false)) {
            }

            listing.End();

            // backup details
            Listing_Standard detailsListing = new Listing_Standard();
            detailsListing.Begin(new Rect(inRect.xMin, inRect.yMax - 150f, inRect.width, 150f));
            Text.Font = GameFont.Tiny;

            detailsListing.Label("Name: " + BackupData.Name);
            detailsListing.Label("Size: " + BackupData.ModBackupsList[SelectedBackup].CreationDate.ToLongDateString());
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
            }

            DebugHelper.DrawBoxAroundRect(closeRect);
            DebugHelper.DrawBoxAroundRect(startRect);
        }

        private void StartVerifyFilesThread() {
            verifyFilesThread = new Thread(() => {
                string genHash = PathUtils.CreateDirectoryMd5(Path.Combine(BackupData.Location, SelectedBackup.ToString()));
                Log.Message(string.Format("comparing generated {0} | {1}", genHash, BackupData.ModBackupsList[SelectedBackup].ModHash));
                verifiedFiles = genHash.Equals(BackupData.ModBackupsList[SelectedBackup].ModHash);
            });
            verifyFilesThread.Start();
        }
    }
}