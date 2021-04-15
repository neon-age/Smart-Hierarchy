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
        
        private static GUIContent visibilityIcon = IconContent("animationvisibilitytoggleon");
        private static GUIContent pickingIcon = IconContent("scenepicking_pickable_hover");
        private static GUIContent toggleIcon = IconContent("Toggle Icon");
        private static GUIContent prefabModeIcon = IconContent("tab_next");

        private SerializedObject serializedObject;
        
        public HierarchyOptionsPopup()
        {
            var toggles = new Toolbar();

            var options = HierarchyOptions.instance;
            serializedObject = new SerializedObject(options);
            
            this.Bind(serializedObject);
            
            var togglesLabel = new Label("Toggles");
            togglesLabel.AddToClassList("label");
            
            toggles.Add(CreateToggle(visibilityIcon, "showVisibilityToggle"));
            toggles.Add(CreateToggle(pickingIcon, "showPickingToggle"));
            toggles.Add(CreateToggle(toggleIcon, "showActivationToggle"));
            toggles.Add(CreateToggle(prefabModeIcon, "showPrefabModeToggle"));
            
            Add(togglesLabel);
            Add(toggles);
            
            RegisterCallback<ChangeEvent<bool>>(evt => HierarchyOptions.instance.Save());
        }

        private ToolbarToggle CreateToggle(GUIContent content, string bindingPath)
        {
            var property = serializedObject.FindProperty(bindingPath);
            
            var toggle = new ToolbarToggle() { focusable = false };
            toggle.BindProperty(property);
            toggle.AddToClassList("active-toggle");

            var image = new GUIImage(content.image);
            image.AddToClassList("active-toggle-icon");
            
            toggle.Add(image);
            
            return toggle;
        }
    }
}
