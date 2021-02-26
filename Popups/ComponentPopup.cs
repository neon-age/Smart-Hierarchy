using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class ComponentPopup : ObjectPopupWindow, IDisposable
    {
        private static StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("948d877375c46ee479e97f6b60c07fc0"));
        private static Texture2D prefabOverlayAddedIcon = EditorGUIUtility.IconContent("PrefabOverlayAdded Icon").image as Texture2D;
        private static GUIStyle paneOptions;
        
        private static MethodInfo disableInternal = typeof(Editor).GetMethod("OnDisableINTERNAL", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo drawHeaderItems = typeof(EditorGUIUtility).GetMethod("DrawEditorHeaderItems", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo displayContextMenu = typeof(EditorUtility).GetMethod("DisplayObjectContextMenu", BindingFlags.NonPublic | BindingFlags.Static, null, 
                                                       new [] { typeof(Rect), typeof(Object[]), typeof(int) }, null);

        private static int TitlebarHash = "GenericTitlebar".GetHashCode();

        public Component target { get; }
        
        private Editor editor;


        ~ComponentPopup() => Dispose();

        public void Dispose()
        {
            disableInternal.Invoke(editor, null);
            Object.DestroyImmediate(editor);
            RemoveFromHierarchy();
        }

        public ComponentPopup(Component component)
        {
            var isDisconnected = PrefabUtility.IsDisconnectedFromPrefabAsset(component.gameObject);
            var isOverride = !isDisconnected && 
                             PrefabUtility.IsPartOfAnyPrefab(component) && 
                             PrefabUtility.GetCorrespondingObjectFromSource(component) == null;

            target = component;
            title.text = ObjectNames.GetInspectorTitle(component);
            
            styleSheets.Add(styleSheet);
            
            var icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image as Texture2D;

            editor = Editor.CreateEditorWithContext(new Object[] { component }, component);
            
            var image = new VisualElement { style = {
                backgroundImage = icon,
                width = 16, 
                height = 16,
                marginRight = 2
            }};
            titleContainer.Insert(0, image);
            
            if (isOverride)
            {
                var prefabOverlayAddedImage = new VisualElement { style = {
                    backgroundImage = prefabOverlayAddedIcon,
                    position = Position.Absolute,
                    width = 16, 
                    height = 16,
                    left = image.resolvedStyle.left + 8,
                    top = 4
                }};
                titleContainer.Add(prefabOverlayAddedImage);
            }

            var enabledProperty = editor.serializedObject.FindProperty("m_Enabled");

            if (enabledProperty != null)
            {
                var activationToggle = new Toggle { bindingPath = "m_Enabled" };
                activationToggle.Bind(editor.serializedObject);
                
                titleContainer.Insert(1, activationToggle);
            }
            
            var headerItemsGui = new IMGUIContainer(() => DrawEditorHeaderItems(target)) { name = "HeaderItems" };
            
            titleContainer.Add(headerItemsGui);
            //headerItemsGui.StretchToParentSize();
            
            
            var componentGui = new IMGUIContainer(DrawComponentIMGUI) { name = "ComponentInspector" };
            
            contentContainer.Add(componentGui);
        }

        private void DrawComponentIMGUI()
        {
            if (target == null)
                return;
        
            var marginLeft = contentContainer.resolvedStyle.marginLeft;

            GUILayout.BeginHorizontal();
            GUILayout.Space(Mathf.Abs(marginLeft));
            GUILayout.BeginVertical();
                
            editor.OnInspectorGUI();
                
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawEditorHeaderItems(params Object[] targets)
        {
            if (paneOptions == null)
                paneOptions = "PaneOptions";

            var evt = Event.current;
            
            GUILayout.BeginHorizontal();
            
            var titleRect = GUILayoutUtility.GetRect(16, 16);
            titleRect.x -= 20;
            
            var itemsRect = (Rect)drawHeaderItems.Invoke(null, new object[] { titleRect, targets, 4 });
            GUILayoutUtility.GetRect(itemsRect.width, 16);
            
            var optionsRect = new Rect(titleRect) { width = 16, x = titleRect.x + 20 };
            
            if (evt.type == EventType.MouseDown && optionsRect.Contains(evt.mousePosition))
            {
                displayContextMenu.Invoke(null, new object[] { optionsRect, targets, 0 });
                evt.Use();
            }

            if (evt.type == EventType.Repaint)
                paneOptions.Draw(optionsRect, GUIContent.none, TitlebarHash, false, optionsRect.Contains(evt.mousePosition));
                
            GUILayout.EndHorizontal();
        }
    }
}