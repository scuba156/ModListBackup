using ModListBackup.Controllers;
using System;
using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup.Mods {

    internal class ModSettingsController {
        private string filepath;

        private SaveableSettings modSettings = new SaveableSettings();

        public ModSettingsController(string modID) {
            if (modID.NullOrEmpty()) {
                Main.Log.Message("null");
            }
            filepath = PathHandler.GenModSettingsFile(modID);
            Load();
        }

        public String AltName { get { return modSettings.altName; } set { modSettings.altName = value; Save(); } }

        public String InstallName { get { return modSettings.installName; } set { modSettings.installName = value; Save(); } }

        public Color TextColor { get { return modSettings.textColor; } set { modSettings.textColor = value; Save(); } }

        public void Delete() {
            if (File.Exists(filepath)) {
                File.Delete(filepath);
            }
            if (Directory.Exists(Path.GetDirectoryName(filepath))) {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(filepath));
                if (dir.GetFiles().CountAllowNull() == 0 || dir.GetDirectories().CountAllowNull() == 0) {
                    dir.Delete();
                }
            }
        }

        private void Load() {
            if (File.Exists(filepath))
                this.modSettings = DirectXmlLoader.ItemFromXmlFile<SaveableSettings>(filepath, true);
        }

        private void Save() {
            string dir = new FileInfo(filepath).Directory.FullName;
            if (modSettings.HasSettings) {
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
                DirectXmlSaver.SaveDataObject(modSettings, filepath);
            }
            else {
                Delete();
            }
        }
    }

    public class SaveableSettings {
        public string altName = string.Empty;

        public string installName = string.Empty;

        public Color textColor = Color.white;

        public bool HasSettings { get { return (!altName.NullOrEmpty() || !installName.NullOrEmpty() || textColor != Color.white); } }
    }
}