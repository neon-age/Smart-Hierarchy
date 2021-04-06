using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    public class GameObjectItemBase : ObjectItemBase
    {
        public override int order => 100;

        private readonly GameObjectItem goRoot;
        
        public readonly GameObject gameObject;
        public readonly bool isPrefab;

        public bool renderDisabled => GameObjectItemUtil.GetColorCode(view) >= 4;
        public bool showPrefabModeButton => GameObjectItemUtil.GetShowPrefabModeButton(view);
        public bool isRootPrefab => goRoot.isRootPrefab;


        public GameObjectItemBase(Component component) : base(component)
        {
            this.gameObject = component.gameObject;
            goRoot = root as GameObjectItem;
            isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular;
        }
        
        public GameObjectItemBase(GameObject gameObject) : base(gameObject)
        {
            this.gameObject = gameObject;
            goRoot = root as GameObjectItem;
            isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular;
        }
        
        protected virtual GameObjectItemBase CreateForInstance(GameObject go, Component component) => null;
    }
}
