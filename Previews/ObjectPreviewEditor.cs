using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class ObjectPreviewEditor
    {
        private GameObjectInspector editor;
        
        private static Dictionary<GameObject, GameObjectInspector> cachedEditors = new Dictionary<GameObject, GameObjectInspector>();
        
        public ObjectPreviewEditor(GameObject target)
        {
            if (!cachedEditors.TryGetValue(target, out editor))
            {
                editor = new GameObjectInspector(target);
                cachedEditors.Add(target, editor);
            }
        }

        public void OnPreviewGUI(Rect rect)
        {
            editor.OnPreviewGUI(rect);
        }
    }
}