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

        [Serializable]
        internal class FoldoutStateLookup : SerializableLookup<string, bool> {}
        
        //public bool showVisibilityToggle = true;
        //public bool showPickingToggle = true;
        //public bool showActivationToggle = true;
        //public bool showPrefabModeToggle = true;

        public Layout layout = new Layout();
        
        [SerializeField]
        public List<HierarchyTool> tools = new List<HierarchyTool>();

        [SerializeField]
        internal FoldoutStateLookup foldouts = new FoldoutStateLookup();

        //public HierarchyTool this[int index] => tools[index];
        
        private readonly Dictionary<Type, HierarchyTool> toolsLookup = new Dictionary<Type, HierarchyTool>();

        private static TypeCache.TypeCollection ToolTypes = TypeCache.GetTypesDerivedFrom<HierarchyTool>();
        
        
        public void InitializeToolsList()
        {
            for (int i = tools.Count - 1; i >= 0; i--)
            {
                if (tools[i] == null)
                    tools.RemoveAt(i);
            }
            
            foreach (var toolType in ToolTypes)
            {
                if (tools.Any(x => x.GetType() == toolType))
                    continue;
                    
                //var instance = Activator.CreateInstance(toolType) as HierarchyTool;
                var toolInstance = CreateInstance(toolType) as HierarchyTool;
                tools.Add(toolInstance);
            }

            tools = tools.OrderBy(x => x.order).ToList();

            toolsLookup.Clear();
            
            foreach (var tool in tools)
                toolsLookup.Add(tool.GetType(), tool);
        }

        
        private void OnLoad()
        {
            InitializeToolsList();
        }
        
        public void Save()
        {
            var json = EditorJsonUtility.ToJson(this);
            File.WriteAllText(assetPath, json);
        }

        public void Load()
        {
            if (!File.Exists(assetPath))
            {
                Save();
                OnLoad();
                return;
            }

            EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), this);
            OnLoad();
        }

        public HierarchyTool GetTool(Type toolType)
        {
            toolsLookup.TryGetValue(toolType, out var tool);
            return tool;
        }
        public T GetTool<T>() where T : HierarchyTool
        {
            return (T)GetTool(typeof(T));
        }
        
        public bool IsToolEnabled<T>() where T : HierarchyTool
        {
            var hasTool = toolsLookup.TryGetValue(typeof(T), out var tool);
            if (!hasTool)
                return false;
            return ((T)tool).enabled;
        }
    }
}