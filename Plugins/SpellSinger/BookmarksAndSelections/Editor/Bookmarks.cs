using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bookmarks", menuName = "Bookmarks")]
public class Bookmarks : ScriptableObject
{
    [SerializeField]
    private List<Object> bookmarks = new();
        
    public List<Object> List
    {
        get => bookmarks;
        set => bookmarks = value;
    }
}