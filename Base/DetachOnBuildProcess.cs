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
            var sceneRoots = scene.GetRootGameObjects();
            foreach (var root in sceneRoots)
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

                    var roots = new Transform[transform.childCount];
                    var siblings = new int[roots.Length];
                    var folderSibling = transform.GetSiblingIndex();

                    for (int i = 0; i < roots.Length; i++)
                    {
                        roots[i] = transform.GetChild(i);
                        siblings[i] = transform.GetSiblingIndex();
                    }

                    //transform.DetachChildren();
                    
                    for (int i = roots.Length - 1; i >= 0; i--)
                    {
                        roots[i].SetParent(null);
                        roots[i].SetSiblingIndex(siblings[i]);
                    }
                    
                    transform.SetSiblingIndex(folderSibling);
                }
            }
        }
    }
}
#endif