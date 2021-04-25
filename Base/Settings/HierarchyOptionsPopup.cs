using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    internal class HierarchyOptionsPopup : PopupElement
    {
        private class TabToggle : ToolbarToggle
        {
            /*
            protected override void ToggleValue()
            {
                // Ignore left click toggling
            }*/
        }
        
        private class GUIImage : IMGUIContainer
        {
            public override bool canGrabFocus => false;

            public GUIImage(Texture texture)
            {
                onGUIHandler = () =>
                {
                    GUI.color = new Color(1, 1, 1, resolvedStyle.opacity);
                    ViewItemGUI.DrawIconTexture(new Rect(0, 0, 16, 16), texture, Color.white, true);
                };
            }
        }

        private static Texture2D HelpIcon = IconContent("_Help").image as Texture2D;
        private static string UIPath = AssetDatabase.GUIDToAssetPath("f0d92e1f03926664991b2f7fbfbd6268") + "/";
        
        private Foldout activeToolGroup;
        private VisualElement activeToolTab;
        private VisualElement activeToolMarker;
        private IMGUIContainer activeToolGUI;
        
        private TooltipElement tooltipElement = new TooltipElement();

        private readonly HierarchyOptions options;
        private readonly SerializedObject serializedObject;
        
        
        public HierarchyOptionsPopup()
        {
            options = HierarchyOptions.Instance;
            serializedObject = new SerializedObject(options);
            
            activeToolMarker = new VisualElement { style =
            {
                backgroundColor = new Color(0.9f, 0.5f, 0.4f, 1),
                position = Position.Absolute,
                left = 0, right = 0,
                bottom = 0,
                height = 2
            }};
            
            var toolsTabs = new Toolbar { style = { flexGrow = 0 } };

            this.Bind(serializedObject);
            
            var togglesLabel = CreateLabel("Tools Options");
            CreateActiveToolGroup();

            var toolsProp = serializedObject.FindProperty("tools");
            
            for (int i = 0; i < toolsProp.arraySize; i++)
            {
                var tool = options.tools[i];

                if (tool == null)
                    continue;
                
                var content = new GUIContent(tool.title, tool.icon, tool.tooltip);
                
                var toolSerialized = new SerializedObject(toolsProp.GetArrayElementAtIndex(i).objectReferenceValue);
                var toolIterator = toolSerialized.GetIterator();
                
                var enabledProp = toolSerialized.FindProperty("enabled");

                var tab = CreateTabToggle(content, enabledProp);
                toolsTabs.Add(tab);

                //var tooltip = $"{tool.title}";
                //
                //if (tool.tooltip != "")
                //    tooltip += $"{Environment.NewLine}{tool.tooltip}";

                //tab.tooltip = tool.title;
                //tooltipElement.SetTooltipFor(tab, tooltip);
                
                /*
                tab.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.button != (int)MouseButton.LeftMouse)
                        return;
                    
                    //if (activeToolTab == tab)
                        tab.value = !tab.value;
                    
                    //SetActiveToolAndExpand(tab, tool, property);
                });
                */
                
                tab.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button != (int) MouseButton.RightMouse)
                        return;
                    
                    if (activeToolTab != tab)
                        SetActiveToolAndExpand(tab, tool, toolIterator);
                    else
                        ResetActiveToolGroup();
                });
            }
            
            Add(togglesLabel);
            Add(toolsTabs);
            Add(Space(5));
            Add(activeToolGroup);

            var layoutGUI = new IMGUIContainer(OnLayoutGUI);
            Add(Space(5));
            
            var layout = CreateFoldoutSaveState("Layout", true);
            layout.Add(layoutGUI);
            Add(layout);
            
            Add(new VisualElement { style = { width = 200, height = 1 } });
            
            RegisterCallback<ChangeEvent<bool>>(evt => HierarchyOptions.Instance.Save());
            RegisterCallback<ChangeEvent<int>>(evt => HierarchyOptions.Instance.Save());
        }

        protected override void OnAttach(AttachToPanelEvent evt)
        {
            root.Add(tooltipElement);
        }

        private void CreateActiveToolGroup()
        {
            activeToolGroup = CreateFoldout("Active Tool");
            
            activeToolGUI = new IMGUIContainer();
            activeToolGroup.Add(activeToolGUI);
            activeToolGroup.Add(Space(5));
            
            ResetActiveToolGroup();
        }

        private void ResetActiveToolGroup()
        {
            activeToolTab = null;
            
            activeToolGroup.text = "No Selected Tool";
            activeToolGroup.tooltip = "Right click on toggle to show options.";
            
            activeToolGroup.value = false;
            activeToolGroup.style.opacity = 0.5f;

            activeToolGUI.style.display = DisplayStyle.None;
            
            activeToolMarker.RemoveFromHierarchy();
        }

        private void SetActiveToolAndExpand(VisualElement sender, HierarchyTool tool, SerializedProperty property)
        {
            activeToolGroup.value = true;
            SetActiveTool(sender, tool, property);
        }

        private void SetActiveTool(VisualElement tab, HierarchyTool tool, SerializedProperty property)
        {
            activeToolTab = tab;
            
            activeToolGroup.text = tool.title;
            activeToolGroup.tooltip = tool.tooltip;
            activeToolGroup.style.opacity = 1;
            
            activeToolMarker.RemoveFromHierarchy();
            tab.Add(activeToolMarker);
            
            activeToolGUI.onGUIHandler = () => OnToolGUI(tool, property);
            
            activeToolGUI.style.display = DisplayStyle.Flex;
        }

        private void OnToolGUI(HierarchyTool tool, SerializedProperty property)
        {
            BeginChangeCheck();
            
            var rect = GUILayoutUtility.GetRect(200, 0);

            labelWidth /= 1.25f;
            //SerializedPropertyUtil.DrawSerializedObject(serialized, out var hasVisibleFields, rect);
            //activeToolEditor.OnInspectorGUI();
            SerializedPropertyUtil.DrawDefaultInspector(property.serializedObject, out bool hasVisibleFields);
            //SerializedPropertyUtil.DrawPropertyChildren(property, rect);
            labelWidth *= 1.25f;

            if (!hasVisibleFields)
            {
                using (new GUIColorScope(new Color(1, 1, 1, 0.5f)))
                    GUILayout.Label("This tool has no options.");
            }

            if (EditorGUI.EndChangeCheck())
            {
                tool.OnBeforeSave();
                ApplyPropertiesAndSave();
            }
        }

        private void OnLayoutGUI()
        {
            BeginChangeCheck();
            
            var layoutProp = serializedObject.FindProperty("layout");
            
            var rect = GUILayoutUtility.GetRect(200, 0);

            labelWidth /= 2f;
            SerializedPropertyUtil.DrawPropertyChildren(layoutProp, out _, rect);
            labelWidth *= 2f;

            if (EditorGUI.EndChangeCheck())
                ApplyPropertiesAndSave();
        }

        private void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
        }

        private static VisualElement Space(float height)
        {
            return new VisualElement { style = { height = height } };
        }

        private static VisualElement CreateLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("label");
            return label;
        }

        private static TabToggle CreateTabToggle(GUIContent content, SerializedProperty property)
        {
            var toggle = new TabToggle();
            
            toggle.BindProperty(property);
            toggle.AddToClassList("active-toggle");

            var image = new GUIImage(content.image);
            image.AddToClassList("active-toggle-icon");
            
            toggle.Add(image);
            
            return toggle;
        }
        
        private Foldout CreateFoldout(string text)
        {
            var foldout = new Foldout { text = text };
                
            //foldout.Add(CreateGroupBackground());
            
            foldout.Query(className: "unity-toggle").First().style.marginLeft = 0;
            foldout.contentContainer.style.marginLeft = 0;

            return foldout;
        }
        
        private Foldout CreateFoldoutSaveState(string text, bool expandedByDefault = false)
        {
            var foldout = CreateFoldout(text);
            
            if (!options.foldouts.TryGetValue(text, out var expanded))
                options.foldouts.Add(text, expanded = expandedByDefault);
            
            foldout.value = expanded;
            foldout.RegisterCallback<ChangeEvent<bool>>(evt => SaveOptions());

            return foldout;
        }
        
        private static VisualElement CreateGroupBackground()
        {
            var background = new VisualElement { style = { opacity = 0.08f, backgroundColor = new Color(1, 1, 1, 1) }};
           
            background.style.position = Position.Absolute;
            background.style.top = -1;
            background.style.left = -7;
            background.style.right = -7;
            background.style.bottom = -1;

            return background;
        }


        private void ApplyPropertiesAndSave()
        {
            serializedObject.ApplyModifiedProperties();
            SaveOptions();
        }
        private static void SaveOptions()
        {
            HierarchyOptions.Instance.Save();
        }
    }
}
