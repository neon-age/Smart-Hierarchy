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
        protected override string helpURL => "https://github.com/neon-age/Smart-Hierarchy/wiki";

        private Foldout activeToolGroup;
        private VisualElement activeToolTab;
        private VisualElement activeToolMarker = CreateActiveToggleMarker();
        private IMGUIContainer activeToolGUI;
        
        private VisualElement RMBHint;
        private TooltipElement tooltipElement = new TooltipElement();

        private readonly HierarchyOptions options;
        private readonly SerializedObject serializedObject;
        
        
        public HierarchyOptionsPopup()
        {
            options = HierarchyOptions.Instance;
            serializedObject = new SerializedObject(options);

            this.Bind(serializedObject);
            
            //var togglesLabel = CreateLabel("Tools Options");
            title = "Tools Options";
            
            RMBHint = new VisualElement { style = { flexDirection = FlexDirection.Row }};
            RMBHint.Add(new FlatIcon(UIResources.Index.RMBIcon) { style = { opacity = 0.8f, width = 16, height = 16 }});
            //RMBHint.Add(CreateLabel("Options"));

            var toolTabsBar = CreateTabsToolbar();
            CreateActiveToolGroup();
            
            var toolsProp = serializedObject.FindProperty("tools");
            
            //for (int d = 0; d < 3; d++)
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
                toolTabsBar.Add(tab);

                //var tooltip = $"{tool.title}";
                //
                //if (tool.tooltip != "")
                //    tooltip += $"{Environment.NewLine}{tool.tooltip}";

                //tab.tooltip = tool.title;
                //tooltipElement.SetTooltipFor(tab, tooltip);
                    
                
                tab.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    if (activeToolTab != null)
                        return;
                    
                    activeToolGroup.text = tool.title;
                    RMBHint.style.display = DisplayStyle.Flex;
                    //activeToolGroup.text = "Right click to show options...";
                });
                
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
            
            toolTabsBar.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (activeToolTab == null)
                    ResetActiveToolGroup();
            });
            
            //Add(togglesLabel);
            Add(toolTabsBar);
            Add(CreateSpace(5));
            Add(activeToolGroup);

            var layoutGUI = new IMGUIContainer(OnLayoutGUI);
            Add(CreateSpace(5));
            
            var layout = CreateFoldoutSaveState("Layout", true);
            layout.Add(layoutGUI);
            Add(layout);
            
            Add(new VisualElement { style = { width = 200, height = 1 } });
            
            RegisterCallback<ChangeEvent<bool>>(evt => SaveOptions());
            RegisterCallback<ChangeEvent<int>>(evt => SaveOptions());
        }

        protected override void OnAttach(AttachToPanelEvent evt)
        {
            root.Add(tooltipElement);
        }

        private void CreateActiveToolGroup()
        {
            activeToolGroup = CreateFoldout("Active Tool");
            
            var foldout = activeToolGroup.Query(className: "unity-toggle__input").First();
            var label = foldout.Query<Label>().First();

            label.style.flexGrow = 1;

            foldout.Add(RMBHint);
            
            activeToolGUI = new IMGUIContainer();
            activeToolGroup.Add(activeToolGUI);
            
            ResetActiveToolGroup();
        }

        private void ResetActiveToolGroup()
        {
            activeToolTab = null;
            
            activeToolGroup.text = "No Selected Tool";
            activeToolGroup.tooltip = "";
            
            activeToolGroup.value = false;
            activeToolGroup.style.opacity = 0.5f;
            
            RMBHint.style.display = DisplayStyle.None;
            
            activeToolGroup.Clear();
            activeToolMarker.RemoveFromHierarchy();
        }

        private void SetActiveToolAndExpand(VisualElement sender, HierarchyTool tool, SerializedProperty property)
        {
            SetActiveTool(sender, tool, property);
            activeToolGroup.value = true;
        }

        private void SetActiveTool(VisualElement tab, HierarchyTool tool, SerializedProperty property)
        {
            ResetActiveToolGroup();
            
            activeToolTab = tab;
            activeToolGUI.onGUIHandler = () => OnToolGUI(tool, property);
            
            activeToolGroup.text = tool.title;
            activeToolGroup.tooltip = tool.tooltip;
            activeToolGroup.style.opacity = 1;

            if (!string.IsNullOrEmpty(tool.commentary))
            {
                activeToolGroup.Add(CreateMiniHelpBox(tool.commentary));
            }
            activeToolGroup.Add(activeToolGUI);
            activeToolGroup.Add(CreateSpace(5));
            
            tab.Add(activeToolMarker);
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
        
        private Foldout CreateFoldoutSaveState(string text, bool expandedByDefault = false)
        {
            var foldout = CreateFoldout(text);
            
            if (!options.foldouts.TryGetValue(text, out var expanded))
                options.foldouts.Add(text, expanded = expandedByDefault);
            
            foldout.value = expanded;
            foldout.RegisterCallback<ChangeEvent<bool>>(evt => SaveOptions());

            return foldout;
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
