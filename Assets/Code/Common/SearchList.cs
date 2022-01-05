using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this code is useless - don't use it in anything new.

public static class SearchList<T>
{
	public static T FindFirst(List<T> set, Func<T, bool> check)
	{
		foreach (var t in set)
		{
			if (check(t)) return t;
		}
		return default(T);
	}
	public static T FindNext(List<T> set, Func<T, bool> check)
	{
		for (int i=0;i<set.Count; i++)
		{
			if (check(set[i])) 
			{
				if (i+1 >= set.Count) return default(T);
				return set[i+1];
			}
		}
		return default(T);
	}
	public static List<T> FindAll(List<T> set, Func<T, bool> check)
	{
		List<T> newSet = new List<T>();
		foreach (var t in set)
		{
			if (check(t))
			{
				newSet.Add(t);
			}
		}
		return newSet;
	}
	public static void Erase(List<T> set, Func<T, bool> check)
	{
		for (int i=(int)(set.Count)-1; i>=0; i--)
		{
			if (check(set[i]))
			{
				set.Remove(set[i]);
			}
		}
	}
	public static T FindRandom(List<T> set)
	{
		if (set == null || set.Count == 0) return default(T);
		return set[UnityEngine.Random.Range(0, set.Count-1)];
	}
	public static bool  Exists(List<T> set, Func<T, bool> check) { return FindFirst(set, check) != null; }

}


public static class SearchArray<T>
{
	public static T FindFirst(T[] set, Func<T, bool> check)
	{
		foreach (var t in set)
		{
			if (check(t)) return t;
		}
		return default(T);
	}
	public static T FindNext(T[] set, Func<T, bool> check)
	{
		for (int i=0;i<set.Length; i++)
		{
			if (check(set[i])) 
			{
				if (i+1 >= set.Length) return default(T);
				return set[i+1];
			}
		}
		return default(T);
	}
	public static List<T> FindAll(T[] set, Func<T, bool> check)
	{
		List<T> newSet = new List<T>();
		foreach (var t in set)
		{
			if (check(t))
			{
				newSet.Add(t);
			}
		}
		return newSet;
	}
	public static T FindRandom(T[] set)
	{
		if (set == null || set.Length == 0) return default(T);
		return set[UnityEngine.Random.Range(0, set.Length-1)];
	}
	public static bool  Exists(T[]  set, Func<T, bool> check) 
	{
		foreach (var t in set)
		{
			if (check(t)) return true;
		}
		return false;
	}
}