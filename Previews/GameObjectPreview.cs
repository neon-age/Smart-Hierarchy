using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class GameObjectPreview : ObjectPreviewBase<GameObject>
    {
        private static Type gameObjectInspectorType;
        private static MethodInfo getPreviewDataMethod;
        private static FieldInfo renderUtilityField;

        private Color light0Color;
        private Color light1Color;
        private Color backgroundColor;
        private PreviewRenderUtility renderUtility;
        private Editor editor;
        
        private static HashSet<GameObject> renderableObjects = new HashSet<GameObject>();

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
            var previewDataType = gameObjectInspectorType.GetNestedType("PreviewData", BindingFlags.NonPublic);
            
            getPreviewDataMethod = gameObjectInspectorType.GetMethod("GetPreviewData", BindingFlags.NonPublic | BindingFlags.Instance);
            renderUtilityField = previewDataType.GetField("renderUtility", BindingFlags.Public | BindingFlags.Instance);

            EditorApplication.hierarchyChanged +=  renderableObjects.Clear;
        }

        public override void OnTargetChange()
        {
            Editor.CreateCachedEditor(Target, gameObjectInspectorType, ref editor);
        }

        public override bool HasPreview()
        {
            return HasRenderableParts(Target);
        }

        public override void RenderPreview()
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
            // Hide default preview texture, since we would draw it later with alpha blending
            GUI.color = new Color(1, 1, 1, 0);

            if (!IsCached)
            {
                editor.OnPreviewGUI(RenderArea, null);
                
                GUI.color = color;

                Output = renderUtility.camera.targetTexture;

                CachePreview(TargetID);
            }
        }
        
        public static bool HasRenderableParts(GameObject go)
        {
            if (renderableObjects.Contains(go))
                return true;

            var result = false;
            var renderers = go.GetComponentsInChildren<Renderer>();
            
            foreach (var renderer in renderers)
            {
                switch (renderer)
                {
                    case MeshRenderer _:
                        var filter = renderer.gameObject.GetComponent<MeshFilter>();
                        if (filter && filter.sharedMesh)
                            result = true;
                        break;
                    case SkinnedMeshRenderer skinnedMesh:
                        if (skinnedMesh.sharedMesh)
                            result = true;
                        break;
                    case SpriteRenderer sprite:
                        if (sprite.sprite)
                            result = true;
                        break;
                    case BillboardRenderer billboard:
                        if (billboard.billboard && billboard.sharedMaterial)
                            result = true;
                        break;
                }
            }

            if (result)
                renderableObjects.Add(go);

            return result;
        }
    }
}