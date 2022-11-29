using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecentSelectionsWindow : EditorWindow
{
    private const int MaxHistory = 15;

    private Vector2 scrollPos;
    private RecentSelections storage;

    private void OnEnable()
    {
        storage = SoStorage.GetStorage<RecentSelections>();
        Selection.selectionChanged += HandleSelectionChange;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= HandleSelectionChange;
    }

    private void HandleSelectionChange()
    {
        var activeObject = Selection.activeObject;
        if (activeObject == null) return;

        var selection = new RecentSelection();
        if (AssetDatabase.Contains(activeObject))
        {
            selection.Scene = false;
            selection.Path = AssetDatabase.GetAssetPath(activeObject);
        }
        else
        {
            if (activeObject is GameObject gameObject)
            {
                selection.Scene = true;
                selection.Path = GetGameObjectPath(gameObject);
            }
            else
            {
                return;
            }
        }

        var index = storage.Values.FindIndex(s => s.Scene == selection.Scene && s.Path == selection.Path);
        if (index >= 0)
        {
            selection = storage.Values[index];
            storage.Values.RemoveAt(index);
        }

        storage.Values.Insert(0, selection);

        var i = 0;
        while (storage.Values.Count > MaxHistory)
        {
            if (storage.Values[^(1 + i)].Pinned)
            {
                i++;
            }
            else
            {
                storage.Values.RemoveAt(storage.Values.Count - 1 - i);
            }
        }

        Repaint();
    }


    private void OnGUI()
    {
        storage.Values.RemoveAll(o => o == null);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        var labelWidth = GUILayout.Width(20);
        var buttonWidth = GUILayout.Width(30);
        for (var i = 0; i < storage.Values.Count; i++)
        {
            var recentSelection = storage.Values[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString(), labelWidth);

            if (GUILayout.Button(recentSelection.Pinned ? "★" : "♠", buttonWidth))
            {
                recentSelection.Pinned = !recentSelection.Pinned;
            }


            Object obj;
            if (recentSelection.Scene)
            {
                obj = GetGameObjectAtPath(recentSelection.Path);
            }
            else
            {
                obj = AssetDatabase.LoadAssetAtPath<Object>(recentSelection.Path);
            }

            if (obj != null)
            {
                EditorGUILayout.ObjectField(obj, typeof(Object), true);

                var e = Event.current;
                if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.type == EventType.MouseDrag)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { obj };
                    DragAndDrop.StartDrag("drag");
                    Event.current.Use();
                }

                if (GUILayout.Button("▼", buttonWidth))
                {
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button("►", buttonWidth))
                {
                    Selection.activeObject = obj;
                }
            }
            else
            {
                EditorGUILayout.TextField(recentSelection.Path);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    public string GetGameObjectPath(GameObject obj)
    {
        var path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }

        return path;
    }

    public GameObject GetGameObjectAtPath(string path)
    {
        var names = path.Split("/");

        var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();

        var rootGameObjects = currentPrefabStage != null
            ? new[] { currentPrefabStage.prefabContentsRoot }
            : SceneManager.GetActiveScene().GetRootGameObjects();

        var root = rootGameObjects.FirstOrDefault(o => o.name == names[0]);
        if (root == null)
            return null;

        var obj = root;

        for (var i = 1; i < names.Length; i++)
        {
            var found = false;
            for (var j = 0; j < obj.transform.childCount; j++)
            {
                var child = obj.transform.GetChild(j);
                if (child.name == names[i])
                {
                    obj = child.gameObject;
                    found = true;
                    break;
                }
            }

            if (!found)
                return null;
        }

        return obj;
    }


    [MenuItem("Tools/Recent Selections")]
    private static void ShowWindow()
    {
        var window = GetWindow<RecentSelectionsWindow>();
        window.titleContent = new GUIContent("Recent Selections");
        window.Show();
    }
}