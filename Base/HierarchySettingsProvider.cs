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
        
        public StickyIcon effectiveIcon = StickyIcon.NotOnPrefabs;
        public TransformIcon transformIcon = TransformIcon.OnUniqueOrigin;
        
        public bool keepFoldersInPlaymode;
        
        public bool enableHoverPreview;
        public bool alwaysShowPreview;
        public ModificationKey previewKey;

        public bool preferLastComponent = true;
        public TypesPriority componentsPriority = new TypesPriority();
    }

    internal class HierarchySettingsProvider : SettingsProvider
    {
        private const string PreferencePath = "Preferences/Workflow/Smart Hierarchy";
        private static string UIPath = AssetDatabase.GUIDToAssetPath("f0d92e1f03926664991b2f7fbfbd6268") + "/";

        private static HierarchySettingsProvider provider;
        public static HierarchyPreferences Preferences 
        {
            get
            {
                if (!preferences)
                    LoadFromJson();
                return preferences;
            }
        }

        private static HierarchyPreferences preferences;
        public static event Action onChange;

        private SerializedObject serializedObject;

        private HierarchySettingsProvider(string path, SettingsScope scope)
            : base(path, scope){}

        public override void OnActivate(string searchContext, VisualElement root)
        {
            LoadFromJson();
            serializedObject = new SerializedObject(preferences);
            keywords = GetSearchKeywordsFromSerializedObject(serializedObject);
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIPath + "smart_hierarchy_settings.uxml");
            visualTree.CloneTree(root);
            
            var scrollView = root.Query<ScrollView>().First();
            var container = scrollView.contentContainer;
            
            ApplyStyling(root);
            root.Bind(serializedObject);

            var componentsFoldout = root.Query("Components").First();

            provider.CreateTypesPriorityGUI("Prioritized Types", componentsFoldout, "componentsPriority");
            
            // this is stupid
            container.RegisterCallback<ChangeEvent<bool>>(evt => SaveToJson());
            container.RegisterCallback<ChangeEvent<Enum>>(evt => SaveToJson());
        }

        public override void OnDeactivate()
        {
            if (preferences)
                SaveToJson();
        }

        private void CreateTypesPriorityGUI(string header, VisualElement parent, string propertyName)
        {
            var gui = new TypesPriorityGUI(header, serializedObject.FindProperty(propertyName));
            gui.onChange += SaveToJson;
            
            var container = new IMGUIContainer(() => gui.List.DoLayoutList());

            parent.Add(container);
        }

        private static void ApplyStyling(VisualElement root)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIPath + "preferences-style.uss");
            var foldoutStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIPath + "nice-foldout-header.uss");
            root.styleSheets.Add(styleSheet);
            root.styleSheets.Add(foldoutStyle);

            if (EditorGUIUtility.isProSkin)
            {
                var foldoutDarkStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIPath + "nice-foldout-header_dark.uss");
                root.styleSheets.Add(foldoutDarkStyle);
            }
        }

        private static void LoadFromJson()
        {
            preferences = ScriptableObject.CreateInstance<HierarchyPreferences>();
            var json = EditorPrefs.GetString(PreferencePath);
            EditorJsonUtility.FromJsonOverwrite(json, preferences);
        }

        private static void SaveToJson()
        {
            var json = EditorJsonUtility.ToJson(preferences, true);
            EditorPrefs.SetString(PreferencePath, json);
            
            onChange?.Invoke();
        }

        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider()
        {
            return provider ?? (provider = new HierarchySettingsProvider(PreferencePath, SettingsScope.User));
        }
    }
}