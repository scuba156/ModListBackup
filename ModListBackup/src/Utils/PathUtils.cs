using Harmony;
using ModListBackup.Settings;
using ModListBackup.StorageContainers;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Verse;

namespace ModListBackup.Utils {

    [StaticConstructorOnStartup]
    internal static class PathUtils {
        internal const string FileExtensionXML = ".xml";

        internal static readonly string DirBackupsDefault = Path.Combine(DirHome, "Backups");
        internal static readonly string DirHome = FolderUnderSaveData("ModListBackup");
        internal static readonly string DirModSettings = Path.Combine(DirHome, "Mod");
        internal static readonly string Filename_Backups = "Backups" + FileExtensionXML;
        internal static readonly string Filename_ModsConfig = "ModsConfig" + FileExtensionXML;
        internal static readonly string Filename_ModsConfigBackup = "ModsConfigBackup" + FileExtensionXML;

        internal static void CopyDirectory(string sourcePath, string DestinationPath) {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, DestinationPath));

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, DestinationPath), true);
        }

        internal static string CreateDirectoryMd5(string dir) {
            return CreateDirectoryMd5(new DirectoryInfo(dir));
        }

        internal static string CreateDirectoryMd5(DirectoryInfo dir) {
            // TODO : fix
            var filePaths = dir.GetFiles("*", SearchOption.AllDirectories).OrderBy(p => p.FullName).ToArray();

            using (var md5 = MD5.Create()) {
                foreach (var filePath in filePaths) {
                    // hash path
                    byte[] pathBytes = Encoding.UTF8.GetBytes(filePath.FullName.Replace(dir.FullName, ""));
                    md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                    // hash contents
                    byte[] contentBytes = File.ReadAllBytes(filePath.FullName);

                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                }

                //Handles empty filePaths case
                md5.TransformFinalBlock(new byte[0], 0, 0);

                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
        }

        internal static string FolderUnderSaveData(string folder) {
            return (string)AccessTools.Method(typeof(GenFilePaths), "FolderUnderSaveData").Invoke(null, new object[] { folder });
        }

        internal static string GenModBackupsFile(string destination = null) {
            if (destination.NullOrEmpty()) {
                return Path.Combine(DirBackupsDefault, Filename_Backups);
            }
            return Path.Combine(destination, Filename_Backups);
        }

        internal static string GenModListFile(int state, bool includeFolderUnderSave = false) {
            if (includeFolderUnderSave)
                return Path.Combine(DirHome, Path.Combine("ModLists", state.ToString() + FileExtensionXML));
            else
                return Path.Combine("ModLists", state.ToString() + FileExtensionXML);
            //if (SteamSyncHandler.SYNC_TO_STEAM)
            //    return Path.Combine(DirHome, state.ToString() + FileExtensionXML + GenFilePaths.SavedGameExtension);
            //else
            //    return Path.Combine(DirHome, state.ToString() + FileExtensionXML);
        }

        [Obsolete]
        internal static string GenModSettingsFile(string modID) {
            return Path.Combine(DirModSettings, Path.Combine(modID, "Settings" + FileExtensionXML));
        }

        internal static string GetBytesReadable(long i) {
            long absolute_i = (i < 0 ? -i : i);
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            } else if (absolute_i >= 0x4000000000000) // Petabyte
              {
                suffix = "PB";
                readable = (i >> 40);
            } else if (absolute_i >= 0x10000000000) // Terabyte
              {
                suffix = "TB";
                readable = (i >> 30);
            } else if (absolute_i >= 0x40000000) // Gigabyte
              {
                suffix = "GB";
                readable = (i >> 20);
            } else if (absolute_i >= 0x100000) // Megabyte
              {
                suffix = "MB";
                readable = (i >> 10);
            } else if (absolute_i >= 0x400) // Kilobyte
              {
                suffix = "KB";
                readable = i;
            } else {
                return i.ToString("0 B"); // Byte
            }
            readable = (readable / 1024);
            return readable.ToString("0.### ") + suffix;
        }

        internal static long GetDirectorySize(string dir) {
            return GetDirectorySize(new DirectoryInfo(dir));
        }

        internal static long GetDirectorySize(DirectoryInfo dir) {
            if (!dir.Exists) {
                return 0;
            }
            long result = 0;
            foreach (var enclosedDir in dir.GetDirectories()) {
                foreach (var file in enclosedDir.GetFiles()) {
                    result += file.Length;
                }
                result += GetDirectorySize(enclosedDir);
            }
            return result;
        }
    }
}