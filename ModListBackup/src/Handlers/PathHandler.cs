using RimWorldHandler;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ModListBackup.Handlers
{
    [StaticConstructorOnStartup]
    internal static class PathHandler
    {
        /// <summary>
        /// The extension prefix for an xml file
        /// </summary>
        internal const string XML_FILE_PREFIX = ".xml";

        /// <summary>
        /// Holds the path to store mod list backup files
        /// </summary>
        internal static string DIR_BACKUPS = GenFilePathsAPI.FolderUnderSaveData("ModListBackup");

        /// <summary>
        /// Holds the path to where our images are stored
        /// </summary>
        internal static string DIR_IMAGES = Path.Combine(Main.GetModContentPack.RootDir, "img");

        /// <summary>
        /// Holds the filename of RimWorld's ModsConfig file
        /// </summary>
        internal static string FILE_MODSCONFIG_NAME = "ModsConfig" + XML_FILE_PREFIX;

        /// <summary>
        /// Generates a filename for a state
        /// </summary>
        /// <param name="state">The state to generate a filename for</param>
        /// <returns>The filename for the state</returns>
        internal static string GenBackupStateFile(int state)
        {
            if (SteamSyncHandler.SYNC_TO_STEAM)
                return Path.Combine(DIR_BACKUPS, state.ToString() + XML_FILE_PREFIX + GenFilePathsAPI.SavedGameExtension);
            else
                return Path.Combine(DIR_BACKUPS, state.ToString() + XML_FILE_PREFIX);
        }

        internal static bool FileExists(string filepath)
        {
            return File.Exists(filepath);
        }

        internal static void FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            Main.DebugMessage("Attempting to {0} {1} to {2}", (overwrite) ? "overwrite" : "copy", sourceFileName, destFileName);
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        internal static string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        internal static string DirectoriesParentPath(string path)
        {
            return new DirectoryInfo(path).Parent.FullName;
        }

        internal static void FileDelete(string path)
        {
            Main.DebugMessage("Attempting to delete {0}", path);
            File.Delete(path);
        }

        internal static string[] GetFiles(string path, string searchPattern = "")
        {
            return Directory.GetFiles(path, searchPattern);
        }

        internal static string GetFileName(string filepath, bool withoutExtension = false)
        {
            if (withoutExtension)
                return new FileInfo(filepath).Name.Replace(new FileInfo(filepath).Extension, "");
            return new FileInfo(filepath).Name;
        }

        internal static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }
            return tex;
        }
    }
}
