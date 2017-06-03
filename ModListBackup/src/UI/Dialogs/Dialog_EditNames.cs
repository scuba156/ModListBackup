using HugsLib;
using ModListBackup.Handlers.Settings;
using UnityEngine;
using Verse;

namespace ModListBackup.Dialogs {

    internal class Dialog_EditNames : Window {
        private bool settingsHaveChanged;
        private Vector2 scrollPosition;

        private const float ScrollBarWidthMargin = 18f;

        public override void DoWindowContents(Rect inRect) {
            // Title
            GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 40f);
            Widgets.Label(titleRect, "Dialog_EditNames_Title".Translate());
            Text.Font = GameFont.Tiny;
            GenUI.ResetLabelAlign();

            //Name list
            var scrollViewVisible = new Rect(0f, titleRect.height + 10, inRect.width, inRect.height - titleRect.height - 60f);
            var scrollBarVisible = SettingsHandler.STATE_LIMIT * 42f > scrollViewVisible.height;
            var scrollViewTotal = new Rect(0f, 0f - 10f, scrollViewVisible.width - (scrollBarVisible ? ScrollBarWidthMargin : 0) - 20f, SettingsHandler.STATE_LIMIT * 42f);
            Widgets.BeginScrollView(scrollViewVisible, ref scrollPosition, scrollViewTotal);

            if (SettingsHandler.DoStateNamesDrawerContents(scrollViewTotal))
                settingsHaveChanged = true;
            Widgets.EndScrollView();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Dialog_EditNames() {
            this.closeOnEscapeKey = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        /// <summary>
        /// The windows initial size
        /// </summary>
        public override Vector2 InitialSize {
            get {
                return new Vector2(280f, 650f);
            }
        }

        public override void PreOpen() {
            base.PreOpen();
            settingsHaveChanged = false;
            SettingsHandler.RefreshStateNameSettings();
        }

        public override void PostClose() {
            base.PostClose();
            if (settingsHaveChanged) Main.Log.Message("did");
            if (settingsHaveChanged) HugsLibController.Instance.Settings.SaveChanges();
        }
    }
}