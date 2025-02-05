using System;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false)
        where T : Component
    {
        if (parent == null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));
        var list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
        if (list.Count == 0) return null;

        for (var i = list.Count - 1; i >= 0; i--)
            if (list[i].CompareTag(tag) == false)
                list.RemoveAt(i);

        return list.ToArray();
    }

    public static T[] FindComponentsInChildrenWithName<T>(this GameObject parent, string name, bool forceActive = false)
        where T : Component
    {
        if (parent == null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        var list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
        if (list.Count == 0) return null;

        for (var i = list.Count - 1; i >= 0; i--)
            if (list[i].name != name)
                list.RemoveAt(i);

        return list.ToArray();
    }

    public static GameObject GetPrefabByName(string prefabName)
    {
        var prefab = Resources.Load<GameObject>(prefabName);
        return prefab;
    }
}