using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using System.Linq;
using System;

namespace ZSToolkit.Editor.Organizer
{
    public class OrganizerSettingsWindow : EditorWindow
    {
        private readonly Dictionary<string, int> _markPreviewLines = new();
        private OrganizerSettings _settings;
        private Font _fontRegular;
        private Font _fontBold;

        public static void ShowWindow()
        {
            var window = GetWindow<OrganizerSettingsWindow>();
            window.minSize = new(335, 400);
            window.titleContent = new GUIContent("Organizer Settings");
            window.Show();
        }

        private void OnEnable()
        {
            var root = OrganizerUtils.GetRoot();
            _fontRegular = AssetDatabase.LoadAssetAtPath<Font>($"{root}UbuntuMonoRegular.ttf");
            _fontBold = AssetDatabase.LoadAssetAtPath<Font>($"{root}UbuntuMonoBold.ttf");
            _settings = OrganizerUtils.GetSettings();
            Undo.undoRedoEvent += UndoRedo;

            foreach (var markTypeName in _settings.markTypes.Names)
            {
                _markPreviewLines.Add(markTypeName, Random.Range(1, 199));
            }
        }

        private void UndoRedo(in UndoRedoInfo undo)
        {
            _markPreviewLines.Clear();
            foreach (var markTypeName in _settings.markTypes.Names)
            {
                _markPreviewLines.Add(markTypeName, Random.Range(1, 199));
            }

            Repaint();
            if (_settings.autoScan) OrganizerWindow.Rescan();
        }

        private bool _firstEnterAfterFocus;

        private void OnFocus()
        {
            _firstEnterAfterFocus = true;
        }

        private static void RemoveInputFocus()
        {
            GUIUtility.keyboardControl = 0;
        }

        private string _keyInput = "NEW";

        private void OnGUI()
        {
            if (_firstEnterAfterFocus)
            {
                RemoveInputFocus();
                _firstEnterAfterFocus = false;
            }

            var settingLabelStyle = new GUIStyle(GUI.skin.label)
            {
            };

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                font = _fontBold,
                fontSize = 20,
                alignment = TextAnchor.UpperCenter
            };

            var baseSettingsBaseRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(Screen.width));
            var baseSettingsRect = new Rect(baseSettingsBaseRect.x, baseSettingsBaseRect.y, baseSettingsBaseRect.width, baseSettingsBaseRect.height + 10);
            EditorGUI.LabelField(baseSettingsRect, "Base settings", headerStyle);
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Path to scan", settingLabelStyle, GUILayout.MaxWidth(100));
            _settings.folderPath = EditorGUILayout.TextField(_settings.folderPath);
            if (GUILayout.Button("Select path", GUILayout.MaxWidth(100)))
            {
                var path = EditorUtility.OpenFolderPanel("Select path", "Assets", "");
                if (path != "")
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _settings.folderPath = "Assets" + path.Replace(Application.dataPath, "");
                        if (_settings.autoScan) OrganizerWindow.Rescan();
                    }
                    else
                    {
                        Debug.LogError("specified path must be subdirectory of Assets");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Automatic scan", settingLabelStyle, GUILayout.MaxWidth(100));

            GUILayout.FlexibleSpace();

            var autoScanRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(40));
            autoScanRect.x -= 2;
            GUI.Box(autoScanRect, "", EditorStyles.textField);

            if (_settings.autoScan)
            {
                var dotRect = new Rect(autoScanRect.x + autoScanRect.width - (autoScanRect.height - 4), autoScanRect.y + 4, autoScanRect.height - 8, autoScanRect.height - 8);
                EditorGUI.DrawRect(dotRect, new(1, 1, 1, 0.85f));
            }
            else
            {
                var dotRect = new Rect(autoScanRect.x + 4, autoScanRect.y + 4, autoScanRect.height - 8, autoScanRect.height - 8);
                EditorGUI.DrawRect(dotRect, new(1, 1, 1, 0.5f));
            }

            if (GUI.Button(autoScanRect, "", GUIStyle.none))
            {
                _settings.autoScan = !_settings.autoScan;
                if (_settings.autoScan) OrganizerWindow.Rescan();
            }
            EditorGUIUtility.AddCursorRect(autoScanRect, MouseCursor.Link);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            var markTypesBaseRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(Screen.width));
            var markTypesRect = new Rect(markTypesBaseRect.x, markTypesBaseRect.y, markTypesBaseRect.width, markTypesBaseRect.height + 10);
            EditorGUI.LabelField(markTypesRect, "Mark types", headerStyle);

            EditorGUILayout.Space(6);

            var toRemove = new Queue<string>();
            foreach (var type in _settings.markTypes)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width));

                var lastName = type.Name;
                EditorGUI.BeginChangeCheck();
                type.Name = EditorGUILayout.TextField(type.Name, GUILayout.MaxWidth(100));
                if (EditorGUI.EndChangeCheck())
                {
                    if (string.IsNullOrEmpty(type.Name))
                    {
                        type.Name = lastName;
                    }

                    if (_settings.markTypes.Names.Count(name => name == type.Name) > 1)
                    {
                        Debug.LogWarning($"Mark \"{type.Name}\" already exists, reverting to \"{lastName}\"");
                        type.Name = lastName;
                    }

                    if (_markPreviewLines.ContainsKey(lastName))
                    {
                        var tempLine = _markPreviewLines[lastName];
                        _markPreviewLines.Remove(lastName);
                        _markPreviewLines.Add(type.Name, tempLine);
                    }

                    if (_settings.autoScan) OrganizerWindow.Rescan();
                }

                type.Color = EditorGUILayout.ColorField(type.Color);

                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    toRemove.Enqueue(type.Name);
                }
                EditorGUILayout.EndHorizontal();

                var preferredRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight);
                var previewLabelRect = new Rect(preferredRect.x + 3f, preferredRect.y - 3, 275, preferredRect.height + 4);

                var previewLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    font = _fontRegular,
                    fontSize = 14
                };
                var label = string.Format(" {0, -24} {1}", $"<color=grey>{_markPreviewLines[type.Name]}</color>", $"<color=#{ColorUtility.ToHtmlStringRGB(type.Color)}>{type.Name}:</color> do thing that does thing");
                EditorGUI.LabelField(previewLabelRect, label, previewLabelStyle);

                GUILayout.Space(8);
            }

            foreach (var name in toRemove)
            {
                _settings.markTypes.Remove(name);
                if (_markPreviewLines.ContainsKey(name)) _markPreviewLines.Remove(name);

                if (_settings.autoScan) OrganizerWindow.Rescan();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width));

            _keyInput = EditorGUILayout.TextField(_keyInput, GUILayout.MaxWidth(100));
            if (string.IsNullOrEmpty(_keyInput))
            {
                EditorGUILayout.LabelField($"Mark name cannot be empty");
            }
            else if (Array.Exists(_settings.markTypes.Names, name => name == _keyInput))
            {
                EditorGUILayout.LabelField($"Mark \"{_keyInput}\" already exists");
            }
            else if (GUILayout.Button("Add"))
            {
                _settings.markTypes.Add(_keyInput, Color.white);
                _markPreviewLines.Add(_keyInput, Random.Range(1, 199));

                if (_settings.autoScan) OrganizerWindow.Rescan();
            }

            EditorGUILayout.EndHorizontal();
            Undo.RecordObject(_settings, "Organizer Settings");

            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "", GUIStyle.none))
            {
                GUI.FocusControl(null);
            }
        }
    }
}