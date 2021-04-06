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
    internal class SubSceneItem : GameObjectItemBase
    {
        private static Type subSceneType = TypeCache.GetTypesDerivedFrom<MonoBehaviour>().FirstOrDefault(t => t.FullName == "Unity.Scenes.SubScene");
        private static Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;

        public SubSceneItem(GameObject gameObject) : base(gameObject)
        {
            if (!TreeViewGUI.IsSubSceneHeader(gameObject))
                CancelCreation();
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            args.icon = sceneIcon;
        }

        protected override Texture2D GetEffectiveIcon()
        {
            return sceneIcon;
        }
    }
}
