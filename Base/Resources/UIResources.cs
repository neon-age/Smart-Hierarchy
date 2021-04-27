using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Hierarchy
{
    [CreateAssetMenu]
    internal class UIResources : ScriptableResource<UIResources>
    {
        [Header("UXML")] 
        public VisualTreeAsset preferencesUxml;
        
        [Header("Style Sheets")]
        public StyleSheet popupElementStyle;
        public StyleSheet preferencesStyle;
        public StyleSheet helpBoxStyle;
        public StyleSheet foldoutHeaderStyle;
        public StyleSheet foldoutHeaderDarkStyle;
        
        [Header("Icons")]
        public Texture2D collectionIcon;
        public Texture2D filterIcon;
        public Texture2D foldoutIcon;
        public Texture2D contextArrow;
        public Texture2D boxShadow;
    }
}
