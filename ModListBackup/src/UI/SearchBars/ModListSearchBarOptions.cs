using RimToolsUI.FloatMenus;
using System;
using System.Collections.Generic;
using Verse;

namespace ModListBackup.UI.SearchBars {

    public class ModListFilterOptions : FilterOptionsBase {
        public ModListFilterOptions() => SetShowAll(true);

        public bool ShowDisabled { get; set; }
        public bool ShowEnabled { get; set; }
        public bool ShowHidden { get; set; }
        public bool ShowIncompatible { get; set; }
        public bool ShowLocal { get; set; }
        public bool ShowSteam { get; set; }

        public override void SetShowAll(bool value) {
            Log.Message("Setting Show all to " + value);
            ShowAll = value;
            //if (value) {
                ShowDisabled = value;
                ShowEnabled = value;
                ShowIncompatible = value;
                ShowLocal = value;
                ShowSteam = value;
            //}
        }

        private void update() {
            if(ShowDisabled && 
                ShowEnabled && 
                //ShowHidden && 
                ShowIncompatible && 
                ShowLocal && 
                ShowSteam) {
                SetShowAll(true);
            } else {
                ShowAll = false;
            }
        }

        public override void ShowFloatMenu(Action OnChanged) {
            List<FloatMenuOption> options = new List<FloatMenuOption> {
                new FloatMenuOptionCheckBox("All", (value) => { SetShowAll(value); OnChanged(); }, ShowAll),
                new FloatMenuOptionCheckBox("Incompatible", (value) => { ShowIncompatible = value; update(); OnChanged(); }, ShowIncompatible),
                new FloatMenuOptionCheckBox("Disabled", (value) => { ShowDisabled = value; update(); OnChanged(); }, ShowDisabled),
                new FloatMenuOptionCheckBox("Enabled", (value) => { ShowEnabled = value; update(); OnChanged(); }, ShowEnabled),
                new FloatMenuOptionCheckBox("Local", (value) => { ShowLocal = value; update(); OnChanged(); }, ShowLocal),
                new FloatMenuOptionCheckBox("Steam", (value) => { ShowSteam = value; update(); OnChanged(); }, ShowSteam)
            };
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    public class ModListSearchBarOptions : SearchBarOptionsBase {
        public ModListSearchBarOptions() {
            Filter = new ModListFilterOptions();
            SortOptions = new ModListSortOptions();
        }

        public override bool ShowSortFilterOptions => true;
    }

    public enum SortByValue { Alphabetical, Color, Descending, LoadOrder, Source }

    public class ModListSortOptions : SortOptionsBase {
        public ModListSortOptions() {
            SortBy = new List<Enum>();
        }

        public bool SortByAlphabetical { get; set; }
        public bool SortByColor { get; set; }
        public bool SortByLoadOrder { get { return SortBy.Contains(SortByValue.LoadOrder); } set { if (value && !SortBy.Contains(SortByValue.LoadOrder)) SortBy.Add(SortByValue.LoadOrder); else SortBy.RemoveAll(i => i.Equals(SortByValue.LoadOrder)); } }
        public bool SortBySource { get; set; }
        public bool SortDescending { get; set; }
        public override void ShowFloatMenu(Action OnChanged) {
            var options = new List<FloatMenuOption>();
            var radioOptionsGroup = new FloatMenuRadioOptionsGroup();

            radioOptionsGroup.AddOption("Load Order", () => { SortByLoadOrder = !SortByLoadOrder; OnChanged(); }, SortByLoadOrder);
            radioOptionsGroup.AddOption("Alphabetical", () => { SortByAlphabetical = !SortByAlphabetical; OnChanged(); }, SortByAlphabetical);
            radioOptionsGroup.AddOption("Color", () => { SortByColor = !SortByColor; OnChanged(); }, SortByColor);
            radioOptionsGroup.AddOption("Source", () => { SortBySource = ! SortBySource; }, SortBySource);

            options.AddRange(radioOptionsGroup.options);

            options.Add(new FloatMenuOptionCheckBox("Sort Descending", (value) => { SortDescending = value; OnChanged(); }, SortDescending));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void Clear() {
            SortByAlphabetical = false;
            SortByColor = false;
            SortByLoadOrder = false;
            SortBySource = false;
            SortDescending = false;
        }
    }
}