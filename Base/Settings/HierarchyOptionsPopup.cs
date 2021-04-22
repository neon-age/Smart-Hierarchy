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
        private static string UIPath = AssetDatabase.GUIDToAssetPath("f0d92e1f03926664991b2f7fbfbd6268") + "/";
        
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
        
        private Foldout activeToolGroup;
        private VisualElement activeToolMarker;
        private IMGUIContainer activeToolGUI;

        private readonly SerializedObject serializedObject;
        
        public HierarchyOptionsPopup()
        {
            activeToolMarker = new VisualElement { style =
            {
                backgroundColor = new Color(0.9f, 0.5f, 0.4f, 1),
                position = Position.Absolute,
                left = 0, right = 0,
                bottom = 0,
                height = 2
            }};
            
            var toggles = new Toolbar { style = { flexGrow = 0 } };

            var options = HierarchyOptions.Instance;
            serializedObject = new SerializedObject(options);
            
            this.Bind(serializedObject);
            
            var togglesLabel = CreateLabel("Tools Options");
            activeToolGroup = CreateActiveToolGroup();

            var toolsProp = serializedObject.FindProperty("tools").FindPropertyRelative("list");
            
            for (int i = 0; i < toolsProp.arraySize; i++)
            {
                var tool = options.tools[i];

                if (tool == null)
                    continue;
                
                var content = new GUIContent(tool.title, tool.icon, tool.tooltip);
                
                var property = toolsProp.GetArrayElementAtIndex(i);
                var enabledProp = property.FindPropertyRelative("enabled");

                var toggle = CreateToggle(content, enabledProp);
                toggles.Add(toggle);
                
                toggle.RegisterCallback<ClickEvent>(evt => SetActiveTool(toggle, tool, property));
                toggle.RegisterCallback<MouseDownEvent>(evt => SetActiveToolRightClick(toggle, tool, property));
            }
            
            // toggles.Add(CreateToggle(pickingIcon, "showPickingToggle"));
            // toggles.Add(CreateToggle(toggleIcon, "showActivationToggle"));
            // toggles.Add(CreateToggle(prefabModeIcon, "showPrefabModeToggle"));
            
            Add(togglesLabel);
            Add(toggles);
            Add(Space(5));
            Add(activeToolGroup);

            var layoutGUI = new IMGUIContainer(OnLayoutGUI);
            Add(Space(10));
            
            var layout = CreateGroupFoldout("Layout");
            layout.Add(layoutGUI);
            Add(layout);
            
            RegisterCallback<ChangeEvent<bool>>(evt => HierarchyOptions.Instance.Save());
            RegisterCallback<ChangeEvent<int>>(evt => HierarchyOptions.Instance.Save());
        }

        private Foldout CreateGroupFoldout(string text)
        {
            var foldout = new Foldout { text = text };
            //foldout.Add(CreateGroupBackground());
            
            foldout.Query(className: "unity-toggle").First().style.marginLeft = 0;
            foldout.contentContainer.style.marginLeft = 0;

            return foldout;
        }

        private Foldout CreateActiveToolGroup()
        {
            var foldout = CreateGroupFoldout("No Active Tool");
            foldout.value = false;
            foldout.SetEnabled(false);
            
            activeToolGUI = new IMGUIContainer();
            foldout.Add(activeToolGUI);

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

        private void SetActiveToolRightClick(VisualElement sender, HierarchyTool tool, SerializedProperty property)
        {
            activeToolGroup.value = true;
            SetActiveTool(sender, tool, property);
        }

        private void SetActiveTool(VisualElement sender, HierarchyTool tool, SerializedProperty property)
        {
            activeToolGroup.text = tool.title;
            activeToolGroup.SetEnabled(true);

            //var label = activeToolGroup.Query<Label>().First();
            //label.style.opacity = tool.enabled ? 1 : 0.5f;

            activeToolMarker.RemoveFromHierarchy();
            sender.Add(activeToolMarker);
            
            activeToolGUI.onGUIHandler = () => OnToolGUI(tool, property);
        }

        private void OnToolGUI(HierarchyTool tool, SerializedProperty property)
        {
            BeginChangeCheck();
            
            var rect = GUILayoutUtility.GetRect(200, 0);

            labelWidth /= 1.25f;
            SerializedPropertyUtil.DrawPropertyChildren(property, rect);
            //SerializedPropertyUtil.DrawPropertyChildren(property, rect);
            labelWidth *= 1.25f;

            if (EndChangeCheck())
                tool.OnValidate();
        }

        private void OnLayoutGUI()
        {
            BeginChangeCheck();
            
            var layoutProp = serializedObject.FindProperty("layout");
            
            var rect = GUILayoutUtility.GetRect(200, 0);

            labelWidth /= 2f;
            SerializedPropertyUtil.DrawPropertyChildren(layoutProp, rect);
            labelWidth *= 2f;

            EndChangeCheck();
        }

        private void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
        }
        private bool EndChangeCheck()
        {
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                HierarchyOptions.Instance.Save();
                return true;
            }
            return false;
        }

        private static VisualElement Space(float height)
        {
            return new VisualElement { style = { height = height} };
        }

        private static VisualElement CreateLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("label");
            return label;
        }

        private static ToolbarToggle CreateToggle(GUIContent content, SerializedProperty property)
        {
            var toggle = new ToolbarToggle { focusable = false };
            
            toggle.BindProperty(property);
            toggle.AddToClassList("active-toggle");

            var image = new GUIImage(content.image);
            image.AddToClassList("active-toggle-icon");
            
            toggle.Add(image);
            
            return toggle;
        }
    }
}
