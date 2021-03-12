using UnityEngine;
using UnityEngine.Serialization;

namespace AV.Hierarchy
{
    internal class HierarchyPreferences : ScriptableObject
    {
        public bool enableSmartHierarchy = true;
        
        public StickyIcon effectiveIcon = StickyIcon.NotOnPrefabs;
        public TransformIcon transformIcon = TransformIcon.OnUniqueOrigin;
        public bool showGizmoIcon = true;

        public CopyPastePlace copyPastePlace;
        public AutoPasteAsChild autoPasteAsChild = AutoPasteAsChild.OnExpandedSelection;
        
        [FormerlySerializedAs("keepFoldersInPlaymode")] 
        public bool keepCollectionsInPlaymode;
        
        public bool enableHoverPreview;
        public ModificationKey previewKey;

        public bool preferLastComponent = true;
        public TypesPriority componentsPriority = new TypesPriority();
    }

    internal enum StickyIcon
    {
        Never,
        OnAnyObject,
        NotOnPrefabs
    }
    internal enum CopyPastePlace
    {
        [InspectorName("After Selection (Recommended)")]
        AfterSelection,
        BeforeSelection,
        [InspectorName("Last Sibling (Unity Default)")]
        LastSibling
    }

    internal enum AutoPasteAsChild
    {
        Never,
        Always,
        OnExpandedSelection
    }
    internal enum TransformIcon
    {
        Never,
        Always,
        OnUniqueOrigin,
        OnlyRectTransform
    }

    internal enum ModificationKey
    {
        Alt,
        Shift,
        Control,
    }
}