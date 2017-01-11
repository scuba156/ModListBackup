using System.Collections.Generic;
using Verse;

namespace ModListBackup.Handlers
{
    class ModsConfigHandler
    {
        internal enum Mode
        {
            Saving,
            Loading
        }

        internal static Mode CurrentMode { get; private set; }

        private static IEnumerable<ModMetaData> CurrentMods { get; set; }

        private static ModsConfigData data;

        internal static void SaveState(int state)
        {
            CurrentMode = Mode.Saving;
            ExposeData(GenBackupStateFile(state));
        }

        internal static void LoadState(int state)
        {
            CurrentMode = Mode.Loading;
            ExposeData(GenBackupStateFile(state));
        }

        private static string GenBackupStateFile(int state)
        {
            return Globals.DIR_MODLIST_BACKUP + state.ToString() + ".xml";
        }

        private static void SaveData(string filepath)
        {
            XmlSaver.SaveDataObject((object)data, filepath);
        }

        internal static void ExposeData(string filepath)
        {
            if (CurrentMode == Mode.Saving)
            {
                data = new ModsConfigData();
                data.buildNumber = RimWorld.VersionControl.CurrentBuild;
                data.activeMods = new List<string>();
                foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
                {
                    data.activeMods.Add(mod.Identifier);
                }

                SaveData(filepath);
            }
            else if (CurrentMode == Mode.Loading)
            {
                data = XmlLoader.ItemFromXmlFile<ModsConfigData>(filepath, true);

                ModsConfig.Reset();

                bool foundCore = false;

                foreach (string modID in data.activeMods)
                {
                    if (modID != ModContentPack.CoreModIdentifier)
                        ModsConfig.SetActive(modID, true);
                    else
                        foundCore = true;
                }

                if (!foundCore)
                {
                    ModsConfig.SetActive(ModContentPack.CoreModIdentifier, false);
                }
            }
            else
            {
                Main.Log.Error("Tried to expose data but an unknown mode was set");
            }
        }

        private class ModsConfigData
        {
            public int buildNumber = -1;
            public List<string> activeMods = new List<string>();
        }
    }
}
