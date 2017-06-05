using System.Collections.Generic;
using Verse;

namespace ModListBackup.Controllers {

    /// <summary>
    /// Class for loading mod ids from a save file
    /// </summary>
    internal static class SaveFileHandler {
        private static List<string> importList;

        internal static int GetModCount(string filename) {
            Read(GenFilePaths.FilePathForSavedGame(filename));
            return importList.Count;
        }

        /// <summary>
        /// Import mods from a save file into the current game
        /// </summary>
        /// <param name="filename"></param>
        internal static void ImportMods(string filename) {
            Read(GenFilePaths.FilePathForSavedGame(filename));

            ModsConfigHandler.SetActiveMods(importList);
        }

        private static void ExposeData() {
            Scribe_Collections.Look<string>(ref importList, "modIds", LookMode.Undefined);
        }

        private static void Read(string filepath) {
            Scribe.loader.InitLoadingMetaHeaderOnly(filepath);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, false);
            importList = ScribeMetaHeaderUtility.loadedModIdsList;
            Scribe.loader.FinalizeLoading();
        }
    }
}