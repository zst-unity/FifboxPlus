using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.Editor.Organizer
{
    public static class OrganizerUtils
    {
        public static string GetRoot([System.Runtime.CompilerServices.CallerFilePath] string fullPath = null)
        {
            var path = $"Assets{Path.GetFullPath(Path.Combine(fullPath, "../")).Split("Assets")[1]}";
            return path;
        }

        public static OrganizerSettings GetSettings()
        {
            var path = $"{GetRoot()}Settings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<OrganizerSettings>(path);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<OrganizerSettings>();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssetIfDirty(settings);
            }

            return settings;
        }

        public static List<(string, int)> GetComments(string text)
        {
            var comments = new List<(string, int)>();

            var line = 1;
            var comment = "";
            var parsingCommentType = -1;
            var writingStringType = -1;

            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '\n') line++;

                if (ch == '"' && parsingCommentType == -1)
                {
                    if (i > 0)
                    {
                        var backslashesBefore = 0;
                        var j = i - 1;
                        while (text[j] == '\\' && j < 0)
                        {
                            backslashesBefore++;
                            j--;
                        }
                        var isEscaped = backslashesBefore % 2 == 1;

                        if (writingStringType == -1)
                        {
                            if (text[i - 1] == '@') writingStringType = 1;
                            else if (text[i - 1] != '\'' && !isEscaped) writingStringType = 0;
                        }
                        else if (writingStringType == 0)
                        {
                            if (text[i - 1] != '\'' && !isEscaped) writingStringType = -1;
                        }
                        else if (writingStringType == 1)
                        {
                            writingStringType = -1;
                        }
                    }
                    else
                    {
                        writingStringType = 0;
                    }
                }

                if (writingStringType != -1) continue;
                if (parsingCommentType == -1 && ch != '/') continue;
                if (parsingCommentType != 0 && i == text.Length - 1) break;

                if (parsingCommentType == -1)
                {
                    if (ch == '/' && text[i + 1] == '/')
                    {
                        parsingCommentType = 0;
                        i++;
                        continue;
                    }
                    else if (ch == '/' && text[i + 1] == '*')
                    {
                        parsingCommentType = 1;
                        i++;
                        continue;
                    }
                }
                else
                {
                    if (parsingCommentType == 0)
                    {
                        if (ch == '/' && comment == "")
                        {
                            parsingCommentType = -1;
                            continue;
                        }
                        else if (ch == '\r' || ch == '\n' || i == text.Length - 1)
                        {
                            comments.Add((comment.Trim(), line));
                            comment = "";

                            parsingCommentType = -1;
                            continue;
                        }
                    }
                    else if (parsingCommentType == 1)
                    {
                        if (i == text.Length - 1) break;

                        if (ch == '*' && text[i + 1] == '/')
                        {
                            if (comment != "")
                            {
                                comments.Add((comment.Trim(), line));
                                comment = "";
                            }

                            parsingCommentType = -1;
                            i++;
                            continue;
                        }
                    }

                    comment += ch;
                }
            }

            return comments;
        }
    }
}