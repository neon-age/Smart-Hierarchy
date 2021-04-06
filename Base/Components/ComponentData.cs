using System;
using System.Collections.Generic;
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

            content = FastObjectUtils.GetObjectContent(component);
        }
    }
}