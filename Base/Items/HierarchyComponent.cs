using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    [DisallowMultipleComponent]
    internal class HierarchyComponent : MonoBehaviour
    {
        public virtual bool detachChildren => true;
    }
}
