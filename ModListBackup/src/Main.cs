using HugsLib;
using HugsLib.Utils;
using System.IO;

namespace ModListBackup
{
    public class Main : ModBase
    {
        internal static Main Instance { get; private set; }

        internal new ModLogger Logger
        {
            get { return base.Logger;  }
        }

        public override string ModIdentifier
        {
            get { return "ModListBackup"; }
        }

        public Main()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            InitDirectories();
        }

        internal static ModLogger Log
        {
           get { return Instance.Logger; }
        }

        private static void InitDirectories()
        {
            Directory.CreateDirectory(Globals.DIR_MODLIST_BACKUP);
        }
    }
}
