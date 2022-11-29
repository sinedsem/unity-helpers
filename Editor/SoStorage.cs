using UnityEditor;
using UnityEngine;

public static class SoStorage
{
    public static T GetStorage<T>() where T : ScriptableObject
    {
        var guids = AssetDatabase.FindAssets("t:" + typeof(T));
        if (guids.Length == 0)
        {
            var storage = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(storage, $"Assets/{typeof(T).Name}.asset");
            Debug.Log("Created new storage: " + AssetDatabase.GetAssetPath(storage));
            return storage;
        }

        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}