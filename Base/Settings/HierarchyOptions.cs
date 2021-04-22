using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    public class HierarchyOptions : ScriptableObject
    {
        private static string assetPath => Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 7, 7), "Library", "HierarchyOptions.asset");
        
        public static HierarchyOptions Instance
        {
            get
            {
                if (!instance)
                {
                    instance = CreateInstance<HierarchyOptions>();
                    instance.Load();
                }
                return instance;
            }
        }
        private static HierarchyOptions instance;
        
        [Serializable]
        public class Layout
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
        
        //public bool showVisibilityToggle = true;
        //public bool showPickingToggle = true;
        //public bool showActivationToggle = true;
        //public bool showPrefabModeToggle = true;

        public Layout layout;
        
        [SerializeField]
        internal HierarchyToolsList tools = new HierarchyToolsList();
        
        
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
            OnLoad();
        }

        public HierarchyTool GetTool(Type toolType)
        {
            tools.lookup.TryGetValue(toolType, out var tool);
            return tool;
        }
        public T GetTool<T>() where T : HierarchyTool
        {
            return (T)GetTool(typeof(T));
        }
        
        public bool IsToolEnabled<T>() where T : HierarchyTool
        {
            var hasTool = tools.lookup.TryGetValue(typeof(T), out var tool);
            if (!hasTool)
                return false;
            return ((T)tool).enabled;
        }

        private void OnLoad()
        {
           tools.Initialize();
        }
    }
}