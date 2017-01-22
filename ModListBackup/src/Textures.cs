using System.IO;
using UnityEngine;
using Verse;

namespace ModListBackup
{
    [StaticConstructorOnStartup]
    internal static class Textures
    {
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        public static readonly Texture2D Undo = LoadPNG(Path.Combine(Globals.DIR_IMAGES, "undo-icon.png"));

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }
            return tex;
        }
    }
}
