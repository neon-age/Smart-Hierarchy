using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AV.Hierarchy
{
    internal class SceneItem : ViewItemBase
    {
        private static MethodInfo getSceneByHandleInfo = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.NonPublic | BindingFlags.Static);
        private static Func<int, Scene> getSceneHandle = Delegate.CreateDelegate(typeof(Func<int, Scene>), getSceneByHandleInfo) as Func<int, Scene>;
        
        private static Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
        
        public SceneItem(int id) : base(id) {}
        
        
        protected override HierarchyItemBase CreateForInstance(int id)
        {
            var scene = getSceneHandle.Invoke(id);

            if (!scene.IsValid())
                return null;
            
            return new SceneItem(id);
        }

        protected override Texture2D GetEffectiveIcon()
        {
            return sceneIcon;
        }
    }
}
