using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

    internal enum ModificationKey
    {
        Alt,
        Shift,
        Control,
    }
    
    internal class HierarchyPreferences : ScriptableObject
    {
        public bool enableSmartHierarchy = true;
        public StickyIcon stickyComponentIcon = StickyIcon.NotOnPrefabs;
        public TransformIcon transformIcon = TransformIcon.OnUniqueOrigin;
        public bool keepFoldersInPlaymode;
        public bool enableHoverPreview;
        public bool alwaysShowPreview;
        public ModificationKey magnifyHoldKey;
    }

    internal class HierarchySettingsProvider : SettingsProvider
    {
        private const string PreferencePath = "Preferences/Workflow/Smart Hierarchy";

        private static HierarchySettingsProvider provider;
        public static HierarchyPreferences Preferences 
        {
            get
            {
                if (preferences == null)
                {
                    preferences = ScriptableObject.CreateInstance<HierarchyPreferences>();
                    LoadFromJson();
                }

                return preferences;
            }
        }

        private static HierarchyPreferences preferences;
        public static event Action onChange;


        private HierarchySettingsProvider(string path, SettingsScope scope)
            : base(path, scope){}

        public override void OnActivate(string searchContext, VisualElement root)
        {
            LoadFromJson();
            
            var uiPath = AssetDatabase.GUIDToAssetPath("f0d92e1f03926664991b2f7fbfbd6268") + "/";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(uiPath + "nice-foldout-header.uss");
            root.styleSheets.Add(styleSheet);

            if (EditorGUIUtility.isProSkin)
            {
                var darkStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(uiPath + "nice-foldout-header_dark.uss");
                root.styleSheets.Add(darkStyle);
            }
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uiPath + "smart_hierarchy_settings.uxml");
            
            visualTree.CloneTree(root);

            var serializedObject = new SerializedObject(preferences);
            root.Bind(serializedObject);
            keywords = GetSearchKeywordsFromSerializedObject(serializedObject);
            
            // this is stupid
            root.RegisterCallback<ChangeEvent<bool>>(evt => SaveToJson());
            root.RegisterCallback<ChangeEvent<Enum>>(evt => SaveToJson());
        }

        private static void LoadFromJson()
        {
            var json = EditorPrefs.GetString(PreferencePath);
            EditorJsonUtility.FromJsonOverwrite(json, preferences);
        }

        private static void SaveToJson()
        {
            var json = EditorJsonUtility.ToJson(preferences);
            EditorPrefs.SetString(PreferencePath, json);
            onChange?.Invoke();
        }

        public static HierarchySettingsProvider GetProvider() => (HierarchySettingsProvider)GetSettingsProvider();
        
        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider()
        {
            return provider ?? (provider = new HierarchySettingsProvider(PreferencePath, SettingsScope.User));
        }
    }
}