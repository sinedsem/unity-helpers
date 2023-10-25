using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SpellSinger.BookmarksAndSelections
{
    
	[Serializable]
    public class RecentSelection
    {
        [field: SerializeField] public string Path { get; set; }
        [field: SerializeField] public bool Scene { get; set; }
        [field: SerializeField] public bool Pinned { get; set; }
    }
    
    public class RecentSelectionsWindow : EditorWindow
    {
        private const int MaxHistory = 50;
    
        private Vector2 scrollPos;
    
        [SerializeField] private List<RecentSelection> values = new();
    
        public static RecentSelectionsWindow Instance { get; private set; }
    
        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            Selection.selectionChanged += HandleSelectionChange;
        }
    
        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
    
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
    
            AddToList(selection);
        }
    
        private void AddToList(RecentSelection selection)
        {
            var index = values.FindIndex(s => s.Scene == selection.Scene && s.Path == selection.Path);
            if (index >= 0)
            {
                selection = values[index];
                values.RemoveAt(index);
            }
    
            if (selection.Pinned)
            {
                values.Insert(0, selection);
            }
            else
            {
                values.Insert(values.TakeWhile(s => s.Pinned).Count(), selection);
            }
    
            var i = 0;
            while (values.Count > MaxHistory)
            {
                if (values[^(1 + i)].Pinned)
                {
                    i++;
                }
                else
                {
                    values.RemoveAt(values.Count - 1 - i);
                }
            }
    
            Repaint();
        }
    
    
        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            var labelWidth = GUILayout.Width(20);
            var buttonWidth = GUILayout.Width(30);
            for (var i = 0; i < values.Count; i++)
            {
                var recentSelection = values[i];
    
                var obj = recentSelection.Scene
                    ? GetGameObjectAtPath(recentSelection.Path)
                    : AssetDatabase.LoadAssetAtPath<Object>(recentSelection.Path);
                if (obj == null && !recentSelection.Pinned)
                    continue;
    
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), labelWidth);
    
                if (GUILayout.Button(recentSelection.Pinned ? "★" : ".", buttonWidth))
                {
                    recentSelection.Pinned = !recentSelection.Pinned;
                    AddToList(recentSelection);
                }
    
                if (obj != null)
                {
                    EditorGUILayout.ObjectField(obj, typeof(Object), true);
    
    
                    if (GUILayout.Button("▼", buttonWidth))
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
    
                    if (GUILayout.Button("►", buttonWidth))
                    {
                        Selection.activeObject = obj;
                    }
    
                    var dragHandleWidth = GUILayout.Width(20);
                    EditorGUILayout.LabelField(" ≡ ", dragHandleWidth);
                    var e = Event.current;
                    if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.type == EventType.MouseDrag)
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new[] { obj };
                        DragAndDrop.StartDrag("drag");
                        Event.current.Use();
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
    
            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabRoot = currentPrefabStage != null ? currentPrefabStage.prefabContentsRoot : null;
    
            while (obj.transform.parent != null)
            {
                if (prefabRoot && obj == prefabRoot)
                    break;
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
    
        private void OnCreateAsset(string assetPath)
        {
            AddToList(new RecentSelection
            {
                Path = assetPath
            });
        }
    
    
        [MenuItem("Tools/Recent Selections")]
        private static void ShowWindow()
        {
            var window = GetWindow<RecentSelectionsWindow>();
            window.titleContent = new GUIContent("Recent Selections");
            window.Show();
        }
    
        public class CustomAssetModificationProcessor : AssetModificationProcessor
        {
            static void OnWillCreateAsset(string assetPath)
            {
                Instance.OnCreateAsset(assetPath);
            }
    
            static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
            {
                Instance.OnCreateAsset(destinationPath);
                return AssetMoveResult.DidNotMove;
            }
        }
    }
}