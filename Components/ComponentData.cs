using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    public class ComponentData
    {
        public Component component;
        public bool isHidden { get; }
        public readonly GUIContent content;
        
        public ComponentData(Component component)
        {
            this.component = component;
            content = new GUIContent(EditorGUIUtility.ObjectContent(component, component.GetType()).image);

            isHidden = component.hideFlags == HideFlags.HideInInspector;

            var fullName = component.GetType().FullName;
            
            // Components that should always be hidden
            // https://github.com/Unity-Technologies/UnityCsReference/blob/2020.1/Editor/Mono/Inspector/PropertyEditor.cs#L1680
            if (fullName == "UnityEngine.ParticleSystemRenderer" || 
                fullName == "UnityEngine.VFX.VFXRenderer")
                isHidden = true;
        }
    }
}