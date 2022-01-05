using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class TokenPair
{
	public TokenPair() { key = ""; value = 0; }
	public string key;
	public int value;
}

public class StrReplace
{
	class ReplaceChunk
	{
		public string	key;		// i.e. <NAME>
		public string	data;		// i.e. Peter
		public Func<string>	call;
	};
	Dictionary<string, ReplaceChunk>	chunks = new Dictionary<string, ReplaceChunk>(System.StringComparer.OrdinalIgnoreCase);

	static StrReplace	instance = new StrReplace();

	public string ParseStr(string input)
	{
		string output = input;
        if (input == null) return output;

        foreach (var entry in chunks)
		{
			output = output.Replace(entry.Value.key, (entry.Value.call != null) ? entry.Value.call() : entry.Value.data);
		}

		return output;
	}


	public void AddStrChunk(string key, string data, Func<string> call = null)
	{	
		key = "<" + key.ToUpperInvariant() + ">";
		ReplaceChunk	res;
		if (chunks.TryGetValue(key, out res))
		{
			// already exists, just replace
			res.data = data;
			res.call = call;
			return;
		}
		ReplaceChunk newChunk = new ReplaceChunk();
		newChunk.key = key;
		newChunk.data = data;
		newChunk.call = call;
		chunks.Add(key, newChunk);
	}

	public static string Parse(string input)
	{
		return instance.ParseStr(input);
	}

	public static string CleanStr(string input)
	{
		return input.Trim().ToLowerInvariant();
	}

	public static void AddChunk(string key, string data, Func<string> call = null)
	{	
		instance.AddStrChunk(key, data, call);
	}

	public static bool Exists(string key)
	{
		return instance.chunks.ContainsKey(key);
	}

	public static bool Equals(string a, string b)
	{
		return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase)==0;
	}

	public static string[] Tokenize(string input, char onchar = ',')
	{
		if (string.IsNullOrWhiteSpace(input)) return new string[0];
		string[] set = input.Split(onchar);
		for (int i=0; i<set.Length; i++)
		{
			set[i] = set[i].Trim();
		} 
		return set;
	}
	public static TokenPair[] TokenizePair(string input, char onchar = ',')
	{
		string[] set = input.Split(onchar);
		TokenPair[] output = new TokenPair[set.Length];
		for (int i=0; i<set.Length; i++)
		{
			output[i] = new TokenPair();
			char[] sep = {'[', ']'};
			string[] data = set[i].Split(sep);

			if (data == null) continue;
			if (data.Length == 1)
			{
				output[i].key = StrReplace.CleanStr(data[0]);
				output[i].value = 1;
			} else
			if (data.Length >= 2)
			{
				output[i].key = StrReplace.CleanStr(data[0]);
				output[i].value =Int32.Parse(data[1]);
			} 
		} 
		return output;
	}
	public static void	ParseValue(string input, out string name, out int value)
	{
		string[] set = Tokenize(input, ',');
		if (set.Length>=1) name = set[0]; else name = "";
		if (set.Length>=2) value = Int32.Parse(set[1]); else value = 0;
	}
	public static void	ParseValue(string input, out string name, out string value)
	{
		string[] set = Tokenize(input, ',');
		if (set.Length>=1) name = set[0]; else name = "";
		if (set.Length>=2) value = set[1]; else value = "";
	}
	public static string MakeProper(string input)
	{
		if (string.IsNullOrEmpty(input)) return "";
		return char.ToUpper(input[0]) + input.Substring(1);

	}
}

