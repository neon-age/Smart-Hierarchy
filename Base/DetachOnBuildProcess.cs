#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AV.Hierarchy
{
    internal class DetachOnBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder { get; }
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var isEditor = Application.isEditor;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                foreach (var folder in root.GetComponentsInChildren<Folder>(true))
                {
                    var transform = folder.transform;
                    
                    if (!isEditor)
                    {
                        transform.DetachChildren();
                        Object.DestroyImmediate(folder);
                        continue;
                    }

                    var children = new Transform[transform.childCount];
                    var siblings = new int[children.Length];
                    var folderSibling = transform.GetSiblingIndex();

                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = transform.GetChild(i);
                        siblings[i] = transform.GetSiblingIndex();
                    }

                    //transform.DetachChildren();
                    
                    for (int i = siblings.Length - 1; i >= 0; i--)
                    {
                        children[i].parent = null;
                        children[i].SetSiblingIndex(siblings[i]);
                    }
                    
                    transform.SetSiblingIndex(folderSibling);
                }
            }
        }
    }
}
#endif