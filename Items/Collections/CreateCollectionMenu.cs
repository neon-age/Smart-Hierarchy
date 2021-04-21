using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
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
            
            var selections = Selection.gameObjects;

            if (selections.Length == 1)
            {
                var selection = selections[0];
                var components = selection.GetComponents<Component>();
                var mainComponent = Components.ChooseMainComponent(components);

                var isPrefab = PrefabUtility.IsPartOfAnyPrefab(selection);
                var isCollection = mainComponent is Collection;
                
                var isDummy = mainComponent == null;
                var isPivot = mainComponent is Transform;
                
                var isValidTarget = (!isPrefab || !isCollection) && (isDummy || isPivot);

                if (isValidTarget)
                {
                    Undo.AddComponent<Collection>(selection);
                    isExecuted = true;
                    return;
                }
            }

            var collection = new GameObject("New Collection", typeof(Collection));
            Undo.RegisterCreatedObjectUndo(collection, "Create Collection");
            
            if (selections.Length > 0)
            {
                selections = selections
                    .OrderBy(x => x.transform.GetSiblingIndex()).Reverse().ToArray();
                
                var firstSelection = selections[selections.Length - 1];

                selections = selections
                    .Where(x => x.transform.parent == firstSelection.transform.parent).ToArray();
                
                var siblings = new int[selections.Length];
                var folderSibling = firstSelection.transform.GetSiblingIndex();

                Undo.SetTransformParent(collection.transform, firstSelection.transform.parent, "Create Collection");
            
                for (int i = 0; i < selections.Length; i++)
                {
                    Undo.SetTransformParent(selections[i].transform, collection.transform, "Create Collection");
                    selections[i].transform.SetSiblingIndex(siblings[i]);
                }

                collection.name = CollectionNaming.ChooseCollectionName(firstSelection);
                collection.transform.SetSiblingIndex(folderSibling);
                
                SmartHierarchy.active.window.FrameObject(firstSelection.GetInstanceID());
            }
            
            Selection.activeGameObject = collection;
            
            isExecuted = true;
        }
    }
}