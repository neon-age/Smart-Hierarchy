#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// No namespace cause it removes default Folder icon
[AddComponentMenu("Other/Folder")]
internal class Folder : MonoBehaviour
{
    /* // WIP
    [MenuItem("GameObject/Create Folder", priority = 0)]
    public static void CreateFolder()
    {
        var folder = new GameObject("New Folder", typeof(Folder));
        Undo.RegisterCreatedObjectUndo(folder, "Create Folder");
        
        if (Selection.gameObjects.Length > 0)
        {
            Undo.SetTransformParent(folder.transform, Selection.gameObjects[0].transform.parent, "Move To Folder");
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Undo.SetTransformParent(Selection.gameObjects[i].transform, folder.transform, "Move To Folder");
            }
        }
        EditorGUIUtility.PingObject(Selection.gameObjects.Length > 0 ? Selection.gameObjects[0] : folder);
    }
    */
}
#endif