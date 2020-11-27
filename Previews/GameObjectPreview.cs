using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class GameObjectPreview
    {
        private static Type gameObjectInspectorType;
        private static MethodInfo getPreviewDataMethod;
        private static FieldInfo renderUtilityField;

        private Rect renderSize;
        private Color light0Color;
        private Color light1Color;
        private Color backgroundColor;
        private PreviewRenderUtility renderUtility;
        private Editor editor;

        public RenderTexture outputTexture;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
            var previewDataType = gameObjectInspectorType.GetNestedType("PreviewData", BindingFlags.NonPublic);
            
            getPreviewDataMethod = gameObjectInspectorType.GetMethod("GetPreviewData", BindingFlags.NonPublic | BindingFlags.Instance);
            renderUtilityField = previewDataType.GetField("renderUtility", BindingFlags.Public | BindingFlags.Instance);
        }

        public void CreateCachedEditor(Vector2 renderSize, GameObject target)
        {
            if (!editor || editor.target != target)
            {
                this.renderSize = new Rect(0, 0, renderSize.x, renderSize.y);
                Editor.CreateCachedEditor(target, gameObjectInspectorType, ref editor);
            }
        }

        public void OnPreviewGUI(Rect rect)
        {
            if (!editor)
                return;
            if (renderUtility == null || renderUtility.lights[0] == null)
            {
                var previewData = getPreviewDataMethod.Invoke(editor, null);
                renderUtility = renderUtilityField.GetValue(previewData) as PreviewRenderUtility;

                light0Color = renderUtility.lights[0].color;
                light1Color = renderUtility.lights[1].color;
                backgroundColor = renderUtility.camera.backgroundColor;
            }

            renderUtility.lights[0].color = light0Color * 2f;
            renderUtility.lights[1].color = light1Color * 8f;
            var backColor = renderUtility.camera.backgroundColor;
            renderUtility.camera.backgroundColor = new Color(backColor.r, backColor.g, backColor.b, 0);
            renderUtility.camera.clearFlags = CameraClearFlags.Depth;
            
            var color = GUI.color;
            // Hide default preview texture, because it is being drawn without alpha blending
            GUI.color = new Color(1, 1, 1, 0);
            
            editor.OnInteractivePreviewGUI(renderSize, null);
            
            GUI.color = color;

            var targetTexture = renderUtility.camera.targetTexture;

            if (targetTexture)
            {
                GUI.DrawTexture(rect, targetTexture, ScaleMode.ScaleToFit, true, 0);
                outputTexture = targetTexture;
            }

//
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