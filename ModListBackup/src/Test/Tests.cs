using ModListBackup.Threads;
using ModListBackup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ModListBackup.Test {
    internal static class Tests {
        internal static void HashAllModsPerformance() {
            Log.Message("Testing mod hashing performance");
            DateTime startTime = DateTime.Now;
            int count = 0;

            foreach (var item in ModLister.AllInstalledMods) { //Core.ModListController.Instance.CurrentModList) {
                PathUtils.CreateDirectoryMd5(item.RootDir);
                count++;
            }

            TimeSpan finishTime = DateTime.Now.Subtract(startTime);
            Log.Message("Finished Test");

            Main.Log.Message("Hashed {0} mods in {1}", count, finishTime.Duration());
        }

        internal static void TotalSizeThreadTest() {
            Log.Message("Testing total size thread");
            //new TotalModsSizeTask(ModLister.)
        }
    }
}
