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
            // Need this to hide GameObject editor in inspector
            public static FieldInfo hideInspector;
            
            public static PropertyInfo getInspectorTracker;
            public static Func<object[]> getAllInspectorWindows;

            static Reflected()
            {
                gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
                var inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

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

        private SerializedProperty keepHierarchy;
        
        private new Transform target;
        private new Transform[] targets;
        // TODO: Local-Hierarchy
        private GameObject[] children;

        private VisualElement root;
        private VisualElement componentsWarning;

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            // Reselect to update folder header
            var instanceIDs = Selection.instanceIDs;
            Selection.instanceIDs = instanceIDs;
        }
        
        private void OnEnable()
        {
            keepHierarchy = serializedObject.FindProperty("keepTransformHierarchy");
            
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
                        var goEditor = gameObjectInspector as UnityEditor.Editor;
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
            root = new VisualElement { style = { paddingTop = 12, paddingBottom = 5 } };

            // TODO: Tooltip doesn't work?!
            var keepHierarchyField = new PropertyField(keepHierarchy) { tooltip = keepHierarchy.tooltip };
            root.Add(keepHierarchyField);
            
            root.RegisterCallback<ChangeEvent<bool>>(_ => SetComponentsReadOnly(!keepHierarchy.boolValue));

            // TODO: Find any better callback for initialization
            EditorApplication.delayCall += () => SetComponentsReadOnly(!keepHierarchy.boolValue);
            
            return root;
        }

        private void SetComponentsReadOnly(bool readOnly)
        {
            var inspectorEditorsList = root.parent.parent.parent;
            
            var componentsByName = target.GetComponents<Component>().ToDictionary(ObjectNames.GetInspectorTitle);
            var componentsCount = 0;

            foreach (var editor in inspectorEditorsList.Children())
            {
                var header = editor[0];
                var componentName = header.name.TrimEnd("Header".ToCharArray());

                if (componentsByName.TryGetValue(componentName, out var component))
                {
                    if (component.GetType() == typeof(Collection))
                        continue;

                    componentsCount++;
                    SetDisabledGroup(editor, readOnly);
                }
            }

            if (componentsCount == 0)
                readOnly = false;

            var componentButton = inspectorEditorsList.parent.Query(className: "unity-inspector-add-component-button").First();
            
            componentButton.SetEnabled(!readOnly);


            if (componentsWarning == null)
            {
                componentsWarning = new HelpBox(
                "Collection and components are stripped during build process.\n" +
                "Use \"Keep Transform Hierarchy\" to keep this object in build.\n", HelpBoxMessageType.Error)
                {
                    name = "ComponentsWarning", style = { flexDirection = FlexDirection.Row }
                };

                componentsWarning.Query<Label>().First().style.fontSize = 11;
                
                componentsWarning.style.borderTopLeftRadius = 6;
                componentsWarning.style.borderTopRightRadius = 6;
                componentsWarning.style.borderBottomLeftRadius = 6;
                componentsWarning.style.borderBottomRightRadius = 6;
                
                //componentsWarning.Add(new Image { image = IconContent("console.erroricon.sml").image, style = { alignSelf = Align.FlexStart }});
                //componentsWarning.Add(new TextElement
                //{
                //    text = "Attached components will be stripped during build process.\n" +
                //           "Enable \"Keep Transform Hierarchy\" so it works like regular GameObject.\n" +
                //           "Use only when you know that transform overhead is doable."
                //});
            }

            if (readOnly)
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
                    backgroundColor = new Color(0.23f, 0.23f, 0.23f, 0.5f)
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