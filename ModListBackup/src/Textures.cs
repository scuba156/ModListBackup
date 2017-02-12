using ModListBackup.Handlers;
using UnityEngine;
using Verse;

namespace ModListBackup
{
    [StaticConstructorOnStartup]
    internal static class Textures
    {
        public static readonly Texture2D Undo = PathHandler.LoadPNG(PathHandler.PathCombine(PathHandler.DIR_IMAGES, "undo-icon.png"));
    }
}
