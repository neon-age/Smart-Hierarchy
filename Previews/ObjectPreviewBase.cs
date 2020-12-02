using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public abstract class ObjectPreviewBase
    {
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
                    return;
                }

                if (value != target)
                    OnTargetChange();
                
                target = value;
            }
        }
        
        public Rect RenderArea { get; set; } = new Rect(0, 0, 64, 64);
        
        public RenderTexture Output { get; set; }
        
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            foreach (var previewType in TypeCache.GetTypesDerivedFrom<ObjectPreviewBase>())
            {
                if (previewType.ContainsGenericParameters)
                    return;
                
                var preview = Activator.CreateInstance(previewType) as ObjectPreviewBase;
                availablePreviews.Add(preview.GetTargetType(), preview);
            }
        }

        public static bool TryGetAvailablePreview(Type targetType, out ObjectPreviewBase preview)
        {
            return availablePreviews.TryGetValue(targetType, out preview);
        }
        
        public abstract void OnTargetChange();
        public abstract bool HasPreview();
        public abstract void RenderPreview();

        public abstract Type GetTargetType();
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