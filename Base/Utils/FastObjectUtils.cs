

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal static class FastObjectUtils
    {
        private static Dictionary<Type, string> typesInspectorTitle = new Dictionary<Type, string>();
        private static Dictionary<Type, Texture2D> typesIcons = new Dictionary<Type, Texture2D>();
        private static Dictionary<Type, GUIContent> typesContent = new Dictionary<Type, GUIContent>();

        
        public static bool IsAlive(this Object obj)
        {
            return FastUtils.IsNativeObjectAlive(obj);
        }
        
        public static bool IsNull(this Object obj)
        {
            return !FastUtils.IsNativeObjectAlive(obj);
        }
        
        public static GUIContent GetObjectContent(Object obj)
        {
            var type = obj.GetType();

            if (!typesContent.TryGetValue(type, out var content))
            {
                content = new GUIContent
                {
                    text = GetInspectorTitle(obj),
                    image = GetObjectIcon(obj)
                };
                typesContent.Add(type, content);
            }

            return content;
        }
        
        public static string GetInspectorTitle(Object obj)
        {
            var type = obj.GetType();

            if (!typesInspectorTitle.TryGetValue(type, out var title))
            {
                title = ObjectNames.GetInspectorTitle(obj);
                typesInspectorTitle.Add(type, title);
            }

            return title;
        }
        
        public static Texture2D GetObjectIcon(Object obj)
        {
            if (obj.IsAlive())
                return AssetPreview.GetMiniThumbnail(obj);
            
            var type = obj.GetType();

            if (!typesIcons.TryGetValue(type, out var icon))
            {
                icon = AssetPreview.GetMiniTypeThumbnail(type);
                typesIcons.Add(type, icon);
            }

            return icon;
        }
    }
}