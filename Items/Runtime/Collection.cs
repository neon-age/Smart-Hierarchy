using UnityEngine;

namespace AV.Hierarchy
{
    [AddComponentMenu("GameObject/Collection")]
    public class Collection : MonoBehaviour
    {
        [Tooltip("Will skip collection stripping during scene process.\n" +
                 "Use only when you know that transform overhead is doable.")]
        public bool keepTransformHierarchy;
        public ColorTag colorTag;
    }
}