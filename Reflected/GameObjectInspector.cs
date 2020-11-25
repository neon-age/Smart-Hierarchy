using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class GameObjectInspector
    {
        private static Type gameObjectInspectorType;
        private static MethodInfo getPreviewDataMethod;
        private static FieldInfo renderUtilityField;

        private static Texture2D cachedPreview;

        private Color light0Color;
        private Color light1Color;
        private Color backgroundColor;
        private PreviewRenderUtility renderUtility;
        private Editor editor;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
            var previewDataType = gameObjectInspectorType.GetNestedType("PreviewData", BindingFlags.NonPublic);
            
            getPreviewDataMethod = gameObjectInspectorType.GetMethod("GetPreviewData", BindingFlags.NonPublic | BindingFlags.Instance);
            renderUtilityField = previewDataType.GetField("renderUtility", BindingFlags.Public | BindingFlags.Instance);
        }

        public GameObjectInspector(GameObject target)
        {
            editor = Editor.CreateEditor(target, gameObjectInspectorType);
        }

        public void OnPreviewGUI(Rect rect)
        {
            if (renderUtility == null)
            {
                var previewData = getPreviewDataMethod.Invoke(editor, null);
                renderUtility = renderUtilityField.GetValue(previewData) as PreviewRenderUtility;

                light0Color = renderUtility.lights[0].color;
                light1Color = renderUtility.lights[1].color;
                backgroundColor = renderUtility.camera.backgroundColor;
            }

            renderUtility.lights[0].color = light0Color * 1.5f;
            renderUtility.lights[1].color = light1Color * 3.4f;
            renderUtility.camera.backgroundColor = backgroundColor * 4;

            //var color = GUI.color;
            // Hide default preview texture, because it is being drawn without alpha blending
            //GUI.color = new Color(1, 1, 1, 0);
            
            editor.OnPreviewGUI(rect, null);

            //GUI.color = color;
            
            //EditorGUI.DrawTextureAlpha(rect, renderUtility.camera.targetTexture, ScaleMode.ScaleToFit);
            
            /*
            var id = editor.target.GetInstanceID();
            
            var assetPreview = AssetPreview.GetAssetPreview(editor.target);
            var isLoading = AssetPreview.IsLoadingAssetPreview(id);
            
            if (isLoading)
            {
                if (cachedPreview != null)
                    EditorGUI.DrawTextureTransparent(rect, cachedPreview);
            }
            else if (assetPreview != null)
            {
                EditorGUI.DrawTextureTransparent(rect, assetPreview);
                cachedPreview = assetPreview;
            }
*/
            //EditorGUI.DrawTextureAlpha(rect, assetPreview, ScaleMode.ScaleToFit, 0, GetHashCode());
        }
    }
}