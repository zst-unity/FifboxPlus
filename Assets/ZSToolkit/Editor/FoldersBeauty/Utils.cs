using System.IO;
using System.Linq;
using UnityEngine;

namespace ZSToolkit.Editor.FoldersBeauty
{
    public static class FoldersBeautyUtils
    {
        public static string GetMainAssetPath([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
        {
            var fullPath = Path.GetFullPath(Path.Combine(fileName, "../"));
            var pathSplitted = fullPath.Split("Assets");
            var path = "Assets" + pathSplitted[1];

            return path;
        }

        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static Color InvertForTheme(Color inputColor)
        {
            var invertedColor = new Color(1f - inputColor.r, 1f - inputColor.g, 1f - inputColor.b);

            Color.RGBToHSV(invertedColor, out var h, out var s, out var v);
            var rotatedHue = h > 0.5f ? h - 0.5f : h + 0.5f;

            var outputColor = Color.HSVToRGB(rotatedHue, s, v);
            return outputColor;
        }
    }
}