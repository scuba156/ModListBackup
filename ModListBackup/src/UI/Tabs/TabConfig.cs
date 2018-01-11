using ExtraWidgets;
using Harmony;
using ModListBackup.SearchBars;
using ModListBackup.Settings;
using ModListBackup.UI.Tabs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ModListBackup.UI.Tabs {
    internal class TabConfig : TabBase {

        // Settings Needed
        // - BackupDirectory with paste clipboard button
        // - Edit colours button
        // - Backups count/storage limit
        // - Auto backup new/updated mods
        // - Steam sync states
        // - 
        // - 
        // - 



        private string _backupDirectory = string.Empty;
        private int _storageSizeLimit = 0;
        private int _stateSizeLimit = 0;
        private string _storageSizeLimitBuffer = string.Empty;
        private string _stateSizeLimitBuffer = string.Empty;
        private bool _storageSizeUseGb = false;
        private bool _autoBackupOnBoot = false;



        internal override void OnInit() {
            base.OnInit();
            SettingsHandler.Update();
            SearchBarOptions = new ConfigSearchBarOptions();

            if (SettingsHandler.ModBackupDirectory == null) {
                Log.Message("Settings is null");
            }

            _backupDirectory = SettingsHandler.ModBackupDirectory;
        }

        internal override void DrawExtraContent(Rect rect) {
            //throw new NotImplementedException();
        }

        internal override void DrawMainContent(Rect rect) {
            float height = 400f;//(float)(visibleModList.Count + 1) * 26;
            float padding = (height > rect.height) ? 16f : 4f;

            Rect outerRect = new Rect(rect.xMin, rect.yMin, rect.width - 2f, rect.height);
            Rect innerRect = new Rect(rect.xMin, rect.yMin, outerRect.width - padding, height);
            Widgets.BeginScrollView(outerRect, ref mainContentScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing.Begin(rect6);

            DrawBackupRelatedConfig(listing);
            DrawModRelatedConfig(listing);

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawModRelatedConfig(Listing_Standard listing) {
            // State limit
            //Rect stateLimitRow = listing.GetRect(24f);
            //Rect stateLimitLabelRect = new Rect(stateLimitRow.xMin, stateLimitRow.yMin, 100f, stateLimitRow.height);
            //Rect stateLimitValueRect = new Rect(stateLimitRow.xMin, stateLimitRow.yMin, 140f, stateLimitRow.height);

            listing.TextFieldNumericLabeled<int>("Maximum number of states:", ref _stateSizeLimit, ref _stateSizeLimitBuffer, 0, 100);
            listing.ButtonText("Edit Save State Names...");

            listing.GapLine();
        }

        private void DrawBackupRelatedConfig(Listing_Standard listing) {
            listing.Label("Backup Directory");

            Rect dirRowRect = listing.GetRect(24f);

            Rect dirEntryRect = new Rect(dirRowRect.xMin, dirRowRect.yMin, dirRowRect.width - (dirRowRect.height * 2), dirRowRect.height);
            Rect dirPasteClipboardRect = new Rect(dirEntryRect.xMax, dirRowRect.yMin, dirRowRect.height, dirRowRect.height);
            Rect dirOpenFolderRect = new Rect(dirPasteClipboardRect.xMax, dirRowRect.yMin, dirRowRect.height, dirRowRect.height);


            _backupDirectory = Widgets.TextArea(dirEntryRect, _backupDirectory);
            if (Widgets.ButtonText(dirPasteClipboardRect, "C")) {

            }

            if (Widgets.ButtonText(dirOpenFolderRect, "D")) {

            }

            Text.Anchor = TextAnchor.UpperRight;
            listing.CheckboxLabeled("Auto Backup Mod Updates", ref _autoBackupOnBoot, "Automatically backup mods that have updated when RimWorld starts.");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect storageLimitRow = listing.GetRect(24f);
            Rect storageLimitValueRect = new Rect(storageLimitRow.xMin, storageLimitRow.yMin, 40f, storageLimitRow.height);
            Rect storageLimitTypeMbRect = new Rect(storageLimitValueRect.xMax, storageLimitRow.yMin, 50f, storageLimitRow.height);
            Rect storageLimitTypeGbRect = new Rect(storageLimitTypeMbRect.xMax, storageLimitRow.yMin, 50f, storageLimitRow.height);


            Widgets.TextFieldNumeric(storageLimitValueRect, ref _storageSizeLimit, ref _storageSizeLimitBuffer, 0, 1024);

            if (_storageSizeUseGb == false && _storageSizeLimit >= 1024) {
                _storageSizeUseGb = true;
                _storageSizeLimit = 1;
                _storageSizeLimitBuffer = "1";
            }

            Text.Anchor = TextAnchor.UpperRight;

            if (Widgets.RadioButtonLabeled(storageLimitTypeGbRect, "Gb", _storageSizeUseGb)
                            || Widgets.RadioButtonLabeled(storageLimitTypeMbRect, "Mb", !_storageSizeUseGb)) {
                _storageSizeUseGb = !_storageSizeUseGb;
            }
            Text.Anchor = TextAnchor.UpperLeft;

            listing.GapLine();
        }

    }
}
