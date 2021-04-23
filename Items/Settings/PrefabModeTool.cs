

using UnityEngine;

namespace AV.Hierarchy
{
    public class PrefabModeTool : HierarchyTool
    {
        protected internal override string title => "Prefab Mode Button";
        protected internal override Texture2D icon => GetEditorIcon("tab_next");
    }
}