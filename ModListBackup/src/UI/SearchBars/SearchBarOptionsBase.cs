using RimToolsUI.FloatMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ModListBackup.UI.SearchBars {
    public abstract class SearchBarOptionsBase {
        public string SearchValue { get; set; }
        internal float FilterStringWidth { get; set; }
        internal float SortStringWidth { get; set; }

        public virtual bool ShowSortFilterOptions { get; set; }
        public virtual FilterOptionsBase Filter { get; set; }
        public virtual SortOptionsBase SortOptions { get; set; }

        internal SearchBarOptionsBase() {
            SearchValue = string.Empty;
        }
    }

    public abstract class FilterOptionsBase {
        public bool ShowAll { get; set; }


        public virtual void SetShowAll(bool value) {
            ShowAll = value;
        }

        public abstract void ShowFloatMenu(Action OnChanged);
    }

    public abstract class SortOptionsBase {
        public List<Enum> SortBy { get; set; }

        public abstract void ShowFloatMenu(Action OnChanged);
    }

}
