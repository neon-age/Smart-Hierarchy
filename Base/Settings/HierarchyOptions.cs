using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class HierarchyOptions : ScriptableObject
    {
        [Serializable]
        internal class Layout
        {
            public const float IndentWidthMin = 6;
            public const float IndentWidthMax = 14;
            
            [Range(16, 24)]
            public int lineHeight = 16;
            
            [Range(IndentWidthMin, IndentWidthMax)]
            public int indentWidth = 14;
            
            [Range(6, 24)]
            public int minIndent = 6;
        }
        
        public bool showVisibilityToggle = true;
        public bool showPickingToggle = true;
        public bool showActivationToggle = true;
        public bool showPrefabModeToggle = true;

        public Layout layout;
        
        
        public static HierarchyOptions instance
        {
            get
            {
                if (!loadedInstance)
                {
                    loadedInstance = CreateInstance<HierarchyOptions>();
                    loadedInstance.Load();
                }
                return loadedInstance;
            }
        }
        private static HierarchyOptions loadedInstance;

        private static string assetPath => Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 7, 7), "Library", "HierarchyOptions.asset");
        
        public void Save()
        {
            var json = EditorJsonUtility.ToJson(this);
            File.WriteAllText(assetPath, json);
        }

        public void Load()
        {
            if (!File.Exists(assetPath))
                Save();
            
            EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), this);
        }
    }
}