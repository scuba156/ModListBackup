using UnityEngine;
using Verse;

namespace ModListBackup.UI.Dialogs {

    public abstract class ConfirmCancelResetDialog : Window {
        private readonly Vector2 BottomButSize = new Vector2(60f, 30f);

        private string StatusMessage = string.Empty;

        private int StatusTimer = -1;

        public abstract void OnAccept();

        public abstract void OnReset();

        public virtual void OnCancel() {
            this.Close();
        }

        public abstract override void DoWindowContents(Rect inRect);

        public virtual void DrawBottomButtons(Rect inRect) {
            Rect botRect = new Rect(inRect.x, inRect.yMax - BottomButSize.y, inRect.width, BottomButSize.y);

            //Status Message
            Rect MessageRect = new Rect(botRect.x, botRect.yMax - BottomButSize.y - 18f, inRect.width, 18f);
            if (StatusTimer > 0) {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperRight;
                GUI.color = Color.red;
                Widgets.Label(MessageRect, StatusMessage);
                StatusTimer--;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }

            //Cancel Button
            Rect CancelButton = new Rect(botRect.xMin, botRect.y, BottomButSize.x, BottomButSize.y);
            if (Widgets.ButtonText(CancelButton, "CancelButton".Translate()))
                OnCancel();

            //Reset Button
            Rect ResetButton = new Rect(CancelButton.xMax + 2f, botRect.y, BottomButSize.x, BottomButSize.y);
            if (Widgets.ButtonText(ResetButton, "Reset".Translate()))
                OnReset();

            //Accept Button
            Rect AcceptRect = new Rect(botRect.xMax - BottomButSize.x, botRect.y, BottomButSize.x, BottomButSize.y);
            if (Widgets.ButtonText(AcceptRect, "AcceptButton".Translate()))
                OnAccept();
        }

        public ConfirmCancelResetDialog() {
            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnEscapeKey = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
        }

        internal void SetMessage(string message) {
            StatusMessage = message;
            StatusTimer = 1000;
        }
    }
}