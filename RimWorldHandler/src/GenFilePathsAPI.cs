using System.Reflection;
using Verse;

namespace RimWorldHandler
{
    public static class GenFilePathsAPI
    {
        public static string SavedGameExtension { get { return GenFilePaths.SavedGameExtension; } }

        public static string ModsConfigFilePath { get { return GenFilePaths.ModsConfigFilePath; } }

        public static string FolderUnderSaveData(string folder) { return (string)typeof(GenFilePaths).GetMethod("FolderUnderSaveData", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { folder }); }
    }
}
