using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
#endif

public class AssetTracker<T> : ScriptableObject where T : ScriptableObject
{
	public T[]				items;
#if UNITY_EDITOR
	public static DATA[] GetAllInstances<DATA>() where DATA : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(DATA).Name);  //FindAssets uses tags check documentation for more info
        DATA[] a = new DATA[guids.Length];
        for(int i =0;i<guids.Length;i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<DATA>(path);
        }
        return a;
    }


	[Button]
	private void OnEnable()
	{
		items = GetAllInstances<T>();	
	}

#endif
	public T				GetItem(string itemname)
	{
		if (items == null) 
		{
			Debug.LogError("No items of " + typeof(T).ToString() + " detected!");
			return null;
		}
		foreach (var t in items)
		{
			if (string.Compare(itemname, t.name, System.StringComparison.OrdinalIgnoreCase) == 0) return t;
		}
		//Debug.LogError("No " + typeof(T).ToString() + " named " + itemname + " detected!");
		return null;
	}
}
