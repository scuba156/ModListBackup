using RimToolsUI.ExtraWidgets;
using Harmony;
using ModListBackup.Core.Mods;
using ModListBackup.UI.Tabs;
using ModListBackup.Utils;
using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ModListBackup.UI {

    internal static class Page_ModsConfig_Controller {
        private static readonly TabBackups BackupsTab = new TabBackups();
        private static readonly TabConfig ConfigTab = new TabConfig();
        private static readonly TabModList ModListTab = new TabModList();
        private static readonly Vector2 StatusLabelSize = new Vector2(250f, 18f);
        private static readonly List<TabRecord> TabContent = new List<TabRecord>();
        private static readonly TabTools ToolsTab = new TabTools();
        private static TabBase CurrentTab;
        private static ModsConfigTab CurrentTabType;

        private static bool ForceRestart = false;

        static Page_ModsConfig_Controller() {
            CurrentTab = ModListTab;
        }

        private enum ModsConfigTab : byte {
            ModList,
            Backups,
            Config,
            Tools
        }

        internal static void DoWindowContents(this Page_ModsConfig _instance, Rect rect) {
            TextAnchor origAnchor = Text.Anchor;
            GameFont origFont = Text.Font;

            Rect mainRect = (Rect)AccessTools.Method(typeof(Page_ModsConfig), "GetMainRect").Invoke(_instance, new object[] { rect, 0f, true });
            GUI.BeginGroup(mainRect);
            // Tab View
            Rect tabViewRect = new Rect(0f, mainRect.yMin + 33f, 415f, mainRect.height - 33f);
            Widgets.DrawMenuSection(tabViewRect);
            DrawTabs(tabViewRect);

            // Search Bar
            Rect tabSearchBarRect = new Rect(tabViewRect.xMin, tabViewRect.xMin, tabViewRect.width, 100f);
            CurrentTab.DrawSearchBar(tabSearchBarRect);
            DebugHelper.DrawBoxAroundRect(tabSearchBarRect);

            //Tab Content
            Rect tabContentRect = new Rect(tabViewRect.xMin, tabSearchBarRect.yMax, tabViewRect.width, tabViewRect.height - tabSearchBarRect.height + 13f);
            CurrentTab.DrawMainContent(tabContentRect);
            DebugHelper.DrawBoxAroundRect(tabContentRect);

            // Extra Content
            Rect extraContent = new Rect(tabViewRect.xMax + 8f, tabViewRect.yMin, rect.width - tabViewRect.width - 8f, tabViewRect.height);
            CurrentTab.DrawExtraContent(extraContent);
            DebugHelper.DrawBoxAroundRect(extraContent);

            GUI.EndGroup();

            // Controls
            Rect controlsLeftRect = new Rect(rect.xMin, tabViewRect.yMax + 17f, tabViewRect.width, 33f);
            Rect ControlsRightRect = new Rect(extraContent.xMin, extraContent.yMax + 17f, extraContent.width, 33f);
            CurrentTab.DrawLeftControls(controlsLeftRect);
            CurrentTab.DrawRightControls(ControlsRightRect);
            DebugHelper.DrawBoxAroundRect(controlsLeftRect);
            DebugHelper.DrawBoxAroundRect(ControlsRightRect);

            // Close Button
            Rect closeRect = new Rect((rect.width / 2) - (ButtonWidgets.ButtonSmallWidth / 2), rect.yMax - Page.BottomButHeight, TabBase.ButtonSmallWidth, Page.BottomButHeight);
            string closeText = "CloseButton".Translate();
            DebugHelper.DrawBoxAroundRect(closeRect);
            int hash = (int)AccessTools.Field(_instance.GetType(), "activeModsWhenOpenedHash").GetValue(_instance);
            bool doRestart = ForceRestart;

            if (hash != ModLister.InstalledModsListHash(true)) {
                doRestart = true;
            }
            if (doRestart) {
                closeText = "RestartButton".Translate();
            }

            if (Widgets.ButtonText(closeRect, closeText)) {
                OnClose();
                if (doRestart) {
                    //Utils.ModsConfigUtils.SetActiveMods(ModListController.Instance.);
                    ModsConfig.Save();
                    ModsConfig.RestartFromChangedMods();
                } else {
                    _instance.Close();
                }
            }

            Text.Font = origFont;
            Text.Anchor = origAnchor;
        }

        internal static void ExtraOnGUI() {
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
                if (Event.current.keyCode == KeyCode.Tab) {
                    if (CurrentTab.GetType() == typeof(TabModList)) {
                        CurrentTab = BackupsTab;
                    } else if (CurrentTab.GetType() == typeof(TabBackups)) {
                        CurrentTab = ToolsTab;
                    } else if (CurrentTab.GetType() == typeof(TabTools)) {
                        CurrentTab = ConfigTab;
                    } else {
                        CurrentTab = ModListTab;
                    }
                    CurrentTab.OnTabSelect();

                    Event.current.Use();
                    return;
                }
            }

            CurrentTab.ExtraOnGUI();
        }

        internal static void Notify_BackupListChanged() {
            BackupsTab.Notify_BackupListChanged();
        }

        internal static void Notify_ModsListChanged() {
            ModListTab.Notify_ModsListChanged();
        }

        internal static void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid) {
            ModListTab.Notify_SteamItemUnsubscribed(pfid);
        }

        internal static void OnClose() {
            StopAllThreads();
        }

        internal static void PreOpen(Page_ModsConfig __instance) {
            __instance.doCloseButton = false;
            ModsConfigUtils.BackupCurrent();
        }

        internal static void SetCloseOnEscapeKey(bool value) {
            Find.WindowStack.WindowOfType<Page_ModsConfig>().closeOnCancel = value;
        }

        internal static void SetForceRestart(bool restart = true) {
            ForceRestart = restart;
        }
        internal static void SetMessage(string message, MessageTypeDef messageTypeDef) {
            Messages.Message(message, messageTypeDef);
        }

        private static void DrawTabs(Rect rect) {
            //if (TabContent != null)

            TabContent.Clear();
            TabContent.Add(new TabRecord("MainTab".Translate(), delegate {
                CurrentTab = ModListTab;
                CurrentTab.OnTabSelect();
            }, CurrentTab is TabModList));
            TabContent.Add(new TabRecord("BackupsTab".Translate(), delegate {
                CurrentTab = BackupsTab;
                CurrentTab.OnTabSelect();
            }, CurrentTab is TabBackups));
            TabContent.Add(new TabRecord("Tools".Translate(), delegate {
                CurrentTab = ToolsTab;
                CurrentTab.OnTabSelect();
            }, CurrentTab is TabTools));
            TabContent.Add(new TabRecord("ConfigTab".Translate(), delegate {
                CurrentTab = ConfigTab;
                CurrentTab.OnTabSelect();
            }, CurrentTab is TabConfig));

            TabDrawer.DrawTabs(rect, TabContent);
        }

        private static void StopAllThreads() {
            ModListTab.FileSizeUpdateThread.Abort();
        }
    }
}