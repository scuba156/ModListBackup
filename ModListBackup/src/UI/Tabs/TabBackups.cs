using ExtraWidgets;
using ModListBackup.Core;
using ModListBackup.Mods;
using ModListBackup.StorageContainers;
using ModListBackup.UI.Dialogs;
using ModListBackup.UI.SearchBars;
using ModListBackup.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Tabs {

    internal class TabBackups : TabBase {
        private static readonly List<TabRecord> tabsList = new List<TabRecord>();
        private static SelectedModTab curTab = SelectedModTab.Backups;
        private Vector2 backupListScrollPosition;
        private BackupStorageData selectedBackup;
        private BackupListStorageData selectedMod;
        private ModMetaDataEnhanced selectedModMetaData;
        private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();
        private IEnumerable<BackupListStorageData> visibleList = new List<BackupListStorageData>();
        private enum SelectedModTab : byte {
            Backups,
            Settings
        }

        internal override void DrawExtraContent(Rect rect) {
            DoSelectedMod(rect);
        }

        internal override void ExtraOnGUI() {
            base.ExtraOnGUI();
            if (Event.current.isMouse && Event.current.button == 1 && selectedMod != null ) {
                CreateContextFloatMenuMod(selectedMod).Invoke();
            }
        }

        private Action CreateContextFloatMenuMod(BackupListStorageData backup) {
            return new Action(() => {
                List<FloatMenuOption> options = new List<FloatMenuOption> {
                    new FloatMenuOption("OpenDirectory".Translate(), () => HugsLib.Shell.ShellOpenDirectory.Execute(backup.Location)),
                    new FloatMenuOption("Rename", () => { }),
                    new FloatMenuOption("Delete", () => { BackupController.Instance.DeleteAllModsBackups(backup); })
                };

                Find.WindowStack.Add((Window)new FloatMenu(options));
            });
        }

        internal override void DrawLeftControls(Rect rect) {
            base.DrawLeftControls(rect);
            Rect BackupRect = new Rect((rect.xMin - 1), rect.y, ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(BackupRect, "Backup All")) {
                Page_ModsConfig_Controller.SetMessage("Backing up now. Please wait...", MessageTypeDefOf.NeutralEvent);
                Dialog_StartBackup.Create();
            }

            Rect deleteAllRect = new Rect(BackupRect.xMax + Padding, BackupRect.y, ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(deleteAllRect, "Delete All")) {
                BackupController.Instance.ShowDeleteAllBackupsDialog();
            }

            Rect restoreRect = new Rect(deleteAllRect.xMax + Padding, BackupRect.y, ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(restoreRect, "Restore")) {
                Dialog_RestoreBackup.Create(selectedMod, selectedBackup.Id);
            }
        }

        internal override void DrawMainContent(Rect rect) {
            long totalSize = 0;
            float height = (float)((visibleList.Count() * 24 + 1) + 10);
            if (height <= 24f) {
                height += 24f;
            }
            float padding = (height > rect.height) ? 16f : 4f;
            Rect outerRect = new Rect(rect.xMin, rect.yMin, rect.width - 2f, rect.height);
            Rect innerRect = new Rect(rect.xMin, rect.yMin, outerRect.width - padding, height);
            Widgets.BeginScrollView(outerRect, ref mainContentScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing_Standard.Begin(rect6);

            if (!visibleList.Any()) {
                listing_Standard.Label("No Backups Found!");
                selectedMod = null;
            } else {
                foreach (var backup in visibleList) {
                    DrawModRow(backup, listing_Standard);
                    totalSize += backup.TotalSize;
                }
            }
            listing_Standard.End();
            Widgets.EndScrollView();

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperRight;
            Rect sizeRect = new Rect(rect.xMax - 80f - 4f, rect.yMax, 80f, 18f);
            DebugHelper.DrawBoxAroundRect(sizeRect);
            Widgets.Label(sizeRect, (PathUtils.GetBytesReadable(totalSize)));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        internal override void OnInit() {
            base.OnInit();
            SearchBarOptions = new BackupsSearchBarOptions();
            UpdateVisibleList();
        }

        internal override void OnSearchBarChanged() {
            base.OnSearchBarChanged();
            UpdateVisibleList();
        }

        private static void DrawTabs(Rect rect) {
            tabsList.Clear();
            tabsList.Add(new TabRecord("BackupsTab".Translate(), delegate {
                curTab = SelectedModTab.Backups;
            }, curTab == SelectedModTab.Backups));
            tabsList.Add(new TabRecord("SettingsTab".Translate(), delegate {
                curTab = SelectedModTab.Settings;
            }, curTab == SelectedModTab.Settings));

            TabDrawer.DrawTabs(rect, tabsList);
        }

        private void DrawBackupRow(BackupStorageData backup, Listing_Standard listing) {
            bool selected = backup == selectedBackup;

            if (listing.LabelSelectable(backup.CreationDate.ToString() + " - " + PathUtils.GetBytesReadable(backup.Size), ref selected)) {
                UpdateSelectedBackup(backup);
            }
        }

        private void DrawModRow(BackupListStorageData mod, Listing_Standard listing) {
            bool selected = mod == selectedMod;

            Rect rect = listing.GetRect(26f);
            ContentSourceUtility.DrawContentSource(rect, mod.Source, CreateContextFloatMenuMod(mod));
            rect.xMin += 28f;

            float num = rect.width - 24f;
            string label = mod.Name.Truncate(num, this.truncatedModNamesCache);


            if (ExtraWidgets.TextWidgets.LabelSelectable(rect, label, ref selected)) {
                UpdateSelectedMod(mod);
            }
        }

        private void DoSelectedMod(Rect rect) {
            if (selectedMod != null) {
                Rect backupsListRect = new Rect(rect.xMin, rect.yMax - 150f, rect.width, 150f);
                DebugHelper.DrawBoxAroundRect(backupsListRect);
                Widgets.DrawMenuSection(backupsListRect);
                DrawBackupsList(backupsListRect);

                Rect modDetailsRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height - backupsListRect.height - 40f);
                DebugHelper.DrawBoxAroundRect(modDetailsRect);
                DrawModDetails(modDetailsRect);
            }
        }

        private void DrawBackupsList(Rect rect) {
            float height = (float)(selectedMod.ModBackupsList.Count * 5 + 80);
            float padding = (height > rect.height) ? 16f : 4f;
            DrawTabs(rect);
            Rect innerRect = new Rect(0f, 26f, rect.width - padding, height);
            Widgets.BeginScrollView(rect, ref backupListScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing_Standard.Begin(rect6);

            foreach (var backup in selectedMod.ModBackupsList) {
                DrawBackupRow(backup, listing_Standard);
            }

            listing_Standard.End();
            Widgets.EndScrollView();
        }

        private void DrawModDetails(Rect rect) {
            GUI.BeginGroup(rect);

            if (selectedBackup != null && selectedModMetaData != null) {
                Text.Font = GameFont.Medium;
                Rect rect7 = new Rect(0f, 0f, rect.width, 40f);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect7, selectedModMetaData.OriginalMetaData.Name.Truncate(rect7.width, truncatedModNamesCache));
                Text.Anchor = TextAnchor.UpperLeft;
                if (!selectedModMetaData.OriginalMetaData.IsCoreMod) {
                    Rect rect8 = rect7;
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.LowerRight;
                    if (!selectedModMetaData.OriginalMetaData.VersionCompatible) {
                        GUI.color = Color.red;
                    }
                    Widgets.Label(rect8, "ModTargetVersion".Translate(new object[]
                    {
                        selectedModMetaData.OriginalMetaData.TargetVersion
                    }));
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }
                Rect position2 = new Rect(0f, rect7.yMax, 0f, 20f);
                if (selectedModMetaData.OriginalMetaData.previewImage != null) {
                    position2.width = Mathf.Min((float)selectedModMetaData.OriginalMetaData.previewImage.width, rect.width);
                    position2.height = (float)selectedModMetaData.OriginalMetaData.previewImage.height * (position2.width / (float)selectedModMetaData.OriginalMetaData.previewImage.width);
                    if (position2.height > 300f) {
                        position2.width *= 300f / position2.height;
                        position2.height = 300f;
                    }
                    position2.x = rect.width / 2f - position2.width / 2f;
                    GUI.DrawTexture(position2, selectedModMetaData.OriginalMetaData.previewImage, ScaleMode.ScaleToFit);
                }
                Text.Font = GameFont.Small;
                float num3 = position2.yMax + 10f;
                if (!selectedModMetaData.OriginalMetaData.Author.NullOrEmpty()) {
                    Rect rect9 = new Rect(0f, num3, rect.width / 2f, 25f);
                    Widgets.Label(rect9, "Author".Translate() + ": " + selectedModMetaData.OriginalMetaData.Author);
                }
                if (!selectedModMetaData.OriginalMetaData.Url.NullOrEmpty()) {
                    float num4 = Mathf.Min(rect.width / 2f, Text.CalcSize(selectedModMetaData.OriginalMetaData.Url).x);
                    Rect rect10 = new Rect(rect.width - num4, num3, num4, 25f);
                    Text.WordWrap = false;
                    if (Widgets.ButtonText(rect10, selectedModMetaData.OriginalMetaData.Url, false, false, true)) {
                        Application.OpenURL(selectedModMetaData.OriginalMetaData.Url);
                    }
                    Text.WordWrap = true;
                }
                WidgetRow widgetRow = new WidgetRow(rect.width, num3 + 25f, UIDirection.LeftThenUp, 99999f, 4f);
            }
            GUI.EndGroup();
        }

        private void UpdateSelectedBackup(BackupStorageData backup) {
            if (backup == null) {
                selectedBackup = null;
                selectedModMetaData = null;
                return;
            }

            if (selectedBackup == null || selectedBackup != backup) {
                selectedBackup = backup;
                selectedModMetaData = new ModMetaDataEnhanced(new ModMetaData(Path.Combine(selectedMod.Location, selectedBackup.Id.ToString())));

                if (selectedModMetaData == null) {
                    Log.Message("Failed to get mod metadata");
                }
            }
        }

        private void UpdateSelectedMod(BackupListStorageData mod) {
            if (mod == null) {
                selectedMod = null;
                UpdateSelectedBackup(null);
                return;
            }

            if (selectedMod == null || selectedMod.ModIdentifier != mod.ModIdentifier) {
                selectedMod = mod;
                if (selectedMod == null) {
                    selectedBackup = null;
                    selectedModMetaData = null;
                } else {
                    UpdateSelectedBackup(selectedMod.ModBackupsList.FirstOrDefault());
                }
            }
        }
        private void UpdateVisibleList() {
            visibleList = BackupController.Instance.GetAllMods((BackupsSearchBarOptions)SearchBarOptions);

            if (visibleList != null && selectedMod == null || !visibleList.Contains(selectedMod)) {
                UpdateSelectedMod(visibleList.FirstOrDefault());
            } else {
                UpdateSelectedMod(null);
            }
        }
    }
}