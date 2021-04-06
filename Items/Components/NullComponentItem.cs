using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    public class NullComponentItem : GameObjectItemBase
    {
        public override int order => -1000;

        private static Texture2D nullIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D;
        
        public NullComponentItem(GameObject go) : base(go)
        {
        }

        protected override GameObjectItemBase CreateForInstance(GameObject go, Component component)
        {
            if (component == null)
                return new NullComponentItem(go);
            return null;
        }
        
        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            args.overlayIcon = nullIcon;
        }

        public override void OnBeforeLabel(ref LabelGUIArgs args)
        {
            if (!isPrefab)
                args.color = new Color32(255, 213, 27, 255);
        }

        //protected override Texture2D GetEffectiveIcon()
        //{
        //    return nullIcon;
        //}
    }
}
