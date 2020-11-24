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
        internal EditorWindow window;
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

        public SceneHierarchyWindow(object instance)
        {
            this.instance = instance;
            window = instance as EditorWindow;
            hierarchy = new SceneHierarchy(sceneHierarchyProperty.GetValue(window));
        }
        
        public void FrameObject(int instanceId)
        {
            frameObject.Invoke(instance, new object[] { instanceId, false });
        }
    }
}