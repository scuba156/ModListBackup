using ExtraWidgets;
using ModListBackup.Core;
using ModListBackup.Mods;
using ModListBackup.Utils;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Dialogs {

    internal class Dialog_StartBackup : Window {
        internal Vector2 listScrollPosition = new Vector2();

        private List<ListItem> currentList = new List<ListItem>();
        private long selectedTotalSize;
        private bool verifyBackups;
        private Thread FileSizeUpdateThread;
        private volatile bool threadStop = false;
        public override Vector2 InitialSize => new Vector2(500f, 700f);
        private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();


        internal Dialog_StartBackup() {
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.onlyOneOfTypeAllowed = true;
            foreach (var mod in ModListController.Instance.ActiveModList) {
                currentList.Add(new ListItem(mod));
            }
        }

        public override void DoWindowContents(Rect inRect) {
            //Title
            Rect TitleRect = new Rect(0f, 0f, inRect.width, 48f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(TitleRect, "Backup Mods");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect DescRect = new Rect(inRect.xMin, TitleRect.yMax + 20f, inRect.width, 80f);
            Widgets.Label(DescRect, "InstallModDesc".Translate());

            float height = (currentList.Count + 1) * 24;

            Rect outerRect = new Rect(inRect.xMin, DescRect.yMax, inRect.width - 4f, inRect.height - DescRect.yMax - 95f);
            float padding = (height > outerRect.height) ? 16f : 4f;
            Rect innerRect = new Rect(inRect.xMin, outerRect.yMin, outerRect.width - padding, height);
            Widgets.BeginScrollView(outerRect, ref listScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing_Standard.Begin(rect6);

            foreach (var modItem in ModsInListOrder()) {
                DrawModItem(listing_Standard, modItem);
            }

            listing_Standard.End();
            Widgets.EndScrollView();


            //select all
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect selectAllRect = new Rect(outerRect.xMin, outerRect.yMax, 150f, 18f);
            DebugHelper.DrawBoxAroundRect(selectAllRect);
            string selectLabel = "select all";
            bool select = true;
            if (currentList.FindAll(m=>m.Checked).Count == currentList.Count) {
                selectLabel = "deselect all";
                select = false;
            }
            if (Widgets.ButtonText(selectAllRect, selectLabel, false, false, true)) {
                foreach (var item in currentList) {
                    item.Checked = select;
                }
                StartCalculateFileSize();
            }

            //selected size
            Text.Anchor = TextAnchor.UpperRight;
            Rect sizeRect = new Rect(outerRect.xMax - 150f, outerRect.yMax, 150f, 18f);
            DebugHelper.DrawBoxAroundRect(sizeRect);

            string label;

            if (FileSizeUpdateThread != null && FileSizeUpdateThread.IsAlive) {
                label = "Calculating size";
            } else {
                label = PathUtils.GetBytesReadable(selectedTotalSize);
            }

            Widgets.Label(sizeRect, label);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            // options
            Rect verifyRect = new Rect(outerRect.xMin, outerRect.yMax + 20f, outerRect.width, 30f);
            DebugHelper.DrawBoxAroundRect(verifyRect);
            Widgets.CheckboxLabeled(verifyRect, "Verify backups", ref verifyBackups);

            Rect bottomRect = new Rect(inRect.xMin, inRect.yMax - 40f, inRect.width, Page.BottomButHeight);
            DrawBottomButtons(bottomRect);

            DebugHelper.DrawBoxAroundRect(TitleRect);
            DebugHelper.DrawBoxAroundRect(DescRect);
            DebugHelper.DrawBoxAroundRect(outerRect);
            DebugHelper.DrawBoxAroundRect(bottomRect);
        }

        internal static void Create() {
            Find.WindowStack.Add(new Dialog_StartBackup());
        }

        private void DrawBottomButtons(Rect rect) {
            Rect closeRect = new Rect((rect.width / 2) - (ButtonWidgets.ButtonSmallWidth / 2), rect.yMin, ButtonWidgets.ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(closeRect, "CloseButton".Translate())) {
                threadStop = true;
                FileSizeUpdateThread = null;
                this.Close();
            }

            Rect startRect = new Rect(rect.xMax - ButtonWidgets.ButtonSmallWidth, rect.yMin, ButtonWidgets.ButtonSmallWidth, Page.BottomButHeight);
            if (Widgets.ButtonText(startRect, "StartButton".Translate())) {
                List<ModMetaDataEnhanced> mods = new List<ModMetaDataEnhanced>();
                foreach (var item in currentList.FindAll(m => m.Checked)) {
                    mods.Add(item.Mod);
                }
                BackupController.Instance.StartBackupMods(mods, verifyBackups);
                this.Close();
            }

            DebugHelper.DrawBoxAroundRect(closeRect);
            DebugHelper.DrawBoxAroundRect(startRect);
        }

        private void DrawModItem(Listing_Standard listing, ListItem item) {
            bool isChecked = item.Checked;

            float num = listing.ColumnWidth - 24f;
            string label = item.Mod.Name.Truncate(num, this.truncatedModNamesCache);

            listing.CheckboxLabeled(label, ref isChecked, null);
            if (item.Checked != isChecked) {
                item.Checked = isChecked;
                StartCalculateFileSize();
            }
        }

        private IEnumerable<ListItem> ModsInListOrder() {
            foreach (var item in from m in currentList
                                 orderby m.Mod.Name
                                 orderby m.Checked descending
                                 orderby m.Mod.IsCoreMod descending
                                 select m) {
                yield return item;
            }
        }

        private void StartCalculateFileSize() {
            if (FileSizeUpdateThread != null && FileSizeUpdateThread.ThreadState == ThreadState.Running) {
                FileSizeUpdateThread.Abort();
                FileSizeUpdateThread.Interrupt();
                threadStop = true;
            }
            FileSizeUpdateThread = new Thread(() => {
                long size = 0;
                foreach (var item in currentList.FindAll(m=>m.Checked)) {
                    if (threadStop || FileSizeUpdateThread.ThreadState == ThreadState.AbortRequested || FileSizeUpdateThread.ThreadState == ThreadState.StopRequested) {
                        Log.Message("aborted");
                        threadStop = false;
                        return;
                    }
                    size += PathUtils.GetDirectorySize(item.Mod.RootDir);
                }
                selectedTotalSize = size;
            });
            FileSizeUpdateThread.Start();
        }

        internal class ListItem {

            internal ListItem(ModMetaDataEnhanced mod) {
                Mod = mod;
            }
            private bool _checked;
            internal bool Checked { get { return _checked; } set { _checked = value; }
            }
            internal ModMetaDataEnhanced Mod { get; set; }
        }
    }
}