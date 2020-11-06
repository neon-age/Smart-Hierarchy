#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class FolderNaming
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
        internal static string DecideFolderName(GameObject folder, GameObject firstSelection)
        {
            if (PrefabUtility.GetPrefabAssetType(firstSelection) == PrefabAssetType.Model)
                return "Environment";
            
            var components = firstSelection.GetComponents<Component>();

            if (components.Length > 1)
            {
                var mainComponent = SmartHierarchy.DecideMainComponent(components);

                var type = mainComponent.GetType();
                var declaringType = type.DeclaringType;
                
                Debug.Log(type.FullName);
                
                if (typeNamings.TryGetValue(type.FullName, out var name))
                    return folder.name = name; 
                
                if (declaringType != null && 
                    typeNamings.TryGetValue(declaringType.FullName, out name))
                    return folder.name = name;
            }
            
            return firstSelection.name
                .Replace("-", "");
        }
    }
}
#endif