using UnityEngine;
using Verse;

namespace ModListBackup.UI {

    [StaticConstructorOnStartup]
    internal static class Textures {
        public static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash", true);
        public static readonly Texture2D Undo = ContentFinder<Texture2D>.Get("Undo-Icon", true);
        public static readonly Texture2D ContextMenu = ContentFinder<Texture2D>.Get("ContextMenu-Icon", true);
        public static readonly Texture2D QuestionMark = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark", true);
    }
}