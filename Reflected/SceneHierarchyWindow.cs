using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    public class SceneHierarchyWindow
    {
        private static PropertyInfo sceneHierarchyProperty;
        
        private static MethodInfo frameObject;

        private object instance;
        internal EditorWindow actualWindow;
        internal SceneHierarchy hierarchy;

        private static void DoReflection()
        {
            var sceneHierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            sceneHierarchyProperty = sceneHierarchyWindowType.GetProperty("sceneHierarchy");
            
            frameObject = sceneHierarchyWindowType.GetMethod("FrameObject");
        }
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            DoReflection();
        }

        public SceneHierarchyWindow(EditorWindow instance)
        {
            this.instance = instance;
            actualWindow = instance;
            hierarchy = new SceneHierarchy(sceneHierarchyProperty.GetValue(actualWindow));
        }
        
        public void FrameObject(int instanceId)
        {
            frameObject.Invoke(instance, new object[] { instanceId, false });
        }
    }
}