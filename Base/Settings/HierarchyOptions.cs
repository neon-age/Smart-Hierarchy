using System.IO;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class HierarchyOptions : ScriptableObject
    {
        public bool showVisibilityToggle = true;
        public bool showPickingToggle = true;
        public bool showActivationToggle = true;
        public bool showPrefabModeToggle = true;
        
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