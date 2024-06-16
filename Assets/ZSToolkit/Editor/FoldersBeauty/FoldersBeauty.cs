using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.Editor.FoldersBeauty
{
    [InitializeOnLoad]
    public static class FoldersBeauty
    {
        private static Texture2D _folderTexture;
        private static Texture2D _folderEmptyTexture;
        private static Texture2D _folderOpenedTexture;
        private static Texture2D _foldedIcon;
        private static Texture2D _unfoldedIcon;

        private static bool _isTwoColumns;

        private static string[] _treeViewSelection = Array.Empty<string>();
        private static string[] _displayTreeViewSelection = Array.Empty<string>();
        private static string[] _assetsViewSelection = Array.Empty<string>();

        private static readonly List<string> _activeFolderPaths = new();
        private static readonly List<string> _lastActiveFolderPaths = new();

        private static readonly List<string> _expandedFolders = new();
        private static readonly List<string> _lastExpandedFolders = new();

        private static readonly List<string> _currentAssetViewPaths = new();
        private static readonly List<string> _currentTreeViewPaths = new();

        private static bool _projectWindowRepainting;

        static FoldersBeauty()
        {
            EditorApplication.projectWindowItemOnGUI -= OnAssetRendered;
            EditorApplication.projectWindowItemOnGUI += OnAssetRendered;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_projectWindowRepainting)
            {
                AfterProjectWindowRepaint();
                _projectWindowRepainting = false;
            }
        }

        private static void OnActiveFoldersChanged()
        {
            //Debug.Log($"Active folders changed | count: {_activeFolderPaths.Count}");
            EditorApplication.RepaintProjectWindow();
        }

        private static void OnProjectWindowRepaint()
        {
            var mainAssetPath = FoldersBeautyUtils.GetMainAssetPath();
            if (!_folderTexture) _folderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainAssetPath}folder.png");
            if (!_folderEmptyTexture) _folderEmptyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainAssetPath}folderEmpty.png");
            if (!_folderOpenedTexture) _folderOpenedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainAssetPath}folderOpen.png");
            if (!_foldedIcon) _foldedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainAssetPath}folded.png");
            if (!_unfoldedIcon) _unfoldedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainAssetPath}unfolded.png");

            _isTwoColumns = (bool)typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { "" });

            GetSelection();
        }

        private static void AfterProjectWindowRepaint()
        {
            //Debug.Log($"Finished repaint | Asset elements: {_currentAssetViewPaths.Count} | Tree elements: {_currentTreeViewPaths.Count}");

            _activeFolderPaths.Clear();
            var activeFolderPath = (string)typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            _activeFolderPaths.Add(activeFolderPath);
            foreach (var path in _currentAssetViewPaths)
            {
                var assetDirectory = Path.GetDirectoryName(path).Replace("\\", "/");
                if (!_activeFolderPaths.Contains(assetDirectory)) _activeFolderPaths.Add(assetDirectory);
            }

            if (_lastActiveFolderPaths.Except(_activeFolderPaths).Count() > 0)
            {
                OnActiveFoldersChanged();
            }
            _lastActiveFolderPaths.Clear();
            _lastActiveFolderPaths.AddRange(_activeFolderPaths);

            _expandedFolders.Clear();
            foreach (var path in _currentTreeViewPaths)
            {
                if (path == "Assets") continue;
                var slashIdx = path.LastIndexOf("/");
                var parentFolder = path[..slashIdx];
                if (!_expandedFolders.Contains(parentFolder)) _expandedFolders.Add(parentFolder);
            }

            if (_lastExpandedFolders.Except(_expandedFolders).Count() > 0)
            {
                //EditorApplication.RepaintProjectWindow();
            }
            _lastExpandedFolders.Clear();
            _lastExpandedFolders.AddRange(_expandedFolders);

            _currentAssetViewPaths.Clear();
            _currentTreeViewPaths.Clear();
            _displayTreeViewSelection = Array.Empty<string>();
            _assetsViewSelection = Array.Empty<string>();
        }

        private static void GetSelection()
        {
            _treeViewSelection = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets).Select(folder => AssetDatabase.GetAssetPath(folder)).ToArray();

            if (!EditorWindow.focusedWindow || EditorWindow.focusedWindow.titleContent.text != "Project")
            {
                _displayTreeViewSelection = Array.Empty<string>();
                _assetsViewSelection = Array.Empty<string>();
            }
            else
            {
                _displayTreeViewSelection = _treeViewSelection;
                _assetsViewSelection = Selection.objects.OfType<DefaultAsset>().Select(obj => AssetDatabase.GetAssetPath(obj)).ToArray();
            }
        }

        private static void OnAssetRendered(string guid, Rect selectionRect)
        {
            if (!_projectWindowRepainting)
            {
                OnProjectWindowRepaint();
                _projectWindowRepainting = true;
            }

            var pathToAsset = AssetDatabase.GUIDToAssetPath(guid);
            var isTreeView = selectionRect.width > selectionRect.height;

            if (!string.IsNullOrEmpty(pathToAsset))
            {
                if (!isTreeView && !_currentAssetViewPaths.Contains(pathToAsset)) _currentAssetViewPaths.Add(pathToAsset);
                else if (isTreeView && !_currentTreeViewPaths.Contains(pathToAsset)) _currentTreeViewPaths.Add(pathToAsset);
            }

            var folderAsset = AssetDatabase.LoadAssetAtPath(pathToAsset, typeof(DefaultAsset));
            if (!folderAsset) return;

            var attr = File.GetAttributes(Application.dataPath.Replace("Assets", "") + pathToAsset);
            if (!attr.HasFlag(FileAttributes.Directory)) return;

            var folderData = FoldersData.Singleton.folders.Find(data => data.path == pathToAsset);
            if (folderData == null) DrawFolder(FolderBeautyColors.DefaultFolderColor, pathToAsset, selectionRect);
            else DrawFolder(EditorGUIUtility.isProSkin ? folderData.darkThemeColor : folderData.lightThemeColor, pathToAsset, selectionRect);
        }

        private static void DrawFolder(Color color, string folderPath, Rect rect)
        {
            var isTreeView = rect.width > rect.height;
            var fullPath = Application.dataPath.Replace("Assets", "") + folderPath;

            var selectedInAssetsView = _assetsViewSelection.Contains(folderPath);
            var displayedInAssetView = _currentAssetViewPaths.Contains(folderPath);

            var selectedInTreeView = _displayTreeViewSelection.Contains(folderPath);
            if (_isTwoColumns && displayedInAssetView && selectedInAssetsView) selectedInTreeView = false;

            if (!EditorGUIUtility.isProSkin && selectedInTreeView) color = Color.white;

            if (isTreeView)
            {
                var bgColor = FolderBeautyColors.TreeViewBackgroundColor;
                if (selectedInTreeView)
                {
                    bgColor = FolderBeautyColors.SelectionColor;
                }
                else if (_activeFolderPaths.Contains(folderPath) && _isTwoColumns)
                    bgColor = FolderBeautyColors.ActiveColor;

                var bgRect = new Rect(0, rect.y, rect.width + rect.x, rect.height);
                EditorGUI.DrawRect(bgRect, new(bgColor.r, bgColor.g, bgColor.b, 1f));

                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = selectedInTreeView ? Color.white : GUI.skin.label.normal.textColor }
                };

                var labelRect = new Rect(rect.x + 17, rect.y - 1, rect.width - 17, rect.height);
                EditorGUI.LabelField(labelRect, Path.GetFileName(folderPath), labelStyle);

                if (Directory.GetDirectories(fullPath).Where(path => !path.EndsWith("~")).Any())
                {
                    var foldColor = FolderBeautyColors.DefaultFolderColor - Color.black * 0.65f;
                    var foldRect = new Rect(rect.x - 14, rect.y + 3, 13, rect.height - 5);
                    var foldIcon = _expandedFolders.Contains(folderPath) ? _unfoldedIcon : _foldedIcon;
                    GUI.DrawTexture(foldRect, foldIcon, ScaleMode.ScaleToFit, true, 0f, foldColor, 0f, 0f);
                }
            }

            var texture = FoldersBeautyUtils.IsDirectoryEmpty(fullPath) ? _folderEmptyTexture : _folderTexture;
            if (!isTreeView)
            {
                var folderRect = new Rect(rect.x + 7f, rect.y + 7f, rect.width - 15f, rect.height - 30f);
                var bgRect = new Rect(rect.x, rect.y, rect.width, rect.height - 15);
                EditorGUI.DrawRect(bgRect, FolderBeautyColors.BackgroundColor);
                GUI.DrawTexture(folderRect, texture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
            }
            else
            {
                if (_expandedFolders.Contains(folderPath))
                {
                    var folderRect = new Rect(rect.x + 2, rect.y + 2, 14, rect.height - 3);
                    GUI.DrawTexture(folderRect, _folderOpenedTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
                }
                else
                {
                    var folderRect = new Rect(rect.x + 2, rect.y + 2, 12, rect.height - 4);
                    GUI.DrawTexture(folderRect, texture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
                }
            }
        }
    }
}