using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlagSetInt
{
	[Serializable]
	public class FlagPair
	{
		public string name;
		public int value;
	};
	public List<FlagPair>	 set = new List<FlagPair>();

	public void		Sort()
	{
		set.Sort( (FlagPair a, FlagPair b) => b.value - a.value);
	}

	FlagPair GetFlag(string name)
	{
		name = name.Trim();
		foreach (var t in set)
		{
			if (string.Compare(t.name, name, System.StringComparison.OrdinalIgnoreCase) ==0) return t;
		}
		return null;
	}
	public int Get(string name, int defaultVal = default(int))
	{
		FlagPair t = GetFlag(name);
		if (t!=null) return t.value;
		return defaultVal;	
	}
	public void Set(string name, int value)
	{
		FlagPair t = GetFlag(name);
		if (t==null) 
		{
			t = new FlagPair()
			{
				name = name.Trim()
			};
			set.Add(t);
		}
		t.value = value;
	}
	public bool		Exists(string name) { FlagPair t = GetFlag(name); return t != null; }

	public void Clear() { set.Clear(); }
	public int Count { get { return set.Count; } }
	public bool Empty { get { return set.Count == 0; } }
}

[Serializable]
public class FlagSetBool
{
	[Serializable]
	public class FlagPair
	{
		public string name;
		public bool value;
	};
	public List<FlagPair>	 set = new List<FlagPair>();

	public void SortFlags()
	{
		set.Sort( (a, b) => string.Compare(a.name, b.name) );
	}
	FlagPair GetFlag(string name)
	{
		name = name.Trim();
		foreach (var t in set)
		{
			if (string.Compare(t.name, name, System.StringComparison.OrdinalIgnoreCase) == 0) return t;
		}
		return null;
	}
	public bool Get(string name, bool defaultVal = default(bool))
	{
		FlagPair t = GetFlag(name);
		if (t!=null) return t.value;
		return defaultVal;	
	}
	public void Set(string name, bool value)
	{
		FlagPair t = GetFlag(name);
		if (t==null) 
		{
			t = new FlagPair()
			{
				name = name.Trim()
			};
			set.Add(t);
		}
		t.value = value;
	}

	public void Delete(string name)
	{
		FlagPair exists = set.Find((FlagPair item) => StrReplace.Equals(name, item.name));
		if (exists != null)
		{
			set.Remove(exists);
		}
	}

	public bool		Exists(string name) { FlagPair t = GetFlag(name); return t != null; }

	public void Clear() { set.Clear(); }
	public int Count { get { return set.Count; } }
	public bool Empty { get { return set.Count == 0; } }
}


[Serializable]
public class FlagSetGUIDBool
{
	[Serializable]
	public class FlagPair
	{
		public SerializableGUID guid;
		public bool value;
	};
	public List<FlagPair>	 set = new List<FlagPair>();

	FlagPair GetFlag(SerializableGUID guid)
	{
		foreach (var t in set)
		{
			if (t.guid == guid) return t;
		}
		return null;
	}
	public bool Get(SerializableGUID guid, bool defaultVal = default(bool))
	{
		FlagPair t = GetFlag(guid);
		if (t!=null) return t.value;
		return defaultVal;	
	}
	public void Set(SerializableGUID _guid, bool value)
	{
		FlagPair t = GetFlag(_guid);
		if (t==null) 
		{
			t = new FlagPair()
			{
				guid = _guid
			};
			set.Add(t);
		}
		t.value = value;
	}

	public void Delete(SerializableGUID guid)
	{
		FlagPair exists = set.Find((FlagPair item) => guid == item.guid);
		if (exists != null)
		{
			set.Remove(exists);
		}
	}

	public bool		Exists(SerializableGUID guid) { FlagPair t = GetFlag(guid); return t != null; }

	public void Clear() { set.Clear(); }
	public int Count { get { return set.Count; } }
	public bool Empty { get { return set.Count == 0; } }

}


