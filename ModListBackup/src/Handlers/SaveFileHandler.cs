using System.Collections.Generic;
using Verse;

namespace ModListBackup.Handlers
{
    /// <summary>
    /// Class for loading mod ids from a save file
    /// </summary>
    internal static class SaveFileHandler
    {
        /// <summary>
        /// Holds a list of modIds when a file is read from
        /// </summary>
        private static List<string> importList;

        /// <summary>
        /// Import mods from a save file into the current game
        /// </summary>
        /// <param name="filename"></param>
        internal static void ImportMods(string filename)
        {
            Read(GenFilePaths.FilePathForSavedGame(filename));

            ModsConfigHandler.SetActiveMods(importList);
        }

        internal static int GetModCount(string filename)
        {
            Read(GenFilePaths.FilePathForSavedGame(filename));
            return importList.Count;
        }

        /// <summary>
        /// Expose the modIds
        /// </summary>
        private static void ExposeData()
        {
            Scribe_Collections.Look<string>(ref importList, "modIds", LookMode.Undefined);
        }

        /// <summary>
        /// Read a save file
        /// </summary>
        /// <param name="filepath">The path to the savefile</param>
        private static void Read(string filepath)
        {
            Scribe.loader.InitLoadingMetaHeaderOnly(filepath);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, false);
            importList = ScribeMetaHeaderUtility.loadedModIdsList;
            Scribe.loader.FinalizeLoading();
        }
    }
}
