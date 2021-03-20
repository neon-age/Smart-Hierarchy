using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Header))]
    internal class HeaderItem : GameObjectItem
    {
        private static readonly Color normalColor = isProSkin ? new Color32(78, 78, 78, 255) : new Color32(79, 78, 78, 255);
        private static readonly Color hoverColor = isProSkin ? new Color32(89, 89, 89, 255) : new Color32(90, 89, 89, 255);
        private static readonly Color onColor = isProSkin ? new Color32(44, 93, 135, 255) : new Color32(45, 93, 135, 255);

        private static GUIStyle labelStyle;
        private static GUIContent labelContent = new GUIContent();
        
        private static Texture2D headerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("4751b09d2e04542479ec3838da599e0c"));
        
        
        public HeaderItem(GameObject instance) : base(instance)
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        }

        public override void OnItemGUI()
        {
            var fullWidthRect = new Rect(rect) { xMax = Screen.width };
            fullWidthRect.xMin = 32 + 12;
            fullWidthRect.yMax -= 1;

            var backgroundColor = isOn ? onColor : (isHover ? hoverColor : normalColor);
            
            EditorGUI.DrawRect(fullWidthRect, backgroundColor);
            
            var labelRect = new Rect(fullWidthRect);

            var content = ObjectContent(gameObject, typeof(GameObject));
            
            labelContent.text = null;
            labelContent.image = headerIcon;

            var labelWidth = labelStyle.CalcSize(labelContent).x;

            var contentColor = GUI.contentColor;
            GUI.contentColor = isProSkin ? new Color32(194, 194, 194, 255) : new Color32(195, 194, 194, 255);
            
            GUI.Box(labelRect, labelContent, labelStyle);
            
            GUI.contentColor = contentColor;

            labelRect.x += 32 - 12;
            
            labelContent.text = content.text;
            labelContent.image = null;
            GUI.Box(labelRect, labelContent, labelStyle);
        }
    }
}
