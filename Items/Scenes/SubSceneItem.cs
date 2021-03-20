using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    [HierarchyItem("subSceneType")]
    internal class SubSceneItem : GameObjectItem
    {
        private static Type subSceneType = TypeCache.GetTypesDerivedFrom<MonoBehaviour>().FirstOrDefault(t => t.FullName == "Unity.Scenes.SubScene");
        private static Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;

        public SubSceneItem(GameObject instance) : base(instance)
        {
        }
/*
        protected override HierarchyItem CreateForInstance(Object instance)
        {
            if (!(instance is GameObject go))
                return null;

            var isSubScene = TreeViewGUI.IsSubSceneHeader(go);
            
            if (!isSubScene)
                return null;
            
            return new SubSceneItem(go);
        }*/
        
        protected override Texture2D GetEffectiveIcon()
        {
            return sceneIcon;
        }
    }
}
