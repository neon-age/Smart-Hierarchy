using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Hierarchy
{
    [CreateAssetMenu]
    internal class UIResources : ScriptableResource<UIResources>
    {
        public StyleSheet popupElementStyle;
        
        public Texture2D contextArrow;
        public Texture2D boxShadow;
    }
}
