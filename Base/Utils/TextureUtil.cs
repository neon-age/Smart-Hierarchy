

using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    // https://github.com/Unity-Technologies/UnityCsReference/blob/2020.2/Editor/Mono/AssetPipeline/TextureUtil.bindings.cs
    [InitializeOnLoad]
    internal static class TextureUtil
    {
        private static Func<Texture, int> getGPUWidth;
        private static Func<Texture, int> getGPUHeight;
        
        static TextureUtil()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil");

            var getGPUWidthInfo = type.GetMethod("GetGPUWidth", BindingFlags.Public | BindingFlags.Static);
            var getGPUHeightInfo = type.GetMethod("GetGPUHeight", BindingFlags.Public | BindingFlags.Static);

            var textureParam = Expression.Parameter(typeof(Texture));
            
            getGPUWidth = Expression.Lambda<Func<Texture, int>>(Expression.Call(getGPUWidthInfo, textureParam), textureParam).Compile();
            getGPUHeight = Expression.Lambda<Func<Texture, int>>(Expression.Call(getGPUHeightInfo, textureParam), textureParam).Compile();
        }

        public static int GetGPUWidth(Texture texture) => getGPUWidth(texture);
        public static int GetGPUHeight(Texture texture) => getGPUHeight(texture);
    }
}