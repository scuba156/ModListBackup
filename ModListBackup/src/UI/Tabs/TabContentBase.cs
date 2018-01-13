using ExtraWidgets;
using ModListBackup.UI.SearchBars;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Tabs {

    internal abstract class TabBase {
        public static readonly float ButtonBigWidth = 160f;
        public static readonly float ButtonHeight = 38f;
        public static readonly float ButtonMediumWidth = 140f;
        public static readonly float ButtonSmallWidth = 100f;
        public static readonly float ButtonTinyWidth = 40f;
        internal readonly float LabelWidth = 20f;
        internal readonly float Padding = 5f;
        internal Vector2 mainContentScrollPosition = new Vector2();
        private static float FilterStringWidth;
        private static float SortStringWidth;
        internal TabBase() {
            Init();
        }

        internal bool ControlsVisible { get; set; }
        internal bool ExtraContentVisible { get; set; }
        internal bool IsVisible { get; set; }
        internal SearchBarOptionsBase SearchBarOptions { get; set; }

        internal abstract void DrawExtraContent(Rect rect);

        internal virtual void DrawLeftControls(Rect rect) {
        }

        internal abstract void DrawMainContent(Rect rect);

        internal virtual void DrawRightControls(Rect rect) {
        }

        internal void DrawSearchBar(Rect rect) {
            float ypos = 52f;
            SortStringWidth = Text.CalcSize("Sort".Translate()).x;
            FilterStringWidth = Text.CalcSize("Filter".Translate()).x;

            Rect searchBox = new Rect(8f, ypos, rect.width - 16f, 26f);
            string searchString = TextEntryWidgets.TextEntryWithPlaceHolder(searchBox, SearchBarOptions.SearchValue, "SearchPlaceHolder".Translate());

            if (searchString != SearchBarOptions.SearchValue) {
                SearchBarOptions.SearchValue = searchString;
                OnSearchBarChanged();
            }

            //TODO: advanced search options

            ypos += searchBox.height + 5f;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperRight;
            GUI.color = Color.grey;
            //Rect advancedSearchRect = new Rect(searchBox.xMax - 58f, ypos - 4f, 75f, 18f);
            //if (Widgets.ButtonText(advancedSearchRect, "advanced", false, false, true)) {
            //}

            if (SearchBarOptions.ShowSortFilterOptions) {
                // Filter/Sort options
                Rect rowRect = new Rect(0f, ypos, rect.width - 4f, 18f);
                ypos += rowRect.height;

                Rect sortRect = new Rect(rowRect.xMin + 10f, rowRect.y, SortStringWidth + 8f, rowRect.height);
                Rect filterRect = new Rect(sortRect.xMax + 10f, rowRect.y, FilterStringWidth + 8f, rowRect.height);

                Text.Font = GameFont.Tiny;
                GUI.color = Color.grey;
                if (ButtonWidgets.ButtonDropDown(sortRect, "Sort".Translate(), false)) {
                    SearchBarOptions.SortOptions.ShowFloatMenu(OnSearchBarChanged);
                }

                if (ButtonWidgets.ButtonDropDown(filterRect, "Filter".Translate(), false)) {
                    SearchBarOptions.Filter.ShowFloatMenu(OnSearchBarChanged);
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        internal virtual void ExtraOnGUI() {
        }

        internal virtual void OnInit() {
        }

        internal virtual void OnSearchBarChanged() {
        }

        internal virtual void OnTabSelect() {
        }

        private void Init() {
            OnInit();
        }
    }
}