using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public class HierarchyTool : ScriptableObject
    {
        [SerializeField] 
        [HideInInspector]
        internal bool enabled;

        protected static HierarchyOptions options => HierarchyOptions.Instance;
        
        protected internal virtual int order => 100;
        protected internal virtual string title => ObjectNames.NicifyVariableName(GetType().Name);
        protected internal virtual string tooltip => "";
        protected internal virtual string commentary => "";
        protected internal virtual Texture2D icon => GetEditorIcon("SceneViewTools");

        public virtual void OnBeforeSave() {}
        

        protected static T LoadAssetFromGUID<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        protected static Texture2D GetEditorIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image as Texture2D;
        }
    }
}
