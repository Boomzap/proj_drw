using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ho
{
[Serializable]
public class Flags 
{
	public FlagSetBool		flagStates = new FlagSetBool();

	public bool HasFlag(string flagName)							{ return (flagName.Length == 0) || flagStates.Get(flagName); }
	public void SetFlag(string flagName, bool value)				{ flagStates.Set(flagName, value); }
	public void ClearFlags()										{ flagStates.Clear(); }

	public void DeleteFlags(string flagSet)
	{
		if (string.IsNullOrWhiteSpace(flagSet)) return;
		string[] set = StrReplace.Tokenize(flagSet);
		if (set == null || set.Length == 0) return;
		foreach (var t in set) DeleteFlag(t);
	}
	public void AddFlags(string flagSet)
	{
		if (string.IsNullOrWhiteSpace(flagSet)) return;
		string[] set = StrReplace.Tokenize(flagSet);
		if (set == null || set.Length == 0) return;
		foreach (var t in set) SetFlag(t, true);
	}

	public void ClearFlagsMatching(string matchStr)
	{
		string match = StrReplace.CleanStr(matchStr);
		for (int i=(int)(flagStates.set.Count-1); i>=0; i--)
		{
			var t = flagStates.set[i];
			if (StrReplace.CleanStr(t.name).Contains(match))
			{
				flagStates.set.Remove(t);
			}
		}
	}

	void ClearTempFlags()
	{
		for (int i=(int)(flagStates.set.Count-1); i>=0; i--)
		{
			var t = flagStates.set[i];
			if (t.name.Length > 1 && t.name[0] =='_')
			{
				flagStates.set.Remove(t);
			}
		}
	}

	public void DeleteFlag(string name)
	{
		for (int i=(int)(flagStates.set.Count-1); i>=0; i--)
		{
			if (string.Compare(flagStates.set[i].name.ToString(), name, System.StringComparison.OrdinalIgnoreCase) == 0)	// remove in_office, in_bedroom, etc
			{
				flagStates.set.Remove(flagStates.set[i]);
			}
		}
	}

	public void RefreshInventory(Savegame save)
	{
		for (int i=(int)(flagStates.set.Count-1); i>=0; i--)
		{
			var t = flagStates.set[i];
			string id = StrReplace.CleanStr(t.name);
			if (id.StartsWith("has_"))	// remove all has_xxx flags first
			{
				flagStates.set.Remove(t);
			}
		}
		// then set all the ones that exist
		//foreach (var t in save.inventory.items)
		//{
		//	if (t.count > 0)
		//	{
		//		flagStates.Set("has_"+t.name, true);
		//	}
		//}
	}

	public void UpdateLocation(string CurrentLocation)
	{
		for (int i=(int)(flagStates.set.Count-1); i>=0; i--)
		{
			var t = flagStates.set[i];
			string id = StrReplace.CleanStr(t.name);
			if (id.StartsWith("in_"))	// remove in_office, in_bedroom, etc
			{
				flagStates.set.Remove(t);
			}
		}
		SetFlag("in_" + StrReplace.CleanStr(CurrentLocation), true);
		SetFlag("visited_"+CurrentLocation, true);
	}

	//public void UpdateDateFlags(TimeOfDay currentTime, DayOfWeek currentDay, int currentDayIdx, bool clearTmp)
	//{
	//	for (DayOfWeek t=DayOfWeek.Monday; t<=DayOfWeek.Sunday; t++) 
	//	{
	//		if (t == currentDay)
	//			SetFlag(t.ToString(), true);
	//		else
	//			DeleteFlag(t.ToString());
	//	}

	//	for (int i=0; i<400; i++)
	//	{
	//		if (HasFlag("DAY"+i))
	//		{
	//			DeleteFlag("DAY"+i);
	//		}
	//	}
	//	SetFlag("DAY"+currentDayIdx, true);
	//	UpdateTimeFlags(currentTime);
	//	if (clearTmp)
	//	{
	//		ClearTempFlags();
	//	}
	//}
	//public void UpdateTimeFlags(TimeOfDay currentTime)
	//{
	//	for (TimeOfDay t=TimeOfDay.Morning; t<TimeOfDay.Count; t++) 
	//	{
	//		if (t == currentTime)
	//		{
	//			SetFlag(t.ToString(), true);
	//		} else
	//		{
	//			DeleteFlag(t.ToString());
	//		}
	//	}
	//}

	public void SetFlags(string flagSet, bool value)
	{
		if (flagSet == null) return; 
		string[] flags = StrReplace.Tokenize(flagSet);
		foreach (var t in flags) SetFlag(t, value);
	}
	public bool IncludeAllFlags(string flagSet)
	{
		if (flagSet == null) return true; 
		if (flagSet.Length == 0) return true;

		string[] flags = StrReplace.Tokenize(flagSet);
		foreach (var t in flags) 
		{
			if (!HasFlag(t)) return false;
		}
		return true;
	}
	public bool ExcludeAnyFlags(string flagSet)
	{
		if (flagSet == null) return false; 
		if (flagSet.Length == 0) return false;

		string[] flags = StrReplace.Tokenize(flagSet);
		foreach (var t in flags) 
		{
			if (HasFlag(t)) return true;
		}
		return false;
	}

	public string FailedRequiredFlagsStr(string flagSet)
	{
		if (flagSet == null) return "Invalid Flag set"; 
		if (flagSet.Length == 0) return "No flags set";

		string[] flags = StrReplace.Tokenize(flagSet);
		foreach (var t in flags) 
		{
			if (!HasFlag(t)) return "Missing flag " + t;
		}
		return "Valid";
	}
	public string FailedExcludeFlagsStr(string flagSet)
	{
		if (flagSet == null) return "Invalid Flag set"; 
		if (flagSet.Length == 0) return "No flags set";

		string[] flags = StrReplace.Tokenize(flagSet);
		foreach (var t in flags) 
		{
			if (HasFlag(t)) return "Present flag " + t;
		}
		return "Valid";
	}
}
}