using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    internal class Components
    {
        public readonly Component main;
        public readonly Texture2D icon;
        private readonly List<ComponentData> data;

        public ComponentData this[int index] => data[index];

        public Components(GameObject instance)
        {
            var components = instance.GetComponents<Component>();
            data = new List<ComponentData>(components.Length);

            foreach (var component in components)
            {
                // TODO: Show null component in hierarchy
                if (component)
                    data.Add(new ComponentData(component));
            }

            main = ChooseMainComponent(components);

            if (main)
            {
                icon = ObjectContent(main, main.GetType()).image as Texture2D;
            }
        }
        
        public static Component ChooseMainComponent(params Component[] components)
        {
            var length = components.Length;
            if (length == 0) 
                return null;

            var preferences = HierarchySettingsProvider.Preferences;
            
            var first = components[0];
            
            if (length == 1)
            {
                switch (preferences.transformIcon)
                {
                    case TransformIcon.Always: 
                        return components[0];
                    
                    case TransformIcon.OnUniqueOrigin:
                        if (first is Transform transform)
                        {
                            if (transform.localPosition != Vector3.zero || 
                                transform.localRotation != Quaternion.identity)
                                return components[0];
                        }
                        return first is RectTransform ? components[0] : null;
                        
                    case TransformIcon.OnlyRectTransform:
                        return first is RectTransform ? components[0] : null;
                }

                return null;
            }
            
            if (HasCanvasRenderer(components))
            {
                return GetMainUGUIComponent(components);
            }
            
            return components[1];
        }

        private static bool HasCanvasRenderer(params Component[] components)
        {
            return components.OfType<CanvasRenderer>().Any();
        }

        private static Component GetMainUGUIComponent(params Component[] components)
        {
            Component lastComponent = null;
            UIBehaviour firstUIBehaviour = null;

            foreach (var component in components)
            {
                if (component is Graphic graphic)
                    lastComponent = graphic;

                if (!firstUIBehaviour && component is UIBehaviour uiBehaviour)
                {
                    firstUIBehaviour = uiBehaviour;
                    lastComponent = uiBehaviour;
                }

                if (component is Selectable selectable)
                    lastComponent = selectable;
            }

            return lastComponent;
        }
    }
}