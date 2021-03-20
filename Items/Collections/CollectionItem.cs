using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.AssetDatabase;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Collection))]
    internal class CollectionItem : GameObjectItem
    {
        private readonly Collection collection;
        
        private static readonly Texture2D collectionIcon = LoadAssetAtPath<Texture2D>(GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));
        
        
        public CollectionItem(GameObject instance) : base(instance)
        {
            collection = instance.GetComponent<Collection>();
        }

        public override void OnItemGUI()
        {
            if(!isOn) 
                tintColor *= ColorTags.GetColor(collection.colorTag);
            
            base.OnItemGUI();
            
            if (OnIconClick(rect))
            {
                var collectionPopup = ObjectPopupWindow.GetPopup<CollectionPopup>();
                if (collectionPopup == null)
                {
                    var popup = new CollectionPopup(collection);
                    
                    var scrollPos = SmartHierarchy.active.state.scrollPos.y;
                    var position = new Vector2(rect.x, rect.yMax - scrollPos + 32);
                    
                    popup.ShowInsideWindow(position, SmartHierarchy.active.root);
                }
                else
                    collectionPopup.Close();
            }
        }

        protected override Texture2D GetEffectiveIcon()
        {
            return collectionIcon;
        }
    }
}
