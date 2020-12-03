using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    public enum ColorTag
    {
        // https://chir.ag/projects/name-that-color/
        /// <summary>Silver, Emperor</summary>
        UI, 
        /// <summary>
        /// Burning Orange, Milano Red
        /// </summary>
        Navigation,
        /// <summary>
        /// Amber, Buddha Gold
        /// </summary>
        Audio,
        /// <summary>
        /// Green Yellow, Sea Green
        /// </summary>
        Environment,
        /// <summary>
        /// Aquamarine, Surfie Green
        /// </summary>
        Animations,
        /// <summary>
        /// Malibu, Denim
        /// </summary>
        Rendering,
        /// <summary>
        /// Portage, Royal Purple
        /// </summary>
        Sprites,
        /// <summary>Light Orchid, Cerise</summary> 
        Constraints, 
    }
    
    public static class ColorTags
    {
        private static Dictionary<ColorTag, (Color dark, Color light)> colorTable;

        public static Color GetColor(ColorTag tag)
        {
            if (colorTable == null)
            {
                colorTable = new Dictionary<ColorTag, (Color, Color)>
                {
                    // Hex is taken from built-in icons
                    {ColorTag.UI,          (Hex("#c3c3c3"), Hex("#555555"))}, 
                    {ColorTag.Constraints, (Hex("#e58cda"), Hex("#cb247e"))}, 
                    {ColorTag.Rendering,   (Hex("#80D6FD"), Hex("#0d6cca"))}, 
                    {ColorTag.Navigation,  (Hex("#fc6d40"), Hex("#b00d0d"))},
                    {ColorTag.Sprites,     (Hex("#ae90f2"), Hex("#673ab6"))},
                    {ColorTag.Animations,  (Hex("#7ffde4"), Hex("#107685"))},
                    {ColorTag.Audio,       (Hex("#FDC008"), Hex("#c89601"))},
                    {ColorTag.Environment, (Hex("#b1fd59"), Hex("#2f7d33"))},
                };
            }

            var (dark, light) = colorTable[tag];

            return isProSkin ? dark : light;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}