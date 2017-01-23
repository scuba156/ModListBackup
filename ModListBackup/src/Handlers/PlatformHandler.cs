using System;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace ModListBackup.Handlers
{
    internal static class PlatformHandler
    {
        internal static PlatformID GetCurrentPlatform()
        {
            if (UnityData.platform == RuntimePlatform.OSXPlayer || UnityData.platform == RuntimePlatform.OSXEditor || UnityData.platform == RuntimePlatform.OSXDashboardPlayer)
                return PlatformID.MacOSX;
            else if (UnityData.platform == RuntimePlatform.WindowsPlayer || UnityData.platform == RuntimePlatform.WindowsEditor)
                return PlatformID.Win32NT;
            else
                return PlatformID.Unix;
        }

        private static string GetAppExecutable()
        {
            string filename = "";

            if (GetCurrentPlatform() == PlatformID.Win32NT)
                filename = "RimWorldWin.exe";
            else if (GetCurrentPlatform() == PlatformID.Unix)
                filename = "start_RimWorld.sh";

            return PathHandler.PathCombine(PathHandler.DirectoriesParentPath(UnityData.dataPath), filename);
        }

        internal static void RestartRimWorld()
        {
            string command = "";
            string args = "";
            switch (PlatformHandler.GetCurrentPlatform())
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    command = "cmd.exe";
                    if (Verse.Steam.SteamManager.Initialized)
                        args = "/c start steam://rungameid/294100";
                    else
                        args = "/c \"" + PlatformHandler.GetAppExecutable() + "\"";
                    break;
                case PlatformID.Unix:
                case (PlatformID)128:
                    command = "bash";
                    if (Verse.Steam.SteamManager.Initialized)
                        args = "steam steam://rungameid/294100";
                    else
                        args = "\"" + PlatformHandler.GetAppExecutable() + "\"";
                    break;
                case PlatformID.MacOSX:
                    command = "open";
                    if (Verse.Steam.SteamManager.Initialized)
                        args = "steam://rungameid/294100";
                    else
                        args = "-n -a \"" + PlatformHandler.GetAppExecutable() + "\"";
                    break;
                default:
                    break;
            }
            Main.DebugMessage("Restarting using {0}{1}", command, args);

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = command,
                Arguments = args
            };

            Process process = Process.Start(psi);

            Main.DebugMessage("now shutting down");
            Root.Shutdown();
        }
    }
}
