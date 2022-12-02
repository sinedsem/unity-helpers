using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecentSelection
{
    public string Path { get; set; }
    public bool Scene { get; set; }
    public bool Pinned { get; set; }
}

public class RecentSelections : ScriptableObject
{
    [field: SerializeField] public List<RecentSelection> Values { get; set; }
}