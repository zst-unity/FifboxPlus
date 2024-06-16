using UnityEditor;
using UnityEngine;

namespace ZSToolkit.Editor.FoldersBeauty
{
    public static class FolderBeautyColors
    {
        public static readonly Color DARK_THEME_DEFAULT_FOLDER_COLOR = new Color32(194, 194, 194, 255);
        public static readonly Color LIGHT_THEME_DEFAULT_FOLDER_COLOR = new Color32(86, 86, 86, 255);

        public static readonly Color DARK_THEME_BACKGROUND_COLOR = new Color32(51, 51, 51, 255);
        public static readonly Color LIGHT_THEME_BACKGROUND_COLOR = new Color32(190, 190, 190, 255);

        public static readonly Color DARK_THEME_TREE_VIEW_BACKGROUND_COLOR = new Color32(56, 56, 56, 255);
        public static readonly Color LIGHT_THEME_TREE_VIEW_BACKGROUND_COLOR = new Color32(200, 200, 200, 255);

        public static readonly Color DARK_THEME_SELECTION_COLOR = new Color32(44, 93, 135, 255);
        public static readonly Color LIGHT_THEME_SELECTION_COLOR = new Color32(58, 114, 176, 255);

        public static readonly Color DARK_THEME_ACTIVE_COLOR = new Color32(77, 77, 77, 255);
        public static readonly Color LIGHT_THEME_ACTIVE_COLOR = new Color32(174, 174, 174, 255);

        public static Color DefaultFolderColor => EditorGUIUtility.isProSkin ? DARK_THEME_DEFAULT_FOLDER_COLOR : LIGHT_THEME_DEFAULT_FOLDER_COLOR;
        public static Color BackgroundColor => EditorGUIUtility.isProSkin ? DARK_THEME_BACKGROUND_COLOR : LIGHT_THEME_BACKGROUND_COLOR;
        public static Color TreeViewBackgroundColor => EditorGUIUtility.isProSkin ? DARK_THEME_TREE_VIEW_BACKGROUND_COLOR : LIGHT_THEME_TREE_VIEW_BACKGROUND_COLOR;
        public static Color SelectionColor => EditorGUIUtility.isProSkin ? DARK_THEME_SELECTION_COLOR : LIGHT_THEME_SELECTION_COLOR;
        public static Color ActiveColor => EditorGUIUtility.isProSkin ? DARK_THEME_ACTIVE_COLOR : LIGHT_THEME_ACTIVE_COLOR;
    }
}