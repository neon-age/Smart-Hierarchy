using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    [CustomEditor(typeof(Collection))]
    [CanEditMultipleObjects]
    internal class CollectionEditor : Editor
    {
        // TODO: Remove all this reflection junk and use UI Elements instead.
        private static class Reflected
        {
            public static Type gameObjectInspectorType;
            public static Type inspectorWindowType;
            // Need this to hide GameObject editor in inspector
            public static FieldInfo hideInspector;
            
            public static PropertyInfo getInspectorTracker;
            public static Func<object[]> getAllInspectorWindows;

            static Reflected()
            {
                gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
                inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

                var getAllInspectorsMethod = inspectorWindowType.GetMethod("GetAllInspectorWindows",
                    BindingFlags.NonPublic | BindingFlags.Static);
                getAllInspectorWindows =
                    Expression.Lambda<Func<object[]>>(Expression.Call(null, getAllInspectorsMethod)).Compile();
                getInspectorTracker = inspectorWindowType.GetProperty("tracker", 
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                hideInspector =
                    typeof(Editor).GetField("hideInspector",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
        
        private static StyleSheet helpBoxStyle;


        private SerializedProperty keepHierarchy;
        
        private new Transform target;
        private new Transform[] targets;
        // TODO: Local-Hierarchy
        private GameObject[] children;

        private VisualElement root;
        private VisualElement editorsList;
        private VisualElement componentsWarning;

        private int editorsCount;
        

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            // Reselect to update folder header
            var instanceIDs = Selection.instanceIDs;
            Selection.instanceIDs = instanceIDs;
        }
        
        private void OnEnable()
        {
            target = (base.target as Collection).transform;
            targets = new Transform[base.targets.Length];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = (base.targets[i] as Collection).transform;

            children = new GameObject[target.transform.childCount];
            for (int i = 0; i < target.childCount; i++)
            {
                children[i] = target.GetChild(i).gameObject;
            }

            foreach (var target in targets)
            {
                if((target.hideFlags & HideFlags.HideInInspector) == 0)
                    target.hideFlags |= HideFlags.HideInInspector;
            }

            SetGameObjectInspectorHidden(true);
        }

        private void OnDestroy()
        {
            foreach (var target in targets)
            {
                if (target == null)
                    return;
                    
                // Show transform when folder component is removed
                if (!target.TryGetComponent<Collection>(out _))
                    target.hideFlags ^= HideFlags.HideInInspector;
            }
        }
        
        private void SetGameObjectInspectorHidden(bool hide)
        {
            foreach (var inspector in Reflected.getAllInspectorWindows())
            {
                var tracker = Reflected.getInspectorTracker.GetValue(inspector, null) as ActiveEditorTracker;

                for (var i = 1; i < tracker.activeEditors.Length; i++)
                {
                    var editor = tracker.activeEditors[i];
                    
                    if (editor == null)
                        continue;
                    
                    if (!(editor.target is Collection folder))
                        continue;
                    
                    foreach (var gameObjectInspector in Resources.FindObjectsOfTypeAll(Reflected.gameObjectInspectorType))
                    {
                        var goEditor = gameObjectInspector as Editor;
                        var gameObject = goEditor.target as GameObject;

                        // Hide GameObject inspector only for folders
                        if (folder.gameObject == gameObject)
                        {
                            Reflected.hideInspector.SetValue(gameObjectInspector, hide);
                        }
                    }
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (helpBoxStyle == null)
                helpBoxStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("d23f3b0d4e030dc43a7d023bc53a5508"));
        
            keepHierarchy = serializedObject.FindProperty("keepTransformHierarchy");
        
            root = new VisualElement { style = { paddingTop = 12, paddingBottom = 5 } };

            // TODO: Tooltip doesn't work?!
            var keepHierarchyField = new PropertyField(keepHierarchy) { tooltip = keepHierarchy.tooltip };
            root.Add(keepHierarchyField);
            
            root.RegisterCallback<ChangeEvent<bool>>(_ => SetComponentsDisabled(!keepHierarchy.boolValue));
            root.RegisterCallback<AttachToPanelEvent>(_ => OnInspectorInitialize());
            
            return root;
        }

        private void OnInspectorInitialize()
        {
        #if UNITY_2019_3 || UNITY_2019_4
            var inspectors = Resources.FindObjectsOfTypeAll(Reflected.inspectorWindowType);
            foreach (EditorWindow inspector in inspectors)
                inspector.SetAntiAliasing(8);
        #endif
        
            editorsList = root.parent.parent.parent;
            editorsList.RegisterCallback<GeometryChangedEvent>(OnEditorsListChange);
        }

        private void OnDisable()
        {
            editorsList?.UnregisterCallback<GeometryChangedEvent>(OnEditorsListChange);
        }

        private void OnEditorsListChange(GeometryChangedEvent evt)
        {
            if (editorsCount == editorsList.childCount)
                return;
                    
            editorsCount = editorsList.childCount;
            SetComponentsDisabled(!keepHierarchy.boolValue);
        }

        private void SetComponentsDisabled(bool disabled)
        {
            var componentsByName = target.GetComponents<Component>().Where(c => c != null).ToDictionary(ObjectNames.GetInspectorTitle);
            var componentsCount = 0;

            foreach (var editor in editorsList.Children())
            {
                var header = editor[0];
                var componentName = header.name;
                
                if (componentName.EndsWith("Header"))
                    componentName = componentName.Remove(componentName.Length - 6, 6);
                
                if (componentsByName.TryGetValue(componentName, out var component))
                {
                    if (component.GetType() == typeof(Collection))
                        continue;

                    componentsCount++;
                    SetDisabledGroup(editor, disabled);
                }
            }

            if (componentsCount == 0)
                disabled = false;

            var componentButton = editorsList.parent.Query(className: "unity-inspector-add-component-button").First();
            
            componentButton.SetEnabled(!disabled);


            if (componentsWarning == null)
            {
                componentsWarning = new VisualElement { name = "ComponentsWarning" };
                
                componentsWarning.style.SetBorderColor(isProSkin ? new Color(0, 0, 0, 0.5f) : new Color(0.33f, 0.33f, 0.33f, 0.5f));
                componentsWarning.styleSheets.Add(helpBoxStyle);
                
                componentsWarning.Add(new Image { name = "Icon", image = IconContent("console.erroricon").image });
                componentsWarning.Add(new TextElement
                {
                    name = "Warning",
                    text = "Collection and components are stripped during build process.\n" +
                           "Use \"Keep Transform Hierarchy\" to keep this object in build.\n",
                    #if UNITY_2019_3 || UNITY_2019_4
                    style = { marginBottom = -10 }
                    #endif
                });
            }

            if (disabled)
                root.Add(componentsWarning);
            else
                componentsWarning.RemoveFromHierarchy();
        }
        
        private VisualElement SetDisabledGroup(VisualElement target, bool disabled)
        {
            var disabledGroup = target.Query(className: "disabled-group").First();
            
            if (disabledGroup == null && disabled)
            {
                disabledGroup = new VisualElement { style = {
                    position = Position.Absolute,
                    top = 0, left = 0, right = 0, bottom = 0,
                    backgroundColor = isProSkin ? new Color(0.23f, 0.23f, 0.23f, 0.5f) : new Color(0.77f, 0.77f, 0.77f, 0.5f)
                }};
                
                disabledGroup.pickingMode = PickingMode.Ignore;
                disabledGroup.AddToClassList("disabled-group");
                target.Add(disabledGroup);
            }
            else if (disabledGroup != null && !disabled)
            {
                disabledGroup.RemoveFromHierarchy();
            }

            return disabledGroup;
        }
    }
}