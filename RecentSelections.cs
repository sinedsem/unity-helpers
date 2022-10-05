using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RecentSelections : EditorWindow
{
    private const int MaxHistory = 15;

    private Vector2 scrollPos;
    private List<Object> recentSelections;


    private void OnEnable()
    {
        recentSelections ??= new List<Object>();
        Selection.selectionChanged += HandleSelectionChange;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= HandleSelectionChange;
    }

    private void HandleSelectionChange()
    {
        var activeObject = Selection.activeObject;

        if (recentSelections.Contains(activeObject))
        {
            recentSelections.Remove(activeObject);
        }

        recentSelections.Insert(0, activeObject);
        if (recentSelections.Count > MaxHistory)
        {
            recentSelections.RemoveRange(MaxHistory, recentSelections.Count - MaxHistory);
        }

        Repaint();
    }


    private void OnGUI()
    {
        recentSelections.RemoveAll(o => o == null);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        var labelWidth = GUILayout.Width(20);
        var buttonWidth = GUILayout.Width(30);
        for (var i = 0; i < recentSelections.Count; i++)
        {
            var recentSelection = recentSelections[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString(), labelWidth);
            EditorGUILayout.ObjectField(recentSelection, typeof(Object), true);

            var e = Event.current;
            if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.type == EventType.MouseDrag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { recentSelection };
                DragAndDrop.StartDrag("drag");
                Event.current.Use();
            }

            if (GUILayout.Button("▼", buttonWidth))
            {
                EditorGUIUtility.PingObject(recentSelection);
            }

            if (GUILayout.Button("►", buttonWidth))
            {
                Selection.activeObject = recentSelection;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    [MenuItem("Tools/Recent Selections")]
    private static void ShowWindow()
    {
        var window = GetWindow<RecentSelections>();
        window.titleContent = new GUIContent("Recent Selections");
        window.Show();
    }
}