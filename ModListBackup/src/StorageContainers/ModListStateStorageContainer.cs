using HugsLib.Core;
using ModListBackup.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace ModListBackup.StorageContainers {

    public class ModListStateStorageContainer : PersistentDataManager {

        public ModListStateStorageContainer(string filename) {
            this._FileName = filename;
        }

        internal ModListStateStorageContainer(int saveStateId) {
            this.SaveStateId = saveStateId;
            this.Data = new ModListStateStorageData();
        }

        public int SaveStateId { get; private set; }
        internal ModListStateStorageData Data { get; set; }
        protected override string FileName { get { if (_FileName.NullOrEmpty()) return SaveStateId.ToString() + PathUtils.FileExtensionXML; return _FileName; } }
        protected override string FolderName => Path.Combine("ModListBackup", "ModLists");
        private string _FileName { get; set; }
        public void Load() {
            this.LoadData();
        }

        public void Save() {
            this.SaveData();
        }
        protected override void LoadFromXml(XDocument xml) {
            Data = new ModListStateStorageData();
            if (xml.Root == null) throw new NullReferenceException("Missing root node");

            XElement stateXml = xml.Element("ModListState");
            Data.BuildNumber = int.Parse(stateXml.Element("BuildNumber").Value);

            foreach (var activeMod in stateXml.Element("ActiveMods").Elements("id"))
                Data.ActiveMods.Add(activeMod.Value);
        }

        protected override void WriteXml(XDocument xml) {
            var root = new XElement("ModListState");
            xml.Add(root);
            root.Add(new XElement("BuildNumber", new XText(Data.BuildNumber.ToString())));

            var activeModsElem = new XElement("ActiveMods");
            foreach (var activeMod in Data.ActiveMods) {
                activeModsElem.Add(new XElement("id", new XText(activeMod)));
            }
            root.Add(activeModsElem);
        }
    }

    public class ModListStateStorageData {

        public ModListStateStorageData() {
            ActiveMods = new List<string>();
        }

        public List<string> ActiveMods { get; set; }
        public int BuildNumber { get; set; }
    }
}