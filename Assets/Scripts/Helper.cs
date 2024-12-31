using System.Collections.Generic;
using UnityEngine;

namespace Reconnect
{
    public static class Helper
    {
        public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component
        {
            if(parent == null) { throw new System.ArgumentNullException(); }
            if(string.IsNullOrEmpty(tag)) { throw new System.ArgumentNullException(nameof(tag)); }
            List<T> list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
            if(list.Count == 0) { return null; }

            for(int i = list.Count - 1; i >= 0; i--) 
            {
                if (list[i].CompareTag(tag) == false)
                {
                    list.RemoveAt(i);
                }
            }
            return list.ToArray();
        }
    }
}