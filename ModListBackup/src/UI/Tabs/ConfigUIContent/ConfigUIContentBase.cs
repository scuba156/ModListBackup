using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ModListBackup.UI.Tabs.ConfigUIContent {
    internal abstract class ConfigUIContentBase {

        internal virtual string Description { get; }


        internal abstract void DrawContent(Listing_Standard listing);
    }
}
