using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SharedTools
{
    public class BookmarksWindow : EditorWindow
    {
        private readonly string AddTip =
            "To add asset object to bookmarks right click on asset and choose 'Add to Bookmarks' in context menu";

        private Vector2 scrollPos;
        private Bookmarks so;
        private bool editMode;
        private bool loaded;

        private void OnEnable()
        {
            so = SoStorage.GetStorage<Bookmarks>();
        }

        private void OnGUI()
        {
            if (so == null)
            {
                return;
            }

            if (so.List.Count == 0 || editMode)
            {
                var textStyle = EditorStyles.label;
                textStyle.wordWrap = true;
                EditorGUILayout.LabelField(AddTip, textStyle);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var labelWidth = GUILayout.Width(20);
            var labelExpandWidth = GUILayout.ExpandWidth(false);
            var buttonWidth = GUILayout.Width(30);

            var list = so.List;
            for (var i = 0; i < list.Count; i++)
            {
                var bookmark = list[i];

                EditorGUILayout.BeginHorizontal();

                if (editMode)
                {
                    if (i == 0)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button("▲", buttonWidth))
                    {
                        list.RemoveAt(i);
                        list.Insert(i - 1, bookmark);
                        SaveSo();
                    }

                    if (i == 0)
                    {
                        GUI.enabled = true;
                    }

                    if (i == list.Count - 1)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button("▼", buttonWidth))
                    {
                        list.RemoveAt(i);
                        list.Insert(i + 1, bookmark);
                        SaveSo();
                    }

                    if (i == list.Count - 1)
                    {
                        GUI.enabled = true;
                    }

                    if (GUILayout.Button("X", buttonWidth))
                    {
                        list.RemoveAt(i);
                        SaveSo();
                    }
                }
                else
                {
                    GUILayout.Label(i.ToString(), labelWidth);
                }


                EditorGUILayout.ObjectField(bookmark, typeof(Object), true);


                if (!editMode)
                {
                    if (GUILayout.Button("▼", buttonWidth))
                    {
                        ScrollAssetsViewToTheBottom();
                        EditorGUIUtility.PingObject(bookmark);
                    }

                    if (Selection.activeObject == bookmark)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button("►", buttonWidth))
                    {
                        ScrollAssetsViewToTheBottom();
                        Selection.activeObject = bookmark;
                    }

                    if (Selection.activeObject == bookmark)
                    {
                        GUI.enabled = true;
                    }
                }


                EditorGUILayout.EndHorizontal();
            }


            if (list.Count > 0)
            {
                if (editMode)
                {
                    if (GUILayout.Button("Exit Edit Mode"))
                    {
                        editMode = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit"))
                    {
                        editMode = true;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void SaveSo()
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssetIfDirty(so);
        }

        private static void ScrollAssetsViewToTheBottom()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var windowType = assembly.GetType("UnityEditor.ProjectBrowser");
            var controllerType = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            var projectBrowser = GetWindow(windowType);
            var treeViewController =
                windowType.GetField("m_AssetTree", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(projectBrowser);
            var state = controllerType.GetProperty("state", BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(treeViewController) as TreeViewState;

            var contentRect = (Rect)controllerType.GetField("m_ContentRect",
                    BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(treeViewController);
            var visibleRect = (Rect)controllerType.GetField("m_VisibleRect",
                    BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(treeViewController);

            state!.scrollPos = new Vector2(state.scrollPos.x, contentRect.height - visibleRect.height);
        }

        [MenuItem("Tools/Bookmarks")]
        private static void ShowWindow()
        {
            var window = GetWindow<BookmarksWindow>();
            window.titleContent = new GUIContent("Bookmarks");
            window.Show();
        }

        [MenuItem("Assets/Add to Bookmarks", false, 19)]
        private static void AddToBookmarks(MenuCommand menuCommand)
        {
            var o = Selection.activeObject;
            if (o == null)
            {
                return;
            }

            var bookmarks = SoStorage.GetStorage<Bookmarks>();
            bookmarks.List.Add(o);
            EditorUtility.SetDirty(bookmarks);
            AssetDatabase.SaveAssetIfDirty(bookmarks);
        }
    }
}