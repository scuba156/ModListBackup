using ModListBackup.SearchBars;
using ModListBackup.UI.Tools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Tabs {

    internal class TabTools : TabBase {
        private List<ToolUIContentBase> DebugRelatedTools;
        private List<ToolUIContentBase> BackupRelatedTools;
        private List<ToolUIContentBase> ModListRelatedTools;
        private ToolUIContentBase SelectedTool;

        private IEnumerable<ToolUIContentBase> AllTools {
            get {
                foreach (var item in DebugRelatedTools) {
                    yield return item;
                }
                foreach (var item in BackupRelatedTools) {
                    yield return item;
                }
                foreach (var item in ModListRelatedTools) {
                    yield return item;
                }
            }
        }

        private int toolCooldown;

        internal override void OnInit() {
            SearchBarOptions = new ToolsSearchBarOptions();
            ModListRelatedTools = new List<ToolUIContentBase> {
                new ImportFromSave(),
                new EnableOnlyCore(),
                new EnableOnlyCoreWithModList(),
                new CleanUninstalledFromSave(),
                new EditColors(),
                new RemoveAllColors(),
                new DeleteAllMods()
            };

            BackupRelatedTools = new List<ToolUIContentBase> {
                new BackupAllMods(),
                new BackupActiveMods(),
                new DeleteAllBackups(),
                new DeleteBackupsOlderThan(3),
                new DeleteBackupsOlderThan(6),
                new MoveBackupsToDefaultPath()
            };

            DebugRelatedTools = new List<ToolUIContentBase> {
#if DEBUG
                new DebugRestartRimWorld(),
                new DebugToggleRectLines()
#endif
            };
        }

        internal override void DrawExtraContent(Rect rect) {
            GameFont origFont = Text.Font;
            TextAnchor origAnchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Rect contentRect = rect;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(contentRect, "Tools".Translate());
            Text.Anchor = origAnchor;
            float yPos = rect.yMin + 40f;

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(contentRect.xMin, yPos, contentRect.width, contentRect.height - yPos);
            Widgets.Label(descriptionRect, "ToolsTabDescription".Translate());

            Text.Font = origFont;

            if (SelectedTool != null) {
                DrawSelectedToolDesription(rect);
            }
        }

        internal override void OnSearchBarChanged() {
            base.OnSearchBarChanged();
            foreach (var tool in AllTools) {
                if (SearchBarOptions.SearchValue.NullOrEmpty() || tool.Name.ToLower().Contains(SearchBarOptions.SearchValue.ToLower())) {
                    tool.IsVisible = true;
                } else {
                    tool.IsVisible = false;
                }
            }
        }

        internal override void DrawMainContent(Rect rect) {
            //TODO: Show warning icon and disable search if translation not available
            float height = (float)((ModListRelatedTools.Count + BackupRelatedTools.Count + DebugRelatedTools.Count) * 33f + (3 * 33f) + 52);
            float padding = (height > rect.height) ? 16f : 4f;
            Rect innerRect = new Rect(0f, rect.yMin, rect.width - padding, height);
            Widgets.BeginScrollView(rect, ref mainContentScrollPosition, innerRect, true);
            Rect rect6 = innerRect.ContractedBy(4f);
            Listing_Standard listing_Standard = new Listing_Standard {
                ColumnWidth = rect6.width
            };
            listing_Standard.Begin(rect6);
            //listing_Standard.Gap();

            if (DebugRelatedTools.Count > 0) {
                DrawToolListSection("Debug Tools", DebugRelatedTools, listing_Standard);
            }
            DrawToolListSection("ModListToolsTitle".Translate(), ModListRelatedTools, listing_Standard);
            DrawToolListSection("BackupToolsTitle".Translate(), BackupRelatedTools, listing_Standard);

            listing_Standard.End();
            Widgets.EndScrollView();
        }

        private void DrawSelectedToolDesription(Rect position) {
            GameFont origFont = Text.Font;
            TextAnchor origAnchor = Text.Anchor;
            Color origColor = GUI.color;
            Widgets.DrawMenuSection(position);

            // Name
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect nameRect = new Rect(position.xMin, position.yMin, position.width, 30f);
            Widgets.Label(nameRect, SelectedTool.Name);

            // Description
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect descRect = new Rect(position.xMin, nameRect.yMax + 20f, position.width, 80f);
            Widgets.Label(descRect, SelectedTool.Description);


            bool onCooldown = true;
            if (toolCooldown > 0) {
                toolCooldown--;
                onCooldown = false;
                GUI.color = Color.gray;
            }
            Rect startRect = new Rect(position.xMax - 85f, position.yMax - 35f, 80f, 30f);
            if (Widgets.ButtonText(startRect, "StartButton".Translate(), true, true, onCooldown)) {
                SelectedTool.Start();
                toolCooldown = 420;
            }
            GUI.color = origColor;

            Text.Font = origFont;
            Text.Anchor = origAnchor;
        }

        private void DrawToolListSection(string sectionTitle, List<ToolUIContentBase> tools, Listing_Standard listing_Standard) {
            listing_Standard.Label(sectionTitle);
            listing_Standard.GapLine();

            //TODO: Show/hide section button

            foreach (var toolContent in tools) {
                if (toolContent.IsVisible && listing_Standard.ButtonText(toolContent.ButtonText)) {
                    if (toolContent.InstantStart == true)
                        toolContent.Start();
                    else
                        SelectedTool = toolContent;
                }
            }
            listing_Standard.Gap();
        }
    }
}