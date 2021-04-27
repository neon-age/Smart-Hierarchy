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
                    if (activeToolTab == null)
                        activeToolGroup.text = "Right click to show options...";
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
            
            activeToolGUI = new IMGUIContainer();
            activeToolGroup.Add(activeToolGUI);
            activeToolGroup.Add(CreateSpace(5));
            
            ResetActiveToolGroup();
        }

        private void ResetActiveToolGroup()
        {
            activeToolTab = null;
            
            activeToolGroup.text = "No Selected Tool";
            activeToolGroup.tooltip = "";
            
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
