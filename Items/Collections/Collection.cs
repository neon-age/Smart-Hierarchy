using UnityEngine;

namespace AV.Hierarchy
{
    [AddComponentMenu("GameObject/Collection")]
    internal class Collection : HierarchyComponent
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        public override bool detachChildrenOnStripping => !skipStripping;
        public override bool destroyOnStripping => false;
        
        public bool skipStripping => keepTransformHierarchy || (prefs.keepCollectionsInPlaymode && Application.isEditor);
        
        [Tooltip("Will skip collection stripping during scene process.\n" +
                 "Use only when you know that transform overhead is doable.")]
        public bool keepTransformHierarchy;
        public ColorTag colorTag;
    }
}