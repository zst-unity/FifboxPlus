using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.Editor.FoldersBeauty
{
    public class FoldersData : ScriptableObject
    {
        public static FoldersData Singleton
        {
            get
            {
                if (_singleton) return _singleton;

                var mainAssetPath = FoldersBeautyUtils.GetMainAssetPath();
                var path = $"{mainAssetPath}FoldersData.asset";
                _singleton = AssetDatabase.LoadAssetAtPath<FoldersData>(path);
                if (!_singleton)
                {
                    _singleton = CreateInstance<FoldersData>();
                    AssetDatabase.CreateAsset(_singleton, path);
                    AssetDatabase.SaveAssetIfDirty(_singleton);
                }

                return _singleton;
            }
        }

        private static FoldersData _singleton;

        public List<FolderData> folders = new();
    }

    [Serializable]
    public class FolderData
    {
        public string path;
        public Color darkThemeColor;
        public Color lightThemeColor;
    }
}