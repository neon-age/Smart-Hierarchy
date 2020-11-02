using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUI;

namespace AV.Hierarchy
{
    internal enum StickyIcon
    {
        Never,
        OnAnyObject,
        NotOnPrefabs
    }
    internal enum TransformIcon
    {
        Never,
        Always,
        OnUniqueOrigin,
        OnlyRectTransform
    }
    
    internal class HierarchyPreferences : ScriptableObject
    {
        public bool enableSmartHierarchy = true;
        public StickyIcon stickyComponentIcon = StickyIcon.OnAnyObject;
        public TransformIcon transformIcon = TransformIcon.OnUniqueOrigin;
    }
    
    internal class HierarchySettingsProvider : SettingsProvider
    {
        private const string Path = "Preferences/Workflow/Smart Hierarchy";
        
        private static HierarchySettingsProvider provider;
        
        public HierarchyPreferences preferences { get; private set; }
        public event Action onChange;


        private HierarchySettingsProvider(string path, SettingsScope scope)
            : base(path, scope){}

        public override void OnActivate(string searchContext, VisualElement root)
        {
            LoadFromJson();
            
            const string UiPath = "Packages/com.av.smart-hierarchy/UI/";
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UiPath + "nice-foldout-header.uss");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiPath + "smart_hierarchy_settings.uxml");

            root.styleSheets.Add(styleSheet);

            if (EditorGUIUtility.isProSkin)
            {
                var darkStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(UiPath + "nice-foldout-header_dark.uss");
                root.styleSheets.Add(darkStyle);
            }
            
            visualTree.CloneTree(root);

            var serializedObject = new SerializedObject(preferences);
            root.Bind(serializedObject);
            keywords = GetSearchKeywordsFromSerializedObject(serializedObject);
            
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
                provider = new HierarchySettingsProvider(Path, SettingsScope.User);
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