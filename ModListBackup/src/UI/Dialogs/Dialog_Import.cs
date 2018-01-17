﻿using ModListBackup.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup.UI.Dialogs {

    /// <summary>
    /// Class for the import save dialog
    /// </summary>
    internal class Dialog_Import : Window {
        private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);
        private readonly List<SaveFileInfo> files = new List<SaveFileInfo>();
        private readonly List<int> filesModCount = new List<int>();
        private string interactButLabel = "Button_Select_Text".Translate();
        private float labelDescriptionHeight = 60f;
        private float listViewHeight = 500f;
        private float paddingSize = 5f;
        private Vector2 scrollPosition = Vector2.zero;
        private float titleHeight = 45f;

        public Dialog_Import() {
            this.closeOnEscapeKey = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize { get { return new Vector2(650f, 700f); } }

        internal string SelectedFile { get; private set; }

        public override void DoWindowContents(Rect inRect) {
            // Title
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, titleHeight), "Dialog_Import_Title".Translate());
            Text.Font = GameFont.Small;

            Rect rect = inRect;

            // Description
            Rect DescRect = rect;
            DescRect.y = titleHeight + paddingSize;
            DescRect.height = labelDescriptionHeight;
            Widgets.Label(DescRect, "Dialog_Import_Desc".Translate());

            // File list
            Vector2 vector2_1 = new Vector2(rect.width - 16f, 36f);
            Text.Font = GameFont.Small;
            Vector2 vector2_2 = new Vector2(100f, vector2_1.y - 2f);
            rect.height = listViewHeight;
            float height = (float)this.files.Count * (vector2_1.y + 3f);
            Rect viewRect = new Rect(0.0f, 0.0f, rect.width - 800f, height);
            Rect outRect = rect.AtZero();
            outRect.y = DescRect.yMax + paddingSize;
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
            float y = 0.0f;
            int num = 0;
            using (List<SaveFileInfo>.Enumerator enumerator = this.files.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    SaveFileInfo current = enumerator.Current;
                    Rect rect1 = new Rect(0.0f, y, vector2_1.x, vector2_1.y);
                    if (num % 2 == 0)
                        Widgets.DrawAltRect(rect1);
                    Rect position = rect1.ContractedBy(1f);
                    GUI.BeginGroup(position);
                    string withoutExtension = Path.GetFileNameWithoutExtension(current.FileInfo.Name);
                    GUI.color = this.FileNameColor(current);
                    Rect rect2 = new Rect(15f, 0.0f, position.width, position.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;
                    Widgets.Label(rect2, withoutExtension);
                    GUI.color = Color.white;
                    Rect rect3 = new Rect(270f, 0.0f, 200f, position.height);
                    Dialog_FileList.DrawDateAndVersion(current, rect3);

                    // Mod count label
                    GUI.color = SaveFileInfo.UnimportantTextColor;
                    Rect countLabel = new Rect(430, 10f, 150f, position.height);
                    Widgets.Label(countLabel, GetModsCountString(current));

                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    float x = vector2_1.x - 2f - vector2_2.x;
                    TooltipHandler.TipRegion(new Rect(x, 0.0f, vector2_2.x, vector2_2.y), "Button_Select_Tooltip".Translate());
                    if (Widgets.ButtonText(new Rect(x, 0.0f, vector2_2.x, vector2_2.y), this.interactButLabel, true, false, true))
                        this.DoFileInteraction(Path.GetFileNameWithoutExtension(current.FileInfo.Name));
                    GUI.EndGroup();
                    y += vector2_1.y + 3f;
                    ++num;
                }
            }
            Widgets.EndScrollView();
        }

        public override void PreOpen() {
            base.PreOpen();
            SelectedFile = "";
            ReloadFiles();
        }

        /// <summary>
        /// Import selected file
        /// </summary>
        /// <param name="file">The save file that was selected</param>
        private void DoFileInteraction(string file) {
            SelectedFile = file;

            SaveFileHandler.ImportMods(file);
            //Page_ModsConfig_Controller.SetStatus("Status_Message_Imported".Translate());
            this.Close(true);
        }

        /// <summary>
        /// Gets the Default file text color
        /// </summary>
        /// <param name="sfi"></param>
        /// <returns></returns>
        private Color FileNameColor(SaveFileInfo sfi) { return Dialog_Import.DefaultFileTextColor; }

        private string GetModsCountString(SaveFileInfo saveFile) {
            int index = files.FindIndex(name => saveFile.FileInfo.Name == name.FileInfo.Name);

            return String.Format("{0} {1}", filesModCount[index], "Dialog_EditNames_ModCount_Label".Translate());
        }

        /// <summary>
        /// Reload the list of save files
        /// </summary>
        private void ReloadFiles() {
            this.files.Clear();
            foreach (FileInfo saveFile in GenFilePaths.AllSavedGameFiles) {
                try {
                    this.files.Add(new SaveFileInfo(saveFile));
                    this.filesModCount.Add(SaveFileHandler.GetModCount(Path.GetFileNameWithoutExtension(saveFile.Name)));
                }
                catch (Exception ex) {
                    Log.Error("Exception loading " + saveFile.Name + ": " + ex.ToString());
                }
            }
        }
    }
}