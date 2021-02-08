using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    public enum ColorTag
    {
        // https://chir.ag/projects/name-that-color/
        /// <summary>Silver / Emperor. UI</summary>
        Gray, 
        /// <summary>
        /// Burning Orange / Milano Red. Navigation
        /// </summary>
        Red,
        /// <summary>
        /// Amber / Buddha Gold. Audio
        /// </summary>
        Orange,
        /// <summary>
        /// Green Yellow / Sea Green. Environment
        /// </summary>
        Green,
        /// <summary>
        /// Aquamarine / Surfie Green. Animation
        /// </summary>
        Aquamarine,
        /// <summary>
        /// Malibu / Denim. Rendering
        /// </summary>
        Blue,
        /// <summary>
        /// Portage / Royal Purple. Sprites
        /// </summary>
        Purple,
        /// <summary>Light Orchid / Cerise. Constraints</summary> 
        Pink, 
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
                    {ColorTag.Gray,       (Hex("#c3c3c3"), Hex("#555555"))}, 
                    {ColorTag.Pink,       (Hex("#e58cda"), Hex("#cb247e"))}, 
                    {ColorTag.Blue,       (Hex("#80D6FD"), Hex("#0d6cca"))}, 
                    {ColorTag.Red,        (Hex("#fc6d40"), Hex("#b00d0d"))},
                    {ColorTag.Purple,     (Hex("#ae90f2"), Hex("#673ab6"))},
                    {ColorTag.Aquamarine, (Hex("#7ffde4"), Hex("#107685"))},
                    {ColorTag.Orange,     (Hex("#FDC008"), Hex("#c89601"))},
                    {ColorTag.Green,      (Hex("#b1fd59"), Hex("#2f7d33"))},
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