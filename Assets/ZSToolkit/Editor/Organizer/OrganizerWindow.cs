using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Threading;
using System.Linq;

namespace ZSToolkit.Editor.Organizer
{
    public class OrganizerWindow : EditorWindow
    {
        private static OrganizerSettings _settings;
        private static Font _fontRegular;
        private static Font _fontBold;
        private static Texture2D _foldedIcon;
        private static Texture2D _unfoldedIcon;
        private readonly static Dictionary<string, Mark[]> _marks = new();
        private readonly static List<string> _foldedMarkTypes = new();

        private static float _scrollOffset = 0;

        public class Mark
        {
            public string type;
            public string comment;
            public string filepath;
            public int line;

            public Mark(string type, string comment, string filepath, int line)
            {
                this.type = type;
                this.comment = comment;
                this.filepath = filepath;
                this.line = line;
            }
        }

        [MenuItem("ZSToolkit/Organizer")]
        private static void ShowWindow()
        {
            var window = GetWindow<OrganizerWindow>();

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{OrganizerUtils.GetRoot()}icon.png");
            window.titleContent = new GUIContent("Organizer", icon);
            window.Show();
        }

        private void OnEnable()
        {
            var root = OrganizerUtils.GetRoot();
            _fontRegular = AssetDatabase.LoadAssetAtPath<Font>($"{root}UbuntuMonoRegular.ttf");
            _fontBold = AssetDatabase.LoadAssetAtPath<Font>($"{root}UbuntuMonoBold.ttf");
            _foldedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{root}Folded.png");
            _unfoldedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{root}Unfolded.png");
        }

        private int _updateTimer;

        private void Update()
        {
            _updateTimer++;
            if (_updateTimer >= 10)
            {
                if (commentsExtractionThreadData.Count > 0)
                {
                    for (int i = 0; i < commentsExtractionThreadData.Count; i++)
                    {
                        var threadData = commentsExtractionThreadData.Dequeue();
                        threadData.callback(threadData.data);
                    }
                }

                _updateTimer = 0;
                Repaint();
            }
        }

        private string _searchString = "";
        private Color _tempColor;

        private readonly Dictionary<string, float> _foldButtonHeights = new();
        private readonly Dictionary<string, float> _headersYPositions = new();

        private void OnGUI()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                _scrollOffset -= Event.current.delta.y * 5;
                Repaint();
            }

            var marksHeight = -65f;
            foreach (var markType in _marks)
            {
                if (!_headersYPositions.ContainsKey(markType.Key))
                {
                    _headersYPositions.Add(markType.Key, marksHeight + 65);
                }
                else
                {
                    _headersYPositions[markType.Key] = marksHeight + 65;
                }

                marksHeight -= EditorGUIUtility.singleLineHeight + 2f;
                if (_foldedMarkTypes.Contains(markType.Key)) continue;
                marksHeight -= markType.Value.Length * (EditorGUIUtility.singleLineHeight + 2f);
            }

            var minScroll = Mathf.Min(marksHeight + Screen.height, 0);
            _scrollOffset = Mathf.Clamp(_scrollOffset, minScroll, 0);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Scan")) Rescan();
            if (GUILayout.Button("Settings")) OrganizerSettingsWindow.ShowWindow();

            var searchStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSearchTextField"))
            {
                fixedWidth = 200
            };

            _searchString = GUILayout.TextField(_searchString, searchStyle);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Text);

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
            {
                _searchString = "";
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            var totalLabelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.gray },
                hover = { textColor = Color.gray },
                alignment = TextAnchor.UpperLeft
            };

            var total = 0;
            foreach (var marks in _marks.Values)
            {
                total += marks.Length;
            }
            GUILayout.Label($"Total scanned: {total}", totalLabelStyle);

            foreach (var (markTypeName, marks) in _marks)
            {
                if (!_settings.markTypes.Names.Contains(markTypeName)) continue;
                var marksMatchingSearch = marks.Where(mark => mark.comment.ToLower().Contains(_searchString.ToLower().Trim()));
                if (marksMatchingSearch.Count() == 0) continue;

                var headerStyle = new GUIStyle(GUI.skin.label)
                {
                    font = _fontBold,
                    fontSize = 18,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = Color.white },
                };

                var headerAmountStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new(0, 0, 0, 0.4f) },
                };

                var headerRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight + 2f);
                headerRect.y = Mathf.Max(_scrollOffset + headerRect.y, 42);

                var idx = 1;
                foreach (var mark in marksMatchingSearch)
                {
                    if (_foldedMarkTypes.Contains(markTypeName)) break;

                    var markRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight + 2f);
                    markRect.y = Mathf.Max(_scrollOffset + markRect.y, 42);

                    var markInteractionRect = new Rect
                    (
                        markRect.x,
                        Mathf.Max(markRect.y, 42 + headerRect.height),
                        markRect.width,
                        markRect.height
                    );

                    if (markInteractionRect.y == 42 + headerRect.height)
                    {
                        markInteractionRect.height += _scrollOffset % markRect.height;
                    }

                    if (markRect.y == 42) markInteractionRect.height = 0;
                    EditorGUIUtility.AddCursorRect(markInteractionRect, MouseCursor.Link);

                    Color targetColor = idx % 2 == 0 ?
                        EditorGUIUtility.isProSkin ? new(0.18f, 0.18f, 0.18f) : new(0.82f, 0.82f, 0.82f) :
                        EditorGUIUtility.isProSkin ? new(0.16f, 0.16f, 0.16f) : new(0.84f, 0.84f, 0.84f);

                    if (markInteractionRect.Contains(Event.current.mousePosition))
                    {
                        targetColor = EditorGUIUtility.isProSkin ? new(0.14f, 0.14f, 0.14f) : new(0.8f, 0.8f, 0.8f);
                    }

                    EditorGUI.DrawRect(markRect, targetColor);
                    EditorGUILayout.BeginHorizontal();

                    var textStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        font = _fontRegular,
                        fontSize = 16
                    };

                    var filename = Path.GetFileName(mark.filepath);

                    var labelLine = $"{mark.line}";
                    var labelType = $"{mark.type}:";
                    var labelComment = mark.comment;
                    var label = string.Format(" {0, -4}{1} {2}", labelLine, labelType, labelComment);

                    var maxLabelLength = Mathf.Max(Screen.width / 8 - (markInteractionRect.Contains(Event.current.mousePosition) ? 5 : filename.Length + 2), 0);
                    var labelEllipsis = label.Length > maxLabelLength ? label[..maxLabelLength] + "..." : label;
                    var isEllipsed = label.Length > maxLabelLength;

                    var displayLabelLine = labelEllipsis[..Mathf.Min(labelEllipsis.Length, 5)];
                    var displayLabelType = labelEllipsis.Length < 5 ? "" : labelEllipsis[5..Mathf.Min(labelEllipsis.Length, 5 + labelType.Length)];
                    var displayLabelComment = labelEllipsis.Length < 5 + labelType.Length ? "" : labelEllipsis[(5 + labelType.Length)..labelEllipsis.Length];
                    var displayLabel = $"<color=grey>{displayLabelLine}</color><color=#{ColorUtility.ToHtmlStringRGBA(_settings.markTypes[mark.type])}>{displayLabelType}</color>{displayLabelComment}";

                    EditorGUI.LabelField(markRect, displayLabel, textStyle);

                    var filenameStyle = new GUIStyle(GUI.skin.label)
                    {
                        font = _fontRegular,
                        alignment = TextAnchor.MiddleRight,
                        normal = { textColor = Color.gray }
                    };

                    if (!isEllipsed || !markInteractionRect.Contains(Event.current.mousePosition)) EditorGUI.LabelField(markRect, filename, filenameStyle);

                    if (GUI.Button(markInteractionRect, isEllipsed ? new GUIContent("", mark.comment) : new GUIContent(""), GUIStyle.none))
                    {
                        InternalEditorUtility.OpenFileAtLineExternal(mark.filepath, mark.line, 0);
                    }

                    EditorGUILayout.EndHorizontal();
                    idx++;
                }

                if (headerRect.size == Vector2.one) continue;

                EditorGUI.DrawRect(headerRect, _settings.markTypes[markTypeName]);

                var sUpper = char.IsUpper(markTypeName[^1]);
                EditorGUI.LabelField(new(headerRect.x, headerRect.y, headerRect.width / 2, headerRect.height), sUpper ? $"{markTypeName}S" : $"{markTypeName}s", headerStyle);
                EditorGUI.LabelField(new(headerRect.x + headerRect.width / 2, headerRect.y, headerRect.width / 2, headerRect.height), $"({marksMatchingSearch.Count()})", headerAmountStyle);

                if (!_foldButtonHeights.ContainsKey(markTypeName))
                {
                    _foldButtonHeights.Add(markTypeName, headerRect.height);
                }

                var markTypeIdx = _foldButtonHeights.Keys.ToList().IndexOf(markTypeName);
                if (markTypeIdx != 0)
                {
                    var previous = _foldButtonHeights.ElementAt(markTypeIdx - 1).Key;

                    if (headerRect.y < 42 + headerRect.height)
                        _foldButtonHeights[previous] = headerRect.y - 42;
                    else
                        _foldButtonHeights[previous] = headerRect.height;
                }

                var foldButtonRect = new Rect(headerRect.x, headerRect.y, headerRect.height, _foldButtonHeights[markTypeName]);
                var foldIconRect = new Rect(foldButtonRect.x + 2f, foldButtonRect.y + 2f, foldButtonRect.width - 4f, foldButtonRect.width - 4f);

                _tempColor = GUI.color;
                if (foldButtonRect.Contains(Event.current.mousePosition))
                {
                    GUI.color = new(1, 1, 1, 0.5f);
                }
                else GUI.color = Color.white;

                GUI.DrawTexture(foldIconRect, _foldedMarkTypes.Contains(markTypeName) ? _foldedIcon : _unfoldedIcon);
                GUI.color = _tempColor;

                if (GUI.Button(foldButtonRect, "", GUIStyle.none))
                {
                    if (_foldedMarkTypes.Contains(markTypeName)) _foldedMarkTypes.Remove(markTypeName);
                    else
                    {
                        _foldedMarkTypes.Add(markTypeName);
                        if (headerRect.y == 42) _scrollOffset = _headersYPositions[markTypeName];
                    }
                }
            }

            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "", GUIStyle.none))
            {
                GUI.FocusControl(null);
            }
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!_settings) _settings = OrganizerUtils.GetSettings();

            if (_settings.autoScan) Rescan();
        }

        public static void Rescan()
        {
            if (!_settings) _settings = OrganizerUtils.GetSettings();

            _marks.Clear();
            var scanPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../")) + _settings.folderPath;
            RequestComments(scanPath, (data) =>
            {
                foreach (var type in _settings.markTypes)
                {
                    var marks = new List<Mark>();

                    foreach (var (script, comments) in data)
                    {
                        foreach (var (comment, line) in comments)
                        {
                            if (comment.StartsWith($"{type.Name}:"))
                            {
                                var mark = new Mark(type.Name, comment[(type.Name.Length + 1)..].TrimStart(), script, line);
                                marks.Add(mark);
                            }
                        }
                    }

                    if (marks.Count > 0)
                    {
                        if (_marks.ContainsKey(type.Name)) _marks[type.Name] = marks.ToArray();
                        else _marks.Add(type.Name, marks.ToArray());
                    }
                }
            });
        }

        private struct CommentsExtractionData
        {
            public Action<List<(string script, List<(string comment, int line)> comments)>> callback;
            public List<(string script, List<(string comment, int line)> comments)> data;
        }

        private static readonly Queue<CommentsExtractionData> commentsExtractionThreadData = new();

        private static void RequestComments(string scanPath, Action<List<(string script, List<(string comment, int line)> comments)>> callback)
        {
            var threadStart = new ThreadStart(() => CommentsExtractionThread(scanPath, callback));
            new Thread(threadStart).Start();
        }

        private static void CommentsExtractionThread(string scanPath, Action<List<(string script, List<(string comment, int line)> comments)>> callback)
        {
            var output = new List<(string script, List<(string comment, int line)> comments)>();

            var scripts = Directory.GetFiles(scanPath, "*.cs", SearchOption.AllDirectories);
            foreach (var script in scripts)
            {
                var text = File.ReadAllText(script);
                var comments = OrganizerUtils.GetComments(text);
                output.Add((script, comments));
            }

            lock (commentsExtractionThreadData)
            {
                commentsExtractionThreadData.Enqueue(new() { data = output, callback = callback });
            }
        }
    }
}