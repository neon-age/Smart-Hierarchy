using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class CreateCollectionMenu
    {
        private static bool isExecuted;

        [MenuItem("GameObject/Create Collection", priority = 0)]
        public static void CreateCollection()
        {
            if (isExecuted)
                return;
        
            EditorApplication.delayCall += () => isExecuted = false;
        
            var folder = new GameObject("New Collection", typeof(Collection));
            Undo.RegisterCreatedObjectUndo(folder, "Create Collection");

            var selections = Selection.gameObjects;

            if (selections.Length > 0)
            {
                selections = selections
                    .OrderBy(x => x.transform.GetSiblingIndex()).Reverse().ToArray();
                
                var firstSelection = selections[selections.Length - 1];

                selections = selections
                    .Where(x => x.transform.parent == firstSelection.transform.parent).ToArray();
                
                var siblings = new int[selections.Length];
                var folderSibling = firstSelection.transform.GetSiblingIndex();

                Undo.SetTransformParent(folder.transform, firstSelection.transform.parent, "Create Collection");
            
                for (int i = 0; i < selections.Length; i++)
                {
                    Undo.SetTransformParent(selections[i].transform, folder.transform, "Create Collection");
                    selections[i].transform.SetSiblingIndex(siblings[i]);
                }

                folder.name = CollectionNaming.ChooseCollectionName(firstSelection);
                folder.transform.SetSiblingIndex(folderSibling);

                SmartHierarchy.lastHierarchy.window.FrameObject(firstSelection.GetInstanceID());
                Selection.activeGameObject = folder;
            }
            isExecuted = true;
        }
    }
}