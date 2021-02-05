using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class CollectionNaming
    {
        // TODO: Expose namings in preferences
        private static Dictionary<string, string> typeNamings = new Dictionary<string, string>()
        {
            { "UnityEngine.Canvas", "UI" },
            { "UnityEngine.UI.Graphic", "UI" },
            { "UnityEngine.UI.Selectable", "UI" },
            
            { "UnityEngine.Rendering.Volume", "Post FX" },
            { "UnityEngine.Terrain", "Environment" },
            { "UnityEngine.Camera", "Camera Work" },
            { "UnityEngine.Light", "Lighting" },
            
            { "UnityEngine.MonoBehaviour", "Gameplay" },
            { "UnityEngine.AI.NavMeshAgent", "AI" },
            
            { "UnityEngine.Rigidbody", "Physics" },
            { "UnityEngine.Rigidbody2D", "Physics" },
            
            { "UnityEngine.Collider", "Surface" },
            { "UnityEngine.Collider2D", "Surface" }
        };
        
        // TODO: Smarter naming
        internal static string ChooseCollectionName(GameObject firstSelection)
        {
            if (PrefabUtility.GetPrefabAssetType(firstSelection) == PrefabAssetType.Model)
                return "Environment";
            
            var components = firstSelection.GetComponents<Component>();
            
            if (components.Length > 1)
            {
                var mainComponent = Components.ChooseMainComponent(components);

                if (TryGetNamingByComponent(mainComponent, out var naming))
                    return naming;

                for (int i = components.Length - 1; i >= 0; i--)
                {
                    if (TryGetNamingByComponent(components[i], out naming))
                        return naming;
                }
            }
            
            return firstSelection.name
                .Replace("-", "");
        }

        private static bool TryGetNamingByComponent(Component component, out string naming)
        {
            if (component == null)
            {
                naming = "";
                return false;
            }

            var type = component.GetType();
            var baseType = type.BaseType;
                
            if (typeNamings.TryGetValue(type.FullName, out naming))
                return true; 
                
            if (baseType != null && 
                typeNamings.TryGetValue(baseType.FullName, out naming))
                return true;

            return false;
        }
    }
}