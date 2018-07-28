using ModListBackup.Mods;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using RimToolsUI.Dialogs;
using ModListBackup.Core.Mods;
using ModListBackup.Utils;

namespace ModListBackup.UI.Dialogs {

    internal class Dialog_InstallMod : Dialog_CancelResetAccept {
        private const int MaxNameLength = 42;

        private readonly string DefaultNameSuffix = "DefaultModlistNameSuffix".Translate();

        private ModMetaDataEnhanced curMod;

        private string curName;

        private NameType selNameType;

        private NameType SelNameType { get { return selNameType; } set { selNameType = value; OnNameTypeChange(); } }

        public override Vector2 InitialSize { get { return new Vector2(320f, 380f); } }

        public Dialog_InstallMod(ModMetaDataEnhanced mod) {
            this.curMod = mod;
        }

        public override void PreOpen() {
            base.PreOpen();
            this.OnReset();
        }

        public override void DoWindowContents(Rect rect) {
            //Title
            Rect TitleRect = new Rect(0f, 0f, rect.width, 48f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(TitleRect, "InstallModDialog_Title".Translate(curMod.Name));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            float contentWidth = 200f;

            Rect DescRect = new Rect(rect.xMin, TitleRect.yMax + 20f, rect.width, 100f);
            Widgets.Label(DescRect, "InstallModDesc".Translate(curMod.Name));

            //Name
            if (selNameType != NameType.Custom) {
                GUI.color = Color.gray;
            }
            Rect NameRect = new Rect((rect.width - contentWidth - 440f) / 2f, DescRect.yMax + 10f, contentWidth + 240f, 26f);
            curName = Widgets.TextEntryLabeled(NameRect, "Name".Translate(), curName);
            GUI.color = Color.white;

            //Options
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(new Rect((rect.width - contentWidth) / 2f, NameRect.yMax + 10f, contentWidth, rect.height));
            if (listing.RadioButton("Custom".Translate(), SelNameType == NameType.Custom)) {
                SelNameType = NameType.Custom;
            }
            if (listing.RadioButton("Name".Translate(), SelNameType == NameType.Name)) {
                SelNameType = NameType.Name;
            }
            if (listing.RadioButton("NameDate".Translate(), SelNameType == NameType.NameDate)) {
                SelNameType = NameType.NameDate;
            }
            //if (listing.RadioButton("NameVersion".Translate(), SelNameType == NameType.NameVersion))
            //{
            //    SelNameType = NameType.NameVersion;
            //}

            listing.End();
            DrawBottomButtons(rect);
        }

        protected override void OnAccept() {
            if (VerifyInput()) {
                LongEventHandler.QueueLongEvent(() => { ModUtils.InstallMod(curMod, curName); }, "Installing", true, null);
                this.Close();
            }
        }

        protected override void OnReset() {
            this.SelNameType = NameType.Custom;
            string name = curMod.Name;

            if ((name.Length + DefaultNameSuffix.Length + 1) >= MaxNameLength)
                name = name.Remove(MaxNameLength - DefaultNameSuffix.Length - 1);

            this.curName = string.Format("{0} {1}", name, DefaultNameSuffix);
            string dirName = curName.Replace(" ", "");
            int pos = curName.IndexOf(DefaultNameSuffix, StringComparison.Ordinal);

            if (Directory.Exists(Path.Combine(GenFilePaths.CoreModsFolderPath, dirName))) {
                int count = 1;
                string[] dirs = Directory.GetDirectories(GenFilePaths.CoreModsFolderPath);
                for (int i = 1; i < dirs.Count(); i++) {
                    if (Directory.Exists(string.Format("{0}{1}", curName, i)))
                        count = i++;
                    else
                        break;
                }
                this.curName = string.Format("{0} {1}", curName, count.ToString());
            }
        }

        private void OnNameTypeChange() {
            switch (SelNameType) {
                case NameType.Name:
                    this.curName = curMod.Name;
                    break;

                case NameType.NameDate:
                    string date = DateTime.Now.ToString(" d-M-yy");
                    string name = curMod.Name;
                    if (name.Length + date.Length >= MaxNameLength)
                        name = name.Remove(MaxNameLength - date.Length);
                    this.curName = string.Format("{0}{1}", name, date);
                    break;

                case NameType.NameVersion:
                case NameType.Custom:
                default:
                    break;
            }
        }

        private bool VerifyInput() {
            if (this.curName.NullOrEmpty()) {
                SetMessage("EmptyName".Translate());
                return false;
            }
            if (this.curName.Length > MaxNameLength) {
                SetMessage("NameLengthLimit".Translate(MaxNameLength));
                return false;
            }
            return true;
        }

        public enum NameType {
            Custom, Name, NameDate, NameVersion
        }
    }
}