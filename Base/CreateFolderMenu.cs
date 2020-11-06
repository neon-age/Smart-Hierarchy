#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class CreateFolderMenu
    {
        private static bool hasCreatedFolder;

        [MenuItem("GameObject/Create Folder", priority = 0)]
        public static void CreateFolder()
        {
            if (hasCreatedFolder)
                return;
        
            EditorApplication.delayCall += () => hasCreatedFolder = false;
        
            var folder = new GameObject("New Folder", typeof(Folder));
            Undo.RegisterCreatedObjectUndo(folder, "Create Folder");

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

                Undo.SetTransformParent(folder.transform, firstSelection.transform.parent, "Create Folder");
            
                for (int i = 0; i < selections.Length; i++)
                {
                    Undo.SetTransformParent(selections[i].transform, folder.transform, "Create Folder");
                    selections[i].transform.SetSiblingIndex(siblings[i]);
                }

                folder.name = FolderNaming.DecideFolderName(folder, firstSelection);
                folder.transform.SetSiblingIndex(folderSibling);

                SmartHierarchy.Reflected.FrameObject(firstSelection.GetInstanceID());
                Selection.activeGameObject = folder;
            }
            hasCreatedFolder = true;
        }
    }
}

#endif