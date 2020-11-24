using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    public class ComponentData
    {
        public Component component;
        public readonly GUIContent content;
        
        public ComponentData(Component component)
        {
            this.component = component;
            content = new GUIContent(EditorGUIUtility.ObjectContent(component, component.GetType()).image);
        }
    }
}