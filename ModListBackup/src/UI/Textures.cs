using UnityEngine;
using Verse;

namespace ModListBackup.UI {

    [StaticConstructorOnStartup]
    internal static class Textures {
        public static readonly Texture2D Undo = ContentFinder<Texture2D>.Get("undo-icon", true);
    }
}