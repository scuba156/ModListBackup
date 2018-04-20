using ExtraWidgets;
using ExtraWidgets.FloatMenu;
using Harmony;
using ModListBackup.Core.Mods;
using ModListBackup.Mods.Notifications;
using ModListBackup.Settings;
using ModListBackup.StorageContainers;
using ModListBackup.UI.Dialogs;
using ModListBackup.UI.SearchBars;
using ModListBackup.Utils;
using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace ModListBackup.UI.Tabs {

    internal class TabModList : TabBase {
        internal Thread FileSizeUpdateThread;
        private int activeModsCount;
        private long installedModsSize;
        private Vector2 modDescriptionScrollPosition;
        private string renameBuf;
        private bool RenameInProgress;
        private ModMetaDataEnhanced selectedMod;
        private int selectedState = 1;
        private ModListSearchBarOptions SortingOptions = new ModListSearchBarOptions();
        private bool threadStop;
        private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();
        private List<ModMetaDataEnhanced> visibleModList = new List<ModMetaDataEnhanced>();

        public static IEnumerable<DirectoryInfo> AllActiveModDirs {
            get {
                return from mod in ModLister.AllInstalledMods
                       select mod.RootDir;
            }
        }

        private ModListStateStorageData CurrentStateMetaData { get; set; }
        private ModListStateStorageData PreviousStateMetaData { get; set; }

        internal override void DrawExtraContent(Rect rect) {
            if (selectedMod != null) {
                DrawModDetails(rect);
            }
        }

        internal override void DrawLeftControls(Rect rect) {
            // Save Button
            Rect saveStateRect = new Rect((rect.xMin - 1), rect.y, ButtonSmallWidth, Page.BottomButHeight);
            TooltipHandler.TipRegion(saveStateRect, "Button_Backup_Tooltip".Translate());
            if (Widgets.ButtonText(saveStateRect, "Button_Backup_Text".Translate()))
                ModListController.Instance.SaveState(selectedState, ModsConfigUtils.ConvertModListToActiveList(visibleModList));

            // '>>' Label
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect toRect = new Rect(saveStateRect.xMax + Padding, saveStateRect.y, LabelWidth, Page.BottomButHeight);
            Widgets.Label(toRect, ">>");

            // State button and Float menu
            Rect selectStateRect = new Rect(toRect.xMax + Padding, saveStateRect.y, ButtonTinyWidth, Page.BottomButHeight);
            TooltipHandler.TipRegion(selectStateRect, "Button_State_Select_Tooltip".Translate());
            if (Widgets.ButtonText(selectStateRect, string.Format("{0}{1}", selectedState.ToString(), (ModListController.ModListIsSet(selectedState)) ? null : "*"))) {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 1; i <= SettingsHandler.ModListStateLimit; i++) {
                    //set a new variable here, otherwise the selected state and button text will change when int i next iterates
                    int n = i;

                    options.Add(new FloatMenuOption(ModListController.GetFormattedModListName(i), (() => { selectedState = n; }), MenuOptionPriority.Default, null, null, 0.0f, null, null));
                }

                options.Add(new FloatMenuOption("EditNames...".Translate(), (() => { Find.WindowStack.Add(new Dialog_EditNames()); }), MenuOptionPriority.Default, null, null, 0f, null, null));

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            // '<<' Label
            Rect fromRect = new Rect(selectStateRect.xMax + Padding, selectStateRect.y, LabelWidth, Page.BottomButHeight);
            Widgets.Label(fromRect, "<<");

            // Load Button
            Rect loadStateRect = new Rect(fromRect.xMax + Padding, selectStateRect.y, ButtonSmallWidth, Page.BottomButHeight);
            TooltipHandler.TipRegion(loadStateRect, "Button_Restore_Tooltip".Translate());
            if (Widgets.ButtonText(loadStateRect, "Button_Restore_Text".Translate()))
                ModListController.Instance.LoadState(selectedState);

            // Undo Button
            Rect undoRect = new Rect(loadStateRect.xMax + Padding + 7f, loadStateRect.y, ButtonTinyWidth, Page.BottomButHeight);
            TooltipHandler.TipRegion(undoRect, "Button_Undo_Tooltip".Translate());
            if (ButtonWidgets.ButtonImage(undoRect, Textures.Undo, true, ModListController.Instance.CanRestorePreviousState))
                if (ModListController.Instance.CanRestorePreviousState)
                    ModListController.Instance.RestorePreviousState();
            //if (ModListController.Instance.CanRestorePreviousState) { Log.Message("can restore"); }

            // Undo Button
            Rect undoRect2 = new Rect(undoRect.xMax + Padding + 7f, undoRect.y, ButtonTinyWidth, Page.BottomButHeight);
            TooltipHandler.TipRegion(undoRect, "Button_Undo_Tooltip".Translate());
            if (ButtonWidgets.ButtonImage(undoRect2, Textures.Undo, true, ModListController.Instance.CanRestorePreviousState)) {
            }

            //Reset text
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        internal override void DrawMainContent(Rect rect) {
            DrawModList(rect);
        }

        // Right Side
        // upload
        // backup
        // delete/uninstall
        //
        //
        //
        internal override void DrawRightControls(Rect rect) {
            bool disabled = selectedMod == null;

            Rect uploadButton = new Rect(rect.xMax - ButtonBigWidth, rect.yMin, ButtonBigWidth, ButtonHeight);
            DebugHelper.DrawBoxAroundRect(uploadButton);
            if (Prefs.DevMode && SteamManager.Initialized && selectedMod != null && selectedMod.OriginalMetaData.CanToUploadToWorkshop()) {
                if (Widgets.ButtonText(uploadButton, Workshop.UploadButtonLabel(selectedMod.OriginalMetaData.GetPublishedFileId()), true, false, true)) {
                    if (!VersionControl.IsWellFormattedVersionString(selectedMod.OriginalMetaData.TargetVersion)) {
                        Messages.Message("MessageModNeedsWellFormattedTargetVersion".Translate(new object[]
                        {
                                VersionControl.CurrentVersionString
                        }), MessageTypeDefOf.RejectInput);
                    } else {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate {
                            SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                            Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate {
                                SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                                AccessTools.Method(typeof(Workshop), "Upload").Invoke(null, new object[] { selectedMod.OriginalMetaData });
                            }, true, null);
                            dialog_MessageBox.buttonAText = "Yes".Translate();
                            dialog_MessageBox.buttonBText = "No".Translate();
                            dialog_MessageBox.interactionDelay = 6f;
                            Find.WindowStack.Add(dialog_MessageBox);
                        }, true, null));
                    }
                }
            }
        }

        internal override void ExtraOnGUI() {
            if (RenameInProgress) {
                if ((Event.current.type == EventType.keyDown || Event.current.type == EventType.used) && (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return)) {
                    RenameInProgress = false;
                    selectedMod.CustomName = renameBuf;
                    Page_ModsConfig_Controller.SetCloseOnEscapeKey(true);
                } else
                    Page_ModsConfig_Controller.SetCloseOnEscapeKey(false);
            } else {
                if (Event.current.isMouse && Event.current.button == 1) {
                    if (selectedMod.OriginalMetaData.Source == ContentSource.SteamWorkshop) {
                        CreateContextFloatMenuSteam(ModListController.Instance.GetModEnhanced(selectedMod.OriginalMetaData)).Invoke();
                    } else {
                        CreateContextFloatMenuLocal(ModListController.Instance.GetModEnhanced(selectedMod.OriginalMetaData)).Invoke();
                    }
                }

                if (Event.current.type == EventType.keyDown || Event.current.type == EventType.used && selectedMod != null) {
                    bool reorder = Event.current.modifiers == (EventModifiers.Shift | EventModifiers.FunctionKey);

                    if (Event.current.keyCode == KeyCode.UpArrow) {
                        int selectedIndex = visibleModList.IndexOf(selectedMod);
                        if (selectedIndex > 0) {
                            if (reorder) {
                                if (selectedMod.Active && visibleModList[selectedIndex - 1].Active) {
                                    AccessTools.Method(typeof(ModsConfig), "Reorder").Invoke(null, new object[] { selectedIndex, selectedIndex - 1 });
                                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                                    UpdateVisibleModList();
                                }
                            } else {
                                selectedMod = visibleModList[selectedIndex - 1];
                                mainContentScrollPosition.y = visibleModList.IndexOf(selectedMod) * 23;
                            }
                        }
                    }
                    if (Event.current.keyCode == KeyCode.DownArrow) {
                        int selectedIndex = visibleModList.IndexOf(selectedMod);
                        if (selectedIndex < visibleModList.Count - 1) {
                            if (reorder) {
                                if (selectedMod.Active && visibleModList[selectedIndex + 1].Active) {
                                    AccessTools.Method(typeof(ModsConfig), "Reorder").Invoke(null, new object[] { selectedIndex, selectedIndex + 1 });
                                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                                    UpdateVisibleModList();
                                }
                            } else {
                                selectedMod = visibleModList[selectedIndex + 1];
                                mainContentScrollPosition.y = visibleModList.IndexOf(selectedMod) * 23;
                            }
                        }
                    }
                    if (Event.current.keyCode == KeyCode.Space) {
                        selectedMod.Active = !selectedMod.Active;
                        UpdateVisibleModList();
                    }
                    if (Event.current.control && Event.current.keyCode == KeyCode.R) {
                        StartRename(selectedMod);
                    }
                    if (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) {
                        Log.Message("delete");
                    }
                }
            }
        }

        internal void Notify_ModsListChanged() {
            UpdateVisibleModList();
            UpdateTotalSize();
        }

        internal void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid) {
            if (selectedMod != null && selectedMod.Identifier == pfid.ToString()) {
                selectedMod = visibleModList.FirstOrDefault();
            }
        }

        internal override void OnInit() {
            SearchBarOptions = new ModListSearchBarOptions();
            UpdateVisibleModList();
            UpdateTotalSize();
        }

        internal override void OnSearchBarChanged() {
            base.OnSearchBarChanged();
            UpdateVisibleModList();
        }

        private void BackupModList() {
            ModListController.Instance.SaveState(selectedState, ModsConfigUtils.ConvertModListToActiveList(visibleModList));
            Page_ModsConfig_Controller.SetMessage("Status_Message_Backup".Translate(), MessageTypeDefOf.PositiveEvent);
        }

        private void DrawModRowDownloading(Listing_Standard listing) {
            Rect rect = listing.GetRect(26f);
            ContentSourceUtility.DrawContentSource(rect, ContentSource.SteamWorkshop, null);
            rect.xMin += 28f;
            Widgets.Label(rect, "Downloading".Translate() + GenText.MarchingEllipsis(0f));
        }

        private Action CreateContextFloatMenuLocal(ModMetaDataEnhanced mod) {
            return new Action(() => {
                List<FloatMenuOption> options = new List<FloatMenuOption> {
                    new FloatMenuOption("Rename".Translate(), () => StartRename(mod) ),
                    new FloatMenuOptionColorPicker((color) => mod.TextColor = color, MenuOptionPriority.Default),
                    new FloatMenuOption("OpenDirectory".Translate(), () => HugsLib.Shell.ShellOpenDirectory.Execute(mod.RootDir.FullName)),
                    new FloatMenuOption("Backup", () => { }, MenuOptionPriority.Default)
                };
                if (!mod.Url.NullOrEmpty()) {
                    options.Add(new FloatMenuOption("OpenWebSite".Translate(), () => Application.OpenURL(mod.Url)));
                }

                if (!mod.IsCoreMod) {
                    options.Add(new FloatMenuOption("DeleteLocal".Translate(), () => RemoveMod(mod)));
                }
                Find.WindowStack.Add((Window)new FloatMenu(options));
            });
        }

        private Action CreateContextFloatMenuSteam(ModMetaDataEnhanced mod) {
            return new Action(() => {
                List<FloatMenuOption> options = new List<FloatMenuOption> {
                    new FloatMenuOption("InstallLocal".Translate(), () => Find.WindowStack.Add(new Dialog_InstallMod(mod))),
                    new FloatMenuOption("Rename".Translate(), () => StartRename(mod)),
                    new FloatMenuOptionColorPicker((color) => mod.TextColor = color, MenuOptionPriority.Default),
                    new FloatMenuOption("Backup", () => { }, MenuOptionPriority.Default),
                    new FloatMenuOption("OpenWorkshopPage".Translate(), () => { SteamUtility.OpenWorkshopPage(mod.OriginalMetaData.GetPublishedFileId()); }),
                    new FloatMenuOption("Unsubscribe".Translate(), () => { RemoveMod(mod); })
                };

                Find.WindowStack.Add((Window)new FloatMenu(options));
            });
        }

        private void DrawModDetails(Rect rect) {
            GUI.BeginGroup(rect);

            if (selectedMod != null) {
                Text.Font = GameFont.Medium;
                Rect rect7 = new Rect(0f, 0f, rect.width, 40f);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect7, selectedMod.OriginalMetaData.Name.Truncate(rect7.width, truncatedModNamesCache));
                Text.Anchor = TextAnchor.UpperLeft;
                if (!selectedMod.IsCoreMod) {
                    Rect rect8 = rect7;
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.LowerRight;
                    if (!selectedMod.OriginalMetaData.VersionCompatible) {
                        GUI.color = Color.red;
                    }
                    Widgets.Label(rect8, "ModTargetVersion".Translate(new object[]
                    {
                        selectedMod.OriginalMetaData.TargetVersion
                    }));
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }
                Rect position2 = new Rect(0f, rect7.yMax, 0f, 20f);
                if (selectedMod.OriginalMetaData.previewImage != null) {
                    position2.width = Mathf.Min((float)selectedMod.OriginalMetaData.previewImage.width, rect.width);
                    position2.height = (float)selectedMod.OriginalMetaData.previewImage.height * (position2.width / (float)selectedMod.OriginalMetaData.previewImage.width);
                    if (position2.height > 300f) {
                        position2.width *= 300f / position2.height;
                        position2.height = 300f;
                    }
                    position2.x = rect.width / 2f - position2.width / 2f;
                    GUI.DrawTexture(position2, selectedMod.OriginalMetaData.previewImage, ScaleMode.ScaleToFit);
                }
                Text.Font = GameFont.Small;
                float num3 = position2.yMax + 10f;
                if (!selectedMod.OriginalMetaData.Author.NullOrEmpty()) {
                    Rect rect9 = new Rect(0f, num3, rect.width / 2f, 25f);
                    Widgets.Label(rect9, "Author".Translate() + ": " + selectedMod.OriginalMetaData.Author);
                }
                if (!selectedMod.OriginalMetaData.Url.NullOrEmpty()) {
                    float num4 = Mathf.Min(rect.width / 2f, Text.CalcSize(selectedMod.OriginalMetaData.Url).x);
                    Rect rect10 = new Rect(rect.width - num4, num3, num4, 25f);
                    Text.WordWrap = false;
                    if (Widgets.ButtonText(rect10, selectedMod.OriginalMetaData.Url, false, false, true)) {
                        Application.OpenURL(selectedMod.OriginalMetaData.Url);
                    }
                    Text.WordWrap = true;
                }
                WidgetRow widgetRow = new WidgetRow(rect.width, num3 + 25f, UIDirection.LeftThenUp, 99999f, 4f);
                if (SteamManager.Initialized && selectedMod.OriginalMetaData.OnSteamWorkshop) {
                    if (widgetRow.ButtonText("Unsubscribe", null, true, false)) {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(new object[]
                        {
                            selectedMod.OriginalMetaData.Name
                        }), delegate {
                            selectedMod.OriginalMetaData.enabled = false;
                            AccessTools.Method(typeof(Workshop), "Unsubscribe").Invoke(null, new object[] { selectedMod.OriginalMetaData });
                            Notify_SteamItemUnsubscribed(selectedMod.OriginalMetaData.GetPublishedFileId());
                        }, true, null));
                    }
                    if (widgetRow.ButtonText("WorkshopPage".Translate(), null, true, false)) {
                        SteamUtility.OpenWorkshopPage(selectedMod.OriginalMetaData.GetPublishedFileId());
                    }
                } else {
                    if (!selectedMod.IsCoreMod && widgetRow.ButtonText("Uninstall", null, true, false)) {
                    }
                }
                if (widgetRow.ButtonText("Open Folder", null, true, false)) {
                }
                if (widgetRow.ButtonText("Rename", null, true, false)) {
                    StartRename(selectedMod);
                }
                if (!selectedMod.IsCoreMod && widgetRow.ButtonText("Backup", null, true, false)) {
                }
                float num5 = num3 + 25f + 24f;
                Rect outRect = new Rect(0f, num5, rect.width, rect.height - num5);
                float width = outRect.width - 16f;
                Rect rect11 = new Rect(0f, 0f, width, Text.CalcHeight(selectedMod.OriginalMetaData.Description, width));
                Widgets.BeginScrollView(outRect, ref modDescriptionScrollPosition, rect11, true);
                Widgets.Label(rect11, selectedMod.OriginalMetaData.Description);
                Widgets.EndScrollView();
            }
            GUI.EndGroup();
        }

        private void DrawModList(Rect rect) {
            float height = (float)(visibleModList.Count + 1) * 26;
            float padding = (height > rect.height) ? 16f : 4f;

            Rect outerRect = new Rect(rect.xMin, rect.yMin, rect.width - 2f, rect.height);
            Rect innerRect = new Rect(rect.xMin, rect.yMin, outerRect.width - padding, height);
            Widgets.BeginScrollView(outerRect, ref mainContentScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing_Standard.Begin(rect6);

            int reorderableGroup = -1;

            if (SortingOptions == null) {
                Log.Message("sorting options is null");
            }

            if (SortingOptions.SearchValue.NullOrEmpty()) {
                reorderableGroup = ReorderableWidget.NewGroup(delegate (int from, int to) {
                    AccessTools.Method(typeof(ModsConfig), "Reorder").Invoke(null, new object[] { from, to });
                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                    UpdateVisibleModList();
                });
            }
            if (visibleModList.Count == 0) {
                listing_Standard.Label("No mods found!");
            } else {
                float selectedPos = 0f;

                foreach (ModMetaDataEnhanced current in visibleModList) {
                    DrawModRow(listing_Standard, current, reorderableGroup);
                    if (current.Identifier == selectedMod.Identifier) {
                        selectedPos = listing_Standard.CurHeight;
                    }
                }
                for (int i = 0; i < WorkshopItems.DownloadingItemsCount; i++) {
                    DrawModRowDownloading(listing_Standard);
                }
            }

            listing_Standard.End();
            Widgets.EndScrollView();

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;

            // Active Count label
            Rect activeCountRect = new Rect(rect.xMin + 4f, rect.yMax, 120f, 18f);
            DebugHelper.DrawBoxAroundRect(activeCountRect);

            string activeCountLabel = string.Format("{0}/{1} Mods", activeModsCount, visibleModList.Count);
            Widgets.Label(activeCountRect, activeCountLabel);

            // Size label
            Rect sizeRect = new Rect(rect.xMax - 80f - 4f, rect.yMax, 80f, 18f);
            DebugHelper.DrawBoxAroundRect(sizeRect);

            string label;
            if (FileSizeUpdateThread != null && FileSizeUpdateThread.IsAlive) {
                label = "Calculating size";
            } else {
                label = PathUtils.GetBytesReadable(installedModsSize);
            }

            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(sizeRect, label);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawModRow(Listing_Standard listing, ModMetaDataEnhanced mod, int reorderableGroup = -1) {
            if (mod == null || listing == null) {
                Log.Message("mod or listing is null");
                return;
            }
            Rect rect = listing.GetRect(26f);
            if (mod.Active && SortingOptions.SearchValue.NullOrEmpty()) {
                //|| SortingOptions.SortOptions.SortBy.Contains(SortByValue.LoadOrder))) {
                ReorderableWidget.Reorderable(reorderableGroup, rect);
            }
            Action clickAction = null;
            if (mod.Source == ContentSource.SteamWorkshop) {
                clickAction = CreateContextFloatMenuSteam(mod);
            } else {
                clickAction = CreateContextFloatMenuLocal(mod);
            }

            ContentSourceUtility.DrawContentSource(rect, mod.Source, clickAction);
            rect.xMin += 28f;
            bool active = mod.Active;
            bool isSelected = mod.Identifier == selectedMod.Identifier;
            Rect rect2 = rect;

            if (mod.Enabled && RenameInProgress == false) {
                string text = string.Empty;
                float num = rect2.width - 24f;
                if (mod.Active) {
                    //TODO: add notification tooltips and remove drag to reorder if not drawn
                    text = text + "DragToReorder".Translate() + ".\n\n";
                    num -= 24f;
                }
                if (!text.NullOrEmpty()) {
                    TooltipHandler.TipRegion(rect2, new TipSignal(text, mod.GetHashCode() * 3311));
                }
                Color origColor = GUI.color;
                GUI.color = mod.TextColor;
                string label = mod.Name.Truncate(num, this.truncatedModNamesCache);
                if (Widgets.CheckboxLabeledSelectable(rect2, label, ref isSelected, ref active)) {
                    selectedMod = mod;
                }
                GUI.color = origColor;

                if (mod.Active && reorderableGroup >= 0) {
                    Rect texRect = new Rect(rect2.xMax - 48f + 2f, rect2.y, 24f, 24f);
                    GUI.DrawTexture(texRect, Textures.DragHash);
                }

                if (mod.HasNotifications) {
                    Color notifColour = Color.gray;
                    string tooltipString = string.Empty;

                    foreach (NotificationBase notification in mod.Notifications) {
                        if (notification.MessageType == NotificationType.Error)
                            notifColour = Color.red;
                        else if (notification.MessageType == NotificationType.Warning && notifColour != Color.red)
                            notifColour = Color.yellow;

                        tooltipString += notification.MessageText + Environment.NewLine + Environment.NewLine;
                    }

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    GUI.color = notifColour;

                    Rect dupRect = new Rect(rect2.xMax - 72f + 2f, rect.y, 24f, 24f);
                    Widgets.Label(dupRect, "!");
                    TooltipHandler.ClearTooltipsFrom(dupRect);
                    TooltipHandler.TipRegion(dupRect, tooltipString.Trim());

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    GUI.color = Color.white;
                }
                if (mod.Active && !active && mod.Name == ModContentPack.CoreModIdentifier) {
                    ModMetaData coreMod = mod.OriginalMetaData;
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisableCoreMod".Translate(), delegate {
                        coreMod.Active = false;
                        truncatedModNamesCache.Clear();
                        UpdateVisibleModList();
                    }, false, null));
                } else {
                    if (mod.Active != active) {
                        mod.Active = active;
                        UpdateVisibleModList();
                    }
                    truncatedModNamesCache.Clear();
                }
            } else if (RenameInProgress == true && isSelected) {
                renameBuf = Widgets.TextField(rect2, renameBuf);
            } else {
                GUI.color = Color.gray;
                Widgets.Label(rect2, mod.Name);
            }
            GUI.color = Color.white;
        }

        private void EnsureSelectedModVisible() {
            int selectedIndex = visibleModList.IndexOf(selectedMod);
            mainContentScrollPosition.y = (selectedIndex) * 23;
        }

        private void RemoveMod(ModMetaDataEnhanced mod) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUninstall".Translate(mod.Name), (Action)(() => {
                ModUtils.UnInstallMod(mod);
            }), true, "AreYouSure".Translate()));
        }

        private void StartRename(ModMetaDataEnhanced mod) {
            selectedMod = mod;
            renameBuf = selectedMod.Name;
            RenameInProgress = true;
        }

        private void UpdateTotalSize() {
            if (FileSizeUpdateThread != null && FileSizeUpdateThread.ThreadState == ThreadState.Running) {
                FileSizeUpdateThread.Abort();
                FileSizeUpdateThread.Interrupt();
                threadStop = true;
            }
            FileSizeUpdateThread = new Thread(() => {
                long size = 0;
                foreach (var item in visibleModList) {
                    if (threadStop || FileSizeUpdateThread.ThreadState == ThreadState.AbortRequested || FileSizeUpdateThread.ThreadState == ThreadState.StopRequested) {
                        DebugHelper.DebugMessage("FileSizeUpdate thread aborted");
                        threadStop = false;
                        return;
                    }
                    size += PathUtils.GetDirectorySize(item.RootDir);
                }
                installedModsSize = size;
            });
            FileSizeUpdateThread.Start();
        }

        private void UpdateVisibleModList() {
            visibleModList = ModListController.Instance.ModsInSortedOrder((ModListSearchBarOptions)SearchBarOptions).ToList();
            if (selectedMod == null || !visibleModList.Contains(selectedMod)) {
                selectedMod = visibleModList.FirstOrDefault();
            } else {
                selectedMod = visibleModList.FirstOrDefault(m => m.Identifier == selectedMod.Identifier);
            }
            activeModsCount = visibleModList.FindAll(m => m.Active).Count;
            EnsureSelectedModVisible();
        }
    }
}