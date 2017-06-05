using ExtraWidgets;
using ExtraWidgets.FloatMenu;
using Harmony;
using ModListBackup.Controllers;
using ModListBackup.Controllers.Settings;
using ModListBackup.Mods;
using ModListBackup.Mods.Notifications;
using ModListBackup.UI.Dialogs;
using RimWorld;
using RimWorld.Planet;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;
using static ModListBackup.UI.Common;

namespace ModListBackup.UI {

    internal static class Page_ModsConfig_Controller {
        public static bool RenameInProgress;
        private static readonly float BottomHeight = 40f;
        private static readonly float BottomRightContentOffset = 109f;
        private static readonly Vector2 LabelStatusSize = new Vector2(100f, 18f);
        private static readonly float LabelWidth = 20f;
        private static readonly float Padding = 5f;
        private static readonly Color[] textColors = new Color[] { Color.white, Color.red, Color.green, Color.blue, Color.yellow };
        private static List<ModMetaDataEnhanced> loadedMods;
        private static Vector2 modDescriptionScrollPosition;
        private static Vector2 modListScrollPosition;
        private static string renameBuf;
        private static string searchString;
        private static ModMetaDataEnhanced selectedMod;
        private static int selectedModlist = 1;
        private static Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();
        private static bool SearchInProgress { get { return !searchString.NullOrEmpty(); } }
        private static string StatusMessage { get; set; }

        private static int StatusMessageDelay { get; set; }

        internal static void DoWindowContents(this Page_ModsConfig instance, Rect rect) {
            Rect mainRect = (Rect)AccessTools.Method(typeof(Page_ModsConfig), "GetMainRect").Invoke(instance, new object[] { rect, 0f, true });
            GUI.BeginGroup(mainRect);
            Text.Font = GameFont.Small;
            float num = 0f;
            Rect rect2 = new Rect(17f, num, 316f, 30f);
            if (Widgets.ButtonText(rect2, "OpenSteamWorkshop".Translate(), true, false, true)) {
                SteamUtility.OpenSteamWorkshopPage();
            }
            num += 30f;
            Rect rect3 = new Rect(17f, num, 316f, 30f);
            if (Widgets.ButtonText(rect3, "GetModsFromForum".Translate(), true, false, true)) {
                Application.OpenURL("http://rimworldgame.com/getmods");
            }
            num += 30f;
            num += 17f;

            // Modlist

            Rect rect4 = new Rect(0f, num, 350f, mainRect.height - num);
            Widgets.DrawMenuSection(rect4, true);

            float height = (float)(ModLister.AllInstalledMods.Count<ModMetaData>() * 34 + 300);
            Rect rect5 = new Rect(0f, 26f, rect4.width - 16f, height);
            Widgets.BeginScrollView(rect4, ref modListScrollPosition, rect5, true);
            Rect rect6 = rect5.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = rect6.width;
            listing_Standard.Begin(rect6);

            Rect searchBox = listing_Standard.GetRect(26f);

            searchString = TextEntryWidgets.TextEntryWithPlaceHolder(searchBox, searchString, "SearchPlaceHolder".Translate());

            int reorderableGroup = -1;

            if (!SearchInProgress) {
                reorderableGroup = ReorderableWidget.NewGroup(delegate (int from, int to) {
                    AccessTools.Method(typeof(ModsConfig), "Reorder").Invoke(null, new object[] { from, to });
                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                });
            }
            int num2 = 0;
            foreach (ModMetaDataEnhanced current in ModsInListOrder()) {
                DoModRow(listing_Standard, current, num2, reorderableGroup);
                num2++;
            }
            for (int i = 0; i < WorkshopItems.DownloadingItemsCount; i++) {
                DoModRowDownloading(listing_Standard, num2);
                num2++;
            }
            listing_Standard.End();
            Widgets.EndScrollView();

            // selected mod details
            Rect position = new Rect(rect4.xMax + 17f, 0f, mainRect.width - rect4.width - 17f, mainRect.height);
            GUI.BeginGroup(position);
            if (selectedMod != null) {
                Text.Font = GameFont.Medium;
                Rect rect7 = new Rect(0f, 0f, position.width, 40f);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect7, selectedMod.OriginalMetaData.Name.Truncate(rect7.width, null));
                Text.Anchor = TextAnchor.UpperLeft;
                if (!selectedMod.OriginalMetaData.IsCoreMod) {
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
                    position2.width = Mathf.Min((float)selectedMod.OriginalMetaData.previewImage.width, position.width);
                    position2.height = (float)selectedMod.OriginalMetaData.previewImage.height * (position2.width / (float)selectedMod.OriginalMetaData.previewImage.width);
                    if (position2.height > 300f) {
                        position2.width *= 300f / position2.height;
                        position2.height = 300f;
                    }
                    position2.x = position.width / 2f - position2.width / 2f;
                    GUI.DrawTexture(position2, selectedMod.OriginalMetaData.previewImage, ScaleMode.ScaleToFit);
                }
                Text.Font = GameFont.Small;
                float num3 = position2.yMax + 10f;
                if (!selectedMod.OriginalMetaData.Author.NullOrEmpty()) {
                    Rect rect9 = new Rect(0f, num3, position.width / 2f, 25f);
                    Widgets.Label(rect9, "Author".Translate() + ": " + selectedMod.OriginalMetaData.Author);
                }
                if (!selectedMod.OriginalMetaData.Url.NullOrEmpty()) {
                    float num4 = Mathf.Min(position.width / 2f, Text.CalcSize(selectedMod.OriginalMetaData.Url).x);
                    Rect rect10 = new Rect(position.width - num4, num3, num4, 25f);
                    Text.WordWrap = false;
                    if (Widgets.ButtonText(rect10, selectedMod.OriginalMetaData.Url, false, false, true)) {
                        Application.OpenURL(selectedMod.OriginalMetaData.Url);
                    }
                    Text.WordWrap = true;
                }
                WidgetRow widgetRow = new WidgetRow(position.width, num3 + 25f, UIDirection.LeftThenUp, 99999f, 4f);
                if (SteamManager.Initialized && selectedMod.OriginalMetaData.OnSteamWorkshop) {
                    if (widgetRow.ButtonText("Unsubscribe", null, true, false)) {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(new object[]
                        {
                            selectedMod.OriginalMetaData.Name
                        }), delegate {
                            selectedMod.OriginalMetaData.enabled = false;
                            AccessTools.Method(typeof(Workshop), "Unsubscribe").Invoke(null, new object[] { selectedMod.OriginalMetaData });
                            AccessTools.Method(typeof(Page_ModsConfig), "Notify_SteamItemUnsubscribed").Invoke(instance, new object[] { selectedMod.OriginalMetaData.GetPublishedFileId() });
                        }, true, null));
                    }
                    if (widgetRow.ButtonText("WorkshopPage".Translate(), null, true, false)) {
                        SteamUtility.OpenWorkshopPage(selectedMod.OriginalMetaData.GetPublishedFileId());
                    }
                }
                float num5 = num3 + 25f + 24f;
                Rect outRect = new Rect(0f, num5, position.width, position.height - num5 - 40f);
                float width = outRect.width - 16f;
                Rect rect11 = new Rect(0f, 0f, width, Text.CalcHeight(selectedMod.OriginalMetaData.Description, width));
                Widgets.BeginScrollView(outRect, ref modDescriptionScrollPosition, rect11, true);
                Widgets.Label(rect11, selectedMod.OriginalMetaData.Description);
                Widgets.EndScrollView();
                if (Prefs.DevMode && SteamManager.Initialized && selectedMod.OriginalMetaData.CanToUploadToWorkshop()) {
                    Rect rect12 = new Rect(0f, position.yMax - 40f, 200f, 40f);
                    if (Widgets.ButtonText(rect12, Workshop.UploadButtonLabel(selectedMod.OriginalMetaData.GetPublishedFileId()), true, false, true)) {
                        if (!VersionControl.IsWellFormattedVersionString(selectedMod.OriginalMetaData.TargetVersion)) {
                            Messages.Message("MessageModNeedsWellFormattedTargetVersion".Translate(new object[]
                            {
                                VersionControl.CurrentVersionString
                            }), MessageSound.RejectInput);
                        }
                        else {
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
            GUI.EndGroup();
            GUI.EndGroup();

            DoBottomLeftWindowContents(rect);
            DoBottomRightWindowContents(rect);
        }

        internal static void ExtraOnGUI(this Page_ModsConfig instance) {
            if (RenameInProgress) {
                if ((Event.current.type == EventType.keyDown || Event.current.type == EventType.keyDown || Event.current.type == EventType.used) && (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return)) {
                    RenameInProgress = false;
                    selectedMod.AltName = renameBuf;
                }
            }
            else {
                if (Event.current.isMouse && Event.current.button == 1) {
                    if (selectedMod.OriginalMetaData.Source == ContentSource.SteamWorkshop) {
                        CreateModRowActionSteam(GetModEnhanced(selectedMod.OriginalMetaData)).Invoke();
                    }
                    else {
                        CreateModRowActionLocal(GetModEnhanced(selectedMod.OriginalMetaData)).Invoke();
                    }
                }
            }
        }
        internal static void Notify_ModsListChanged() {
            UpdateLoadedMods();
            string selModId = selectedMod.Identifier;
            selectedMod = loadedMods.FirstOrDefault(m => m.Identifier == selModId);
        }

        internal static void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid) {
            if (selectedMod != null && selectedMod.Identifier == pfid.ToString()) {
                selectedMod = null;
            }
        }

        internal static void PreOpen(this Page_ModsConfig instance) {
            loadedMods = new List<ModMetaDataEnhanced>();
            UpdateLoadedMods();
            selectedMod = loadedMods.FirstOrDefault(mod => mod.Identifier == instance.selectedMod.Identifier);
        }

        internal static void SetStatus(string message, Status_Delay delay = Status_Delay.longDelay) {
            StatusMessage = message;
            if (delay == Status_Delay.longDelay)
                StatusMessageDelay = STATUS_DELAY_TICKS_LONG;
            else
                StatusMessageDelay = STATUS_DELAY_TICKS_SHORT;
        }

        private static void BackupModList() {
            ModListController.SaveModList(selectedModlist);
            SetStatus("Status_Message_Backup".Translate());
        }

        private static Action CreateModRowActionLocal(ModMetaDataEnhanced mod) {
            return new Action(() => {
                List<FloatMenuOption> options = new List<FloatMenuOption> {
                    new FloatMenuOption("Rename".ToString(), () => StartRename(mod) ),
                    new FloatMenuOptionColorPicker((color) => mod.TextColor = color, MenuOptionPriority.Default, textColors),
                    new FloatMenuOption("OpenDirectory".Translate(), () => HugsLib.Shell.ShellOpenDirectory.Execute(mod.RootDir.FullName))
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

        private static Action CreateModRowActionSteam(ModMetaDataEnhanced mod) {
            return new Action(() => {
                List<FloatMenuOption> options = new List<FloatMenuOption> {
                    new FloatMenuOption("InstallLocal".Translate(), () => Find.WindowStack.Add(new InstallModDialog(mod))),
                    new FloatMenuOption("Rename".ToString(), () => StartRename(mod)),
                    new FloatMenuOptionColorPicker((color) => mod.TextColor = color, MenuOptionPriority.Default, textColors),
                    new FloatMenuOption("OpenWorkshopPage".Translate(), () => { SteamUtility.OpenWorkshopPage(mod.OriginalMetaData.GetPublishedFileId()); }),
                    new FloatMenuOption("Unsubscribe".Translate(), () => { RemoveMod(mod); })
                };

                Find.WindowStack.Add((Window)new FloatMenu(options));
            });
        }

        private static void DoBottomLeftWindowContents(Rect rect) {
            // Backup Button
            Rect BackupRect = new Rect((rect.xMin - 1), rect.yMax - 37f, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(BackupRect, "Button_Backup_Tooltip".Translate());
            if (Widgets.ButtonText(BackupRect, "Button_Backup_Text".Translate()))
                BackupModList();

            // '>>' Label
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect toRect = new Rect(BackupRect.xMax + Padding, BackupRect.y, LabelWidth, BottomHeight);
            Widgets.Label(toRect, ">>");

            // State button and Float menu
            Rect StateRect = new Rect(toRect.xMax + Padding, BackupRect.y, ButtonSmallWidth, BottomHeight);
            TooltipHandler.TipRegion(StateRect, "Button_State_Select_Tooltip".Translate());
            if (Widgets.ButtonText(StateRect, string.Format("{0}{1}", selectedModlist.ToString(), (ModListController.ModListIsSet(selectedModlist)) ? null : "*"))) {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 1; i <= SettingsHandler.STATE_LIMIT; i++) {
                    //set a new variable here, otherwise the selected state and button text will change when int i next iterates
                    int n = i;
                    options.Add(new FloatMenuOption(ModListController.GetFormattedModListName(i), (Action)(() => { selectedModlist = n; }), MenuOptionPriority.Default, (Action)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                }

                options.Add(new FloatMenuOption("Edit Names...", (Action)(() => { Find.WindowStack.Add(new Dialogs.Dialog_EditNames()); }), MenuOptionPriority.Default, (Action)null, (Thing)null, 0f, null, null));

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            // '<<' Label
            Rect fromRect = new Rect(StateRect.xMax + Padding, StateRect.y, LabelWidth, BottomHeight);
            Widgets.Label(fromRect, "<<");

            // Restore Button
            Rect RestoreRect = new Rect(fromRect.xMax + Padding, StateRect.y, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(RestoreRect, "Button_Restore_Tooltip".Translate());
            if (Widgets.ButtonText(RestoreRect, "Button_Restore_Text".Translate()))
                ModListController.RestoreModList(selectedModlist);

            // Undo Button
            Rect UndoRect = new Rect(RestoreRect.xMax + Padding + 7f, RestoreRect.y, ButtonSmallWidth, BottomHeight);
            TooltipHandler.TipRegion(UndoRect, "Button_Undo_Tooltip".Translate());
            if (ButtonWidgets.ButtonImage(UndoRect, Textures.Undo))
                if (ModListController.CanUndo)
                    if (ModListController.DoUndoAction())
                        SetStatus("Status_Message_Undone".Translate());

            // Status Label
            DoStatusMessageTick();
            Text.Font = GameFont.Tiny;
            Rect StatusRect = new Rect(StateRect.x - 25f, StateRect.yMax - 58f, LabelStatusSize.x, LabelStatusSize.y);
            Widgets.Label(StatusRect, StatusMessage);

            //Reset text
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DoBottomRightWindowContents(Rect rect) {
            // Import Button
            Rect ImportRect = new Rect(rect.xMax - BottomRightContentOffset, rect.yMax - 37f, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(ImportRect, "Button_Import_Tooltip".Translate());
            if (Widgets.ButtonText(ImportRect, "Button_Import_Text".Translate())) {
                Dialogs.Dialog_Import importWindow = new Dialogs.Dialog_Import();
                Find.WindowStack.Add(importWindow);
            }
        }

        private static void DoModRow(Listing_Standard listing, ModMetaDataEnhanced mod, int index, int reorderableGroup = -1) {
            Rect rect = listing.GetRect(26f);
            if (mod.Active && !SearchInProgress) {
                ReorderableWidget.Reorderable(reorderableGroup, rect);
            }
            Action clickAction = null;
            if (mod.Source == ContentSource.SteamWorkshop) {
                clickAction = CreateModRowActionSteam(mod);
            }
            else {
                clickAction = CreateModRowActionLocal(mod);
            }
            ContentSourceUtility.DrawContentSource(rect, mod.Source, clickAction);
            rect.xMin += 28f;
            bool active = mod.Active;
            bool isSelected = mod.OriginalMetaData == selectedMod.OriginalMetaData;
            Rect rect2 = rect;
            if (mod.Enabled && RenameInProgress == false) {
                string text = string.Empty;
                if (mod.Active) {
                    text = text + "DragToReorder".Translate() + ".\n\n";
                }
                if (!text.NullOrEmpty()) {
                    TooltipHandler.TipRegion(rect2, new TipSignal(text, mod.GetHashCode() * 3311));
                }
                Color origColor = GUI.color;
                GUI.color = mod.TextColor;
                if (Widgets.CheckboxLabeledSelectable(rect2, mod.Name, ref isSelected, ref active)) {
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

                        tooltipString += notification.MessageText + "\n\n";
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
                    }, false, null));
                }
                else {
                    mod.Active = active;
                    truncatedModNamesCache.Clear();
                }
            }
            else if (RenameInProgress == true && isSelected) {
                renameBuf = Widgets.TextField(rect2, renameBuf);
            }
            else {
                GUI.color = Color.gray;
                Widgets.Label(rect2, mod.Name);
            }
            GUI.color = Color.white;
        }

        private static void DoModRowDownloading(Listing_Standard listing, int index) {
            Rect rect = listing.GetRect(26f);
            ContentSourceUtility.DrawContentSource(rect, ContentSource.SteamWorkshop, null);
            rect.xMin += 28f;
            Widgets.Label(rect, "Downloading".Translate() + GenText.MarchingEllipsis(0f));
        }

        private static void DoStatusMessageTick() {
            if (StatusMessageDelay > 0)
                StatusMessageDelay--;
            if (StatusMessageDelay == 0) {
                StatusMessage = string.Empty;
                StatusMessageDelay = -1;
            }
        }

        private static void DrawSearchBoxRow(Listing_Standard listing) {
            Rect searchBox = listing.GetRect(26f);

            searchString = TextEntryWidgets.TextEntryWithPlaceHolder(searchBox, searchString, "SearchPlaceHolder".Translate());
        }

        private static ModMetaDataEnhanced GetModByIdentifier(ModMetaData mod) {
            return loadedMods.First(m => m.Identifier == mod.Identifier);
        }

        private static ModMetaDataEnhanced GetModEnhanced(ModMetaData mod) {
            return loadedMods.First(m => m.OriginalMetaData == mod);
        }

        private static IEnumerable<ModMetaDataEnhanced> ModsInListOrder() {
            if (SearchInProgress) {
                foreach (ModMetaDataEnhanced mod in loadedMods.Where(m => m.Name.ToLower().Contains(searchString.ToLower()) || m.Identifier.ToLower().Contains(searchString.ToLower()))) {
                    yield return mod;
                }
            }
            else {
                foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder) {
                    yield return GetModByIdentifier(mod);
                }
                foreach (ModMetaDataEnhanced mod2 in from m in loadedMods
                                                     orderby m.Name ascending
                                                     orderby m.VersionCompatible descending
                                                     //orderby m.OnSteamWorkshop ascending
                                                     where !m.Active
                                                     select m) {
                    yield return mod2;
                }
            }
        }

        private static void RemoveMod(ModMetaDataEnhanced mod) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUninstall".Translate(mod.Name), (Action)(() => {
                ModController.UnInstallMod(mod);
            }), true, "AreYouSure".Translate()));
        }

        private static void SetCloseOnEscapeKey(bool value) {
            Find.WindowStack.WindowOfType<Page_ModsConfig>().closeOnEscapeKey = value;
        }

        private static void StartRename(ModMetaDataEnhanced mod) {
            selectedMod = mod;
            renameBuf = selectedMod.Name;
            RenameInProgress = true;
        }
        private static void UpdateLoadedMods() {
            loadedMods.Clear();
            foreach (var mod in ModLister.AllInstalledMods) {
                loadedMods.Add(new ModMetaDataEnhanced(mod));
            }
            Main.Log.Message("There are now {0} loaded mods", loadedMods.Count);
        }
    }
}