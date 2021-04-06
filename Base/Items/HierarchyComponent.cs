using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    //[DisallowMultipleComponent]
    public class HierarchyComponent : MonoBehaviour
    {
        public virtual bool detachChildrenOnStripping => true;
        public virtual bool destroyOnStripping => false;
    }
}
