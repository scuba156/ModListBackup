using HugsLib.Core;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace ModListBackup.StorageContainers {

    internal class ModListSettingsStorageContainer : PersistentDataManager {
        protected List<ModListSettingsStorageData> Data = new List<ModListSettingsStorageData>();
        protected override string FileName => "ModListSettings.xml";
        protected override string FolderName => Path.Combine("ModListBackup", "ModLists");

        public void Load() {
            this.LoadData();
        }

        public void Save() {
            this.SaveData();
        }

        protected override void LoadFromXml(XDocument xml) {
            if (xml.Element("CustomSettings") != null) {
                foreach (var setting in xml.Element("CustomSettings").Elements()) {
                    ModListSettingsStorageData settingData = new ModListSettingsStorageData();

                    settingData.ModIdentifier = xml.Element("Id").Value;

                    if (xml.Element("CustomName") != null) {
                        settingData.CustomName = xml.Element("CustomName").Value;
                    }
                    if (xml.Element("InstallName") != null) {
                        settingData.InstallName = xml.Element("InstallName").Value;
                    }
                    if (xml.Element("TextColor") != null) {
                        //this.TextColor = TextColor. int.Parse(xml.Element("TextColor").Value);
                    }

                    Data.Add(settingData);
                }
            }
        }

        protected override void WriteXml(XDocument xml) {
            var settingsModsElem = new XElement("CustomSettings");
            foreach (var setting in Data) {
                if (setting.IsDirty) {
                    var settingElem = new XElement("Setting");
                    settingElem.Add(new XElement("Id", new XText(setting.ModIdentifier)));
                    if (!setting.CustomName.NullOrEmpty()) {
                        settingElem.Add(new XElement("CustomName", new XText(setting.CustomName)));
                    }
                    if (!setting.InstallName.NullOrEmpty()) {
                        settingElem.Add(new XElement("InstallName", new XText(setting.InstallName)));
                    }
                    if (setting.TextColor != Color.white) {
                        settingElem.Add(new XElement("TextColor", new XText(setting.TextColor.ToString())));
                    }
                    xml.Add(settingElem);
                }
            }
        }
    }

    public class ModListSettingsStorageData {
        public Color TextColor = Color.white;
        public string CustomName { get; set; }
        public string InstallName { get; set; }
        public bool IsDirty { get { return (!CustomName.NullOrEmpty() || !InstallName.NullOrEmpty() || TextColor != Color.white); } }
        public string ModIdentifier { get; set; }
    }
}