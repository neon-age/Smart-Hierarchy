using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.AssetDatabase;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Collection))]
    internal class CollectionItem : GameObjectItemBase
    {
        public override int order => 0;

        private readonly Collection collection;
        
        private static readonly Texture2D collectionIcon = LoadAssetAtPath<Texture2D>(GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));
        
        
        public CollectionItem(GameObject gameObject) : base(gameObject)
        {
            collection = gameObject.GetComponent<Collection>();
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            args.icon = collectionIcon;
            
            if (!args.isOn)
                args.color *= ColorTags.GetColor(collection.colorTag);
        }

        public override bool OnIconClick(IconGUIArgs args)
        {
            var collectionPopup = ObjectPopupWindow.GetPopup<CollectionPopup>();
            if (collectionPopup == null)
            {
                var popup = new CollectionPopup(collection);
                    
                var scrollPos = SmartHierarchy.active.state.scrollPos.y;
                var position = new Vector2(args.rect.x, args.rect.yMax - scrollPos + 32);
                    
                popup.ShowInsideWindow(position, SmartHierarchy.active.root);
            }
            else
                collectionPopup.Close();

            return true;
        }

        protected override Texture2D GetEffectiveIcon()
        {
            return collectionIcon;
        }
    }
}
