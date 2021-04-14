using System.IO;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class HierarchyFilterOptions : ScriptableObject
    {
        public bool showVisibilityToggle = true;
        public bool showPickingToggle = true;
        public bool showActivationToggle = true;
        public bool showPrefabModeToggle = true;
        
        public static HierarchyFilterOptions instance
        {
            get
            {
                if (!loadedInstance)
                {
                    loadedInstance = CreateInstance<HierarchyFilterOptions>();
                    loadedInstance.Load();
                }
                return loadedInstance;
            }
        }
        private static HierarchyFilterOptions loadedInstance;

        private static string assetPath => Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 7, 7), "Library", "HierarchyFilterOptions.asset");
        
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