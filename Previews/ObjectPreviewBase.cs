using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public abstract class ObjectPreviewBase
    {
        private static Dictionary<int, RenderTexture> previewCache = new Dictionary<int, RenderTexture>();
        private static Dictionary<Type, ObjectPreviewBase> availablePreviews = new Dictionary<Type, ObjectPreviewBase>();
        
        private Object target;
        public Object Target
        {
            get => target;
            set
            {
                if (value.GetType() != GetTargetType())
                {
                    target = null;
                    OnTargetChange();
                }

                if (value != target)
                {
                    target = value;
                    OnTargetChange();
                }
            }
        }
        public int TargetID => target ? target.GetInstanceID() : 0;

        public bool IsCached => previewCache.ContainsKey(TargetID);

        public bool IgnoreCaching { get; set; } = false;
        
        public Rect RenderArea { get; set; } = new Rect(0, 0, 64, 64);
        
        public RenderTexture Output { get; set; }
        
        public abstract void OnTargetChange();
        public abstract bool HasPreview();
        public abstract void RenderPreview();
        public abstract Type GetTargetType();
        
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            Cleanup();
            EditorApplication.hierarchyChanged += Cleanup;
            
            foreach (var previewType in TypeCache.GetTypesDerivedFrom<ObjectPreviewBase>())
            {
                if (previewType.ContainsGenericParameters)
                    return;
                
                var preview = Activator.CreateInstance(previewType) as ObjectPreviewBase;
                availablePreviews.Add(preview.GetTargetType(), preview);
            }
        }
        
        private static void Cleanup()
        {
            if (previewCache.Count < 100)
                return;
            
            foreach (var texture in previewCache.Values)
                Object.DestroyImmediate(texture);
            
            previewCache.Clear();
        }

        public static bool TryGetAvailablePreview(Type targetType, out ObjectPreviewBase preview)
        {
            return availablePreviews.TryGetValue(targetType, out preview);
        }
        
        protected void CachePreview(int instanceId)
        {
            if (!Output || instanceId == 0)
                return;

            var copy = new RenderTexture(Output);
            var previous = RenderTexture.active;
            
            Graphics.Blit(Output, copy);
            RenderTexture.active = previous;
            
            previewCache.Add(instanceId, copy);
        }

        public RenderTexture GetCachedPreview(int instanceId)
        {
            previewCache.TryGetValue(instanceId, out var preview);
            return preview;
        }
    }
    
    public abstract class ObjectPreviewBase<TTarget> : ObjectPreviewBase where TTarget : Object
    {
        protected new TTarget Target
        {
            get => base.Target as TTarget;
            set => base.Target = value;
        }

        public override Type GetTargetType()
        {
            return typeof(TTarget);
        }
    }
}