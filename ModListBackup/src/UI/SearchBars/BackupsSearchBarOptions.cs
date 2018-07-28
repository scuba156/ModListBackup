using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModListBackup.UI.SearchBars {
    public class BackupsSearchBarOptions : SearchBarOptionsBase {
        public override bool ShowSortFilterOptions => true;
        public BackupsSearchBarOptions() {
            Filter = new BackupFilterOptions();
            SortOptions = new BackupSortOptions();
        }
    }

    public class BackupFilterOptions : FilterOptionsBase {
        public override void ShowFloatMenu(Action OnChanged) {
        }
    }

    public class BackupSortOptions : SortOptionsBase {
        public override void ShowFloatMenu(Action OnChanged) {
        }
    }
}
