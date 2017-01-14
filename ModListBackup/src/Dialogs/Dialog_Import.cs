using ModListBackup.Handlers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup.Dialogs
{
    /// <summary>
    /// Class for the import save dialog
    /// </summary>
    class Dialog_Import : Window
    {
        private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);
        protected string interactButLabel = "Button_Select_Text".Translate();
        protected List<SaveFileInfo> files = new List<SaveFileInfo>();

        private Vector2 scrollPosition = Vector2.zero;
        private float bottomAreaHeight = 0f;

        private float paddingSize = 5f;
        private float labelDescriptionHeight = 60f;
        private float titleHeight = 45f;
        private float listViewHeight = 500f;

        internal string SelectedFile { get; private set; }

        /// <summary>
        /// The windows initial size
        /// </summary>
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(650f, 700f);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Dialog_Import()
        {
            this.closeOnEscapeKey = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        /// <summary>
        /// Gets the Default file text colour
        /// </summary>
        /// <param name="sfi"></param>
        /// <returns></returns>
        private Color FileNameColor(SaveFileInfo sfi)
        {
            return Dialog_Import.DefaultFileTextColor;
        }

        /// <summary>
        /// Reload the list of save files
        /// </summary>
        private void ReloadFiles()
        {
            this.files.Clear();
            foreach (FileInfo saveFile in GenFilePaths.AllSavedGameFiles)
            {
                try
                {
                    this.files.Add(new SaveFileInfo(saveFile));
                }
                catch (Exception ex)
                {
                    Log.Error("Exception loading " + saveFile.Name + ": " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Before the Dialog has opened
        /// </summary>
        public override void PreOpen()
        {
            base.PreOpen();
            SelectedFile = "";
            ReloadFiles();
        }

        /// <summary>
        /// Draws the window contents
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        public override void DoWindowContents(Rect rect)
        {
            // Title
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, rect.width, titleHeight), "Dialog_Import_Title".Translate());
            Text.Font = GameFont.Small;


            Rect inRect = rect;

            // Description
            Rect DescRect = inRect;
            DescRect.y = titleHeight + paddingSize;
            DescRect.height = labelDescriptionHeight;
            Widgets.Label(DescRect, "Dialog_Import_Desc".Translate());

            // File list
            Vector2 vector2_1 = new Vector2(inRect.width - 16f, 36f);
            Text.Font = GameFont.Small;
            Vector2 vector2_2 = new Vector2(100f, vector2_1.y - 2f);
            inRect.height = listViewHeight;
            float height = (float)this.files.Count * (vector2_1.y + 3f);
            Rect viewRect = new Rect(0.0f, 0.0f, inRect.width - 800f, height);
            Rect outRect = new Rect(inRect.AtZero());
            outRect.y = DescRect.yMax + paddingSize;
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
            float y = 0.0f;
            int num = 0;
            using (List<SaveFileInfo>.Enumerator enumerator = this.files.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
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

        /// <summary>
        /// Import selected file
        /// </summary>
        /// <param name="file">The save file that was selected</param>
        private void DoFileInteraction(string file)
        {
            SelectedFile = file;
            LongEventHandler.QueueLongEvent((Action)(() =>
            {
                SaveFileHandler.ImportMods(file);
                LongEventHandler.ExecuteWhenFinished((Action)(() =>
                {
                    this.Close(true);
                }));
            }), "Dialog_Import_Message", true, (Action<Exception>)null);
        }

    }
}
