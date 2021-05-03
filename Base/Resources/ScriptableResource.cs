
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class ScriptableResource<T> : ScriptableObject where T : ScriptableResource<T>
    {
        public static T Index => LoadIndexAsset();
        private static T index;
        
        private static MethodInfo GetFieldInfoFromPropertyPath;

        private bool fixedUIToolkitReferences;
        
        
        private static T LoadIndexAsset()
        {
            if (index == null)
                index = Resources.Load<T>($"Editor/{ObjectNames.NicifyVariableName(typeof(T).Name)}");
            
            index.FixUIToolkitReferences();

            return index;
        }
        
        private void FixUIToolkitReferences()
        {
            if (fixedUIToolkitReferences)
                return;
            
            var serializedObject = new SerializedObject(this);
            var iterator = serializedObject.GetIterator();
            
            while (iterator.NextVisible(true))
            {
                var path = iterator.propertyPath;
                
                if (path == "m_Script")
                    continue;

                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                // objectReferenceValue doesn't return null on (uninitialized?) UIE assets!
                var reference = iterator.objectReferenceValue;

                if (reference == null)
                    //Debug.Log($"{path} is null");
                    continue;

                var fieldInfo = GetFieldInfoFromProperty(typeof(T), iterator, out var fieldType);
                
                reference = (Object)fieldInfo.GetValue(this);

                // (reference is StyleSheet style && style == null) returns true on referenced uninitialized asset..
                if (!IsUIEAssetAndNull(reference))
                    continue;

                var assetPath = AssetDatabase.GetAssetPath(reference);
                var loadedAsset = AssetDatabase.LoadAssetAtPath(assetPath, fieldType);

                if (loadedAsset == null)
                    //Debug.Log($"{assetPath} {fieldType?.Name} loaded asset is null");
                    continue;
                
                fieldInfo.SetValue(this, loadedAsset);
                
                fixedUIToolkitReferences = true;
            }
        }

        private static bool IsUIEAssetAndNull(Object asset)
        {
            return asset is StyleSheet style && style == null ||
                   asset is VisualTreeAsset tree && tree == null;
        }

        private static FieldInfo GetFieldInfoFromProperty(Type host, SerializedProperty property, out Type fieldType)
        {
            if (GetFieldInfoFromPropertyPath == null)
            {
                var utilType = typeof(Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
                GetFieldInfoFromPropertyPath = utilType.GetMethod("GetFieldInfoFromPropertyPath", BindingFlags.NonPublic | BindingFlags.Static);
            }
            
            var args = new object[] { host, property.propertyPath, default(Type) };
            var fieldInfo = (FieldInfo)GetFieldInfoFromPropertyPath.Invoke(null, args);
            fieldType = (Type)args[2];
            
            return fieldInfo;
        }
    }
}