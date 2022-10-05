using System.Collections.Generic;
using UnityEngine;

namespace SharedTools
{
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
}