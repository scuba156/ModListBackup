using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ModListBackup.Settings
{
    class StateNamesHandleType : SettingHandleConvertible
    {
        internal List<string> StateNames { get; set; }

        public StateNamesHandleType()
        {
            FillNames();
        }

        public override void FromString(string settingValue)
        {
            StateNames = settingValue.Split(Globals.SETTINGS_LIST_SEPARATOR).ToList();
            FillNames();
        }

        public override string ToString()
        {
            return StateNames != null ? string.Join(Globals.SETTINGS_LIST_SEPARATOR.ToString(), StateNames.ToArray()) : "";
        }

        internal string GetStateName(int state)
        {
            return StateNames[state - 1];
        }

        internal void FillNames()
        {
            if (StateNames == null)
                StateNames = new List<string>(Globals.STATE_LIMIT);

            for (int i = 0; i < Globals.STATE_LIMIT; i++)
            {
                if (StateNames.Count == i)
                    StateNames.Add("");
            }
        }
    }
}
