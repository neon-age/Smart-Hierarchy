using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUI;

namespace AV.Editor.Hierarchy
{
    internal enum StickIcon
    {
        Never,
        OnAnyObject,
        NotOnPrefabs
    }
    
    internal class HierarchyPreferences : ScriptableObject
    {
        public bool enableSmartHierarchy = true;
        public StickIcon stickComponentIcon = StickIcon.OnAnyObject;
    }
    
    internal class HierarchySettingsProvider : SettingsProvider
    {
        private class Contents
        {
            public static GUIContent enableSmartHierarchy = new GUIContent("Enable Smart Hierarchy");
            public static GUIContent stickComponentIcon = new GUIContent("Stick Component Icon");
        }
        
        private const string Path = "Preferences/Workflow/Smart Hierarchy";
        private static HierarchySettingsProvider provider;
        
        public HierarchyPreferences preferences { get; private set; }
        public event Action onChange;

        private SerializedObject serializedObject;

        private HierarchySettingsProvider(string path, SettingsScope scope)
            : base(path, scope){}

        public override void OnActivate(string searchContext, VisualElement root)
        {
            LoadFromJson();
            
            serializedObject = new SerializedObject(preferences);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.av.smart-hierarchy/UI/nice-foldout-header.uss");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.av.smart-hierarchy/UI/smart_hierarchy_settings.uxml");

            root.styleSheets.Add(styleSheet);
            
            visualTree.CloneTree(root);
            root.Bind(serializedObject);
            
            // this is stupid
            root.RegisterCallback<ChangeEvent<bool>>(evt => SaveToJson());
            root.RegisterCallback<ChangeEvent<Enum>>(evt => SaveToJson());
        }

        private void LoadFromJson()
        {
            var json = EditorPrefs.GetString(Path);
            EditorJsonUtility.FromJsonOverwrite(json, preferences);
        }

        private void SaveToJson()
        {
            var json = EditorJsonUtility.ToJson(preferences);
            EditorPrefs.SetString(Path, json);
        }

        public static HierarchySettingsProvider GetProvider() => (HierarchySettingsProvider)GetSettingsProvider();
        
        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider()
        {
            if (provider == null)
            {
                provider = new HierarchySettingsProvider(Path, SettingsScope.User)
                {
                    keywords = GetSearchKeywordsFromGUIContentProperties<Contents>()
                };
            }

            if (provider.preferences == null)
            {
                provider.preferences = ScriptableObject.CreateInstance<HierarchyPreferences>();
                provider.LoadFromJson();
            }

            return provider;
        }
    }
}