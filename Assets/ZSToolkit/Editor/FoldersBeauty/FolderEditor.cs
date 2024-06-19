using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.Editor.FoldersBeauty
{
    [CustomEditor(typeof(DefaultAsset))]
    public class FolderEditor : UnityEditor.Editor
    {
        private Color _darkThemeColor;
        private Color _lightThemeColor;

        private string _assetPath;

        private void OnEnable()
        {
            Undo.undoRedoEvent += UndoRedo;
            _assetPath = AssetDatabase.GetAssetPath(target);
        }

        private void UndoRedo(in UndoRedoInfo undo)
        {
            EditorApplication.RepaintProjectWindow();
            AssetDatabase.SaveAssetIfDirty(FoldersData.Singleton);
        }

        public override void OnInspectorGUI()
        {
            var fullPath = Application.dataPath.Replace("Assets", "") + _assetPath;
            if (Directory.Exists(fullPath))
            {
                GUI.enabled = true;
            }
            else
            {
                GUI.enabled = false;
                return;
            }

            var headerRect = GUILayoutUtility.GetRect(Screen.width, 45);
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            EditorGUI.LabelField(headerRect, "Folder Customization", headerStyle);

            var subHeaderRect = GUILayoutUtility.GetRect(Screen.width, 40);
            var subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = GUI.skin.label.normal.textColor - Color.white * 0.1f }
            };
            EditorGUI.LabelField(subHeaderRect, "Color", subHeaderStyle);

            var subHeaderLineColor = GUI.skin.label.normal.textColor - Color.white * 0.3f;
            var subHeaderLineLeftRect = subHeaderRect;
            subHeaderLineLeftRect.y += subHeaderLineLeftRect.height / 2;
            subHeaderLineLeftRect.height = 2;
            subHeaderLineLeftRect.width = subHeaderLineLeftRect.width / 2 - 50;
            subHeaderLineLeftRect.x += 15;
            EditorGUI.DrawRect(subHeaderLineLeftRect, subHeaderLineColor);

            var subHeaderLineRightRect = subHeaderRect;
            subHeaderLineRightRect.y += subHeaderLineRightRect.height / 2;
            subHeaderLineRightRect.height = 2;
            subHeaderLineRightRect.width = subHeaderLineRightRect.width / 2 - 50;
            subHeaderLineRightRect.x += subHeaderRect.width / 2 + 35;
            EditorGUI.DrawRect(subHeaderLineRightRect, subHeaderLineColor);

            var folderDataIdx = FoldersData.Singleton.folders.FindIndex(data => data.path == AssetDatabase.GetAssetPath(target));
            if (folderDataIdx == -1)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);

                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                var needsRepaint = false;
                var changedFieldIdx = -1;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Dark theme", labelStyle, GUILayout.MinWidth(0));
                _darkThemeColor = EditorGUILayout.ColorField(FolderBeautyColors.DARK_THEME_DEFAULT_FOLDER_COLOR);
                if (EditorGUI.EndChangeCheck())
                {
                    needsRepaint = true;
                    changedFieldIdx = 0;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Light theme", labelStyle, GUILayout.MinWidth(0));
                _lightThemeColor = EditorGUILayout.ColorField(FolderBeautyColors.LIGHT_THEME_DEFAULT_FOLDER_COLOR);
                if (EditorGUI.EndChangeCheck())
                {
                    needsRepaint = true;
                    changedFieldIdx = 1;
                }

                switch (changedFieldIdx)
                {
                    case 0:
                        _lightThemeColor = FoldersBeautyUtils.InvertForTheme(_darkThemeColor);
                        break;
                    case 1:
                        _darkThemeColor = FoldersBeautyUtils.InvertForTheme(_lightThemeColor);
                        break;
                }

                if (needsRepaint) EditorApplication.RepaintProjectWindow();

                EditorGUILayout.LabelField(" ↺", GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight + 2));

                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();

                _darkThemeColor = new(_darkThemeColor.r, _darkThemeColor.g, _darkThemeColor.b, 1f);
                _lightThemeColor = new(_lightThemeColor.r, _lightThemeColor.g, _lightThemeColor.b, 1f);

                if (_darkThemeColor != FolderBeautyColors.DARK_THEME_DEFAULT_FOLDER_COLOR || _lightThemeColor != FolderBeautyColors.LIGHT_THEME_DEFAULT_FOLDER_COLOR)
                {
                    FoldersData.Singleton.folders.Add(new()
                    {
                        path = AssetDatabase.GetAssetPath(target),
                        darkThemeColor = _darkThemeColor,
                        lightThemeColor = _lightThemeColor
                    });

                    EditorUtility.SetDirty(FoldersData.Singleton);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);

                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                var needsRepaint = false;
                var changedFieldIdx = -1;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Dark theme", labelStyle, GUILayout.MinWidth(0));
                _darkThemeColor = EditorGUILayout.ColorField(FoldersData.Singleton.folders[folderDataIdx].darkThemeColor);
                if (EditorGUI.EndChangeCheck())
                {
                    needsRepaint = true;
                    changedFieldIdx = 0;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Light theme", labelStyle, GUILayout.MinWidth(0));
                _lightThemeColor = EditorGUILayout.ColorField(FoldersData.Singleton.folders[folderDataIdx].lightThemeColor);
                if (EditorGUI.EndChangeCheck())
                {
                    needsRepaint = true;
                    changedFieldIdx = 1;
                }

                switch (changedFieldIdx)
                {
                    case 0:
                        _lightThemeColor = FoldersBeautyUtils.InvertForTheme(_darkThemeColor);
                        break;
                    case 1:
                        _darkThemeColor = FoldersBeautyUtils.InvertForTheme(_lightThemeColor);
                        break;
                }

                if (needsRepaint) EditorApplication.RepaintProjectWindow();

                if (GUILayout.Button("↺", GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight + 2)))
                {
                    _darkThemeColor = FolderBeautyColors.DARK_THEME_DEFAULT_FOLDER_COLOR;
                    _lightThemeColor = FolderBeautyColors.LIGHT_THEME_DEFAULT_FOLDER_COLOR;
                    EditorApplication.RepaintProjectWindow();
                }

                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();

                _darkThemeColor = new(_darkThemeColor.r, _darkThemeColor.g, _darkThemeColor.b, 1f);
                _lightThemeColor = new(_lightThemeColor.r, _lightThemeColor.g, _lightThemeColor.b, 1f);

                if (_darkThemeColor == FolderBeautyColors.DARK_THEME_DEFAULT_FOLDER_COLOR && _lightThemeColor == FolderBeautyColors.LIGHT_THEME_DEFAULT_FOLDER_COLOR)
                {
                    FoldersData.Singleton.folders.RemoveAt(folderDataIdx);

                    EditorUtility.SetDirty(FoldersData.Singleton);
                }
                else
                {
                    FoldersData.Singleton.folders[folderDataIdx].darkThemeColor = _darkThemeColor;
                    FoldersData.Singleton.folders[folderDataIdx].lightThemeColor = _lightThemeColor;

                    EditorUtility.SetDirty(FoldersData.Singleton);
                }
            }

            Undo.RecordObject(FoldersData.Singleton, "ZSToolkit Folders Beauty Folders Data");
        }
    }

    public class FolderDeleteProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                var from = movedFromAssetPaths[i];
                var to = movedAssets[i];

                var idx = FoldersData.Singleton.folders.FindIndex(folder => folder.path == from);
                if (idx != -1)
                {
                    FoldersData.Singleton.folders[idx].path = to;
                }
            }

            foreach (var path in deletedAssets)
            {
                var toRemove = new Stack<FolderData>();
                foreach (var data in FoldersData.Singleton.folders)
                {
                    if (data.path == path) toRemove.Push(data);
                }

                while (toRemove.Count > 0)
                {
                    FoldersData.Singleton.folders.Remove(toRemove.Pop());
                }

                AssetDatabase.SaveAssetIfDirty(FoldersData.Singleton);
            }
        }
    }
}