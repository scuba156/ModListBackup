using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;

namespace ModListBackup.Settings {

    internal class StateNamesHandleType : SettingHandleConvertible {
        internal List<string> StateNames { get; set; }

        /// <summary>
        /// The separator to split settings list with
        /// </summary>
        private const char Separator = '|';

        public StateNamesHandleType() {
            FillNames();
        }

        public override void FromString(string settingValue) {
            StateNames = settingValue.Split(Separator).ToList();
            FillNames();
        }

        public override string ToString() {
            return StateNames != null ? string.Join(Separator.ToString(), StateNames.ToArray()) : "";
        }

        internal string GetStateName(int state) {
            return StateNames[state - 1];
        }

        internal void FillNames() {
            if (StateNames == null)
                StateNames = new List<string>(SettingsHandler.ModListStateLimit);

            for (int i = 0; i < SettingsHandler.ModListStateLimit; i++) {
                if (StateNames.Count == i)
                    StateNames.Add("");
            }
        }
    }
}