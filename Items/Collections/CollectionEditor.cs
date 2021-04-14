using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    [CustomEditor(typeof(Collection))]
    [CanEditMultipleObjects]
    internal class CollectionEditor : Editor
    {
        private static Type inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        private static StyleSheet helpBoxStyle;

        private SerializedProperty keepHierarchy;
        private SerializedProperty colorTag;
        
        private new Collection target => base.target as Collection;
        private GameObject gameObject => target.gameObject;
        private Transform[] transforms = new Transform[0];

        private VisualElement root;
        private VisualElement editorsList;
        private VisualElement transformEditor;
        private VisualElement collectionEditor;
        private VisualElement addComponentButton;
        private VisualElement componentsWarning;

        private int editorsCount;
        

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            // Reselect to update folder header
            var instanceIDs = Selection.instanceIDs;
            Selection.instanceIDs = instanceIDs;
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (helpBoxStyle == null)
                helpBoxStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("d23f3b0d4e030dc43a7d023bc53a5508"));
        
            keepHierarchy = serializedObject.FindProperty("keepTransformHierarchy");
            colorTag = serializedObject.FindProperty("colorTag");
        
            root = new VisualElement { style = { paddingTop = 12, paddingBottom = 5 } };

            var colorTagField = new EnumField("Color Tag") { bindingPath = colorTag.propertyPath };
            colorTagField.RegisterValueChangedCallback(evt => EditorApplication.RepaintHierarchyWindow());
            root.Add(colorTagField);
            
            root.Add(new VisualElement { style = { height = 2 } });

            // TODO: Tooltip doesn't work?!
            var keepHierarchyField = new PropertyField(keepHierarchy) { tooltip = keepHierarchy.tooltip };
            root.Add(keepHierarchyField);
            
            // Needs to be called before OnEditorsListChange
            ShowTransformComponent();
            
            root.RegisterCallback<ChangeEvent<bool>>(_ => AlternateComponentsGUI(!keepHierarchy.boolValue));
            root.RegisterCallback<AttachToPanelEvent>(OnAttachRootToInspector);
            
            return root;
        }

        private void OnAttachRootToInspector(AttachToPanelEvent _)
        {
            root.UnregisterCallback<AttachToPanelEvent>(OnAttachRootToInspector);
            
        #if UNITY_2019_3 || UNITY_2019_4
            var inspectors = Resources.FindObjectsOfTypeAll(inspectorWindowType);
            foreach (EditorWindow inspector in inspectors)
                inspector.SetAntiAliasing(8);
        #endif

            editorsList = root.parent.parent.parent;
            editorsList.RegisterCallback<GeometryChangedEvent>(OnEditorsListChange);
        }

        private void ShowTransformComponent()
        {
            if (transforms.Length == targets.Length)
                return;
            
            transforms = new Transform[targets.Length];
            
            for (int i = 0; i < targets.Length; i++)
                transforms[i] = (targets[i] as Collection).transform;
            
            foreach (var transform in transforms)
                if (transform)
                    // hideFlags &= HideFlags.HideInInspector; doesn't work in 2019.4?
                    transform.hideFlags = HideFlags.None;
        }

        private void OnDestroy()
        {
            editorsList?.UnregisterCallback<GeometryChangedEvent>(OnEditorsListChange);
            editorsList?.Query(className: "element-dimmer").ForEach(x => x.RemoveFromHierarchy());
            
            // Custom header and editor lasts after Remove Component
            collectionEditor?.RemoveFromHierarchy();
            
            addComponentButton?.SetEnabled(true);
        }

        private void OnEditorsListChange(GeometryChangedEvent evt)
        {
            if (editorsCount == editorsList.childCount)
                return;
            editorsCount = editorsList.childCount;
            
            root.Query<Label>().ForEach(label => label.style.width = 160);

            AlternateComponentsGUI(!keepHierarchy.boolValue);
        }

        // TODO: Change header icon
        // TODO: Add color bar button
        // TODO: Add name field
        // TODO: Add tag / layer / static popups that will affect root or all children  
        protected override void OnHeaderGUI()
        {
            if (target == null)
                return;
            base.OnHeaderGUI();
        }

        private void AlternateComponentsGUI(bool isDisabled)
        {
            var componentsCount = 0;
            var editorsByName = target.GetComponents<Component>().Where(x => x != null).Cast<Object>().ToDictionary(ObjectNames.GetInspectorTitle);
           
            editorsByName.Add(ObjectNames.GetInspectorTitle(gameObject), gameObject);

            foreach (var editor in editorsList.Children())
            {
                var header = editor[0];
                var headerName = header.name;
                
                if (headerName.EndsWith("Header"))
                    headerName = headerName.Remove(headerName.Length - 6, 6);
                
                if (editorsByName.TryGetValue(headerName, out var obj))
                {
                    if (obj is Collection collection)
                    {
                        collectionEditor = editor;
                        
                        for (int i = 0; i < componentsCount + 1; i++)
                            ComponentUtility.MoveComponentUp(collection);
                        
                        collectionEditor.Query($"{headerName}Header").First().RemoveFromHierarchy();
                        collectionEditor.Insert(0, new IMGUIContainer(OnHeaderGUI));
                        
                        continue;
                    }

                    if (obj is Transform || obj is GameObject)
                    {
                        editor.RemoveFromHierarchy();
                        return;
                    }

                    componentsCount++;
                    SetElementDimmed(editor, isDisabled);
                }
            }

            if (componentsCount == 0)
                isDisabled = false;

            addComponentButton = editorsList.parent.Query(className: "unity-inspector-add-component-button").First();
            addComponentButton.SetEnabled(!isDisabled);


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
                    //#if UNITY_2019_3 || UNITY_2019_4
                    style = { marginBottom = -10 }
                    //#endif
                });
            }

            if (isDisabled)
                root.Add(componentsWarning);
            else
                componentsWarning.RemoveFromHierarchy();
        }
        
        private VisualElement SetElementDimmed(VisualElement target, bool dimmed)
        {
            var dimmer = target.Query(className: "element-dimmer").First();
            
            if (dimmer == null && dimmed)
            {
                dimmer = new VisualElement { style = {
                    position = Position.Absolute,
                    top = 0, left = 0, right = 0, bottom = 0,
                    backgroundColor = isProSkin ? new Color(0.23f, 0.23f, 0.23f, 0.5f) : new Color(0.77f, 0.77f, 0.77f, 0.5f)
                }};
                
                dimmer.pickingMode = PickingMode.Ignore;
                dimmer.AddToClassList("element-dimmer");
                target.Add(dimmer);
            }
            else if (dimmer != null && !dimmed)
            {
                dimmer.RemoveFromHierarchy();
            }

            return dimmer;
        }
    }
}