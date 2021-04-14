using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    internal class HierarchyFilterOptionsPopup : ObjectPopupWindow
    {
        private class ActiveToggle : ToolbarToggle
        {
            public override bool canGrabFocus => false;
        }
        
        private static GUIContent visibilityIcon = IconContent("animationvisibilitytoggleon");
        private static GUIContent pickingIcon = IconContent("scenepicking_pickable_hover");
        private static GUIContent toggleIcon = IconContent("Toggle Icon");
        private static GUIContent prefabModeIcon = IconContent("tab_next");

        private SerializedObject serializedObject;
        
        public HierarchyFilterOptionsPopup()
        {
            var toggles = new Toolbar();

            var options = HierarchyFilterOptions.instance;
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
        }

        private ToolbarToggle CreateToggle(GUIContent content, string bindingPath)
        {
            var property = serializedObject.FindProperty(bindingPath);
            
            var toggle = new ActiveToggle();
            toggle.BindProperty(property);
            toggle.AddToClassList("active-toggle");

            IMGUIContainer iconGui = null;
            iconGui = new IMGUIContainer(() =>
            {
                var style = iconGui.resolvedStyle;
                GUI.color = new Color(1, 1, 1, style.opacity);
                ViewItemGUI.DrawIconTexture(new Rect(0, 0, 16, 16), content.image, Color.white, true);
            });
            iconGui.AddToClassList("active-toggle-icon");
            
            toggle.Add(iconGui);
            
            return toggle;
        }
    }
}
