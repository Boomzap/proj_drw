using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Boomzap.Conversation
{
    public static class ClipboardParser
    {
        public class ClipboardData
        {
            public string text;
            public string narrator;
            public string emotion;
        }

        public static List<ClipboardData> FromClipboard()
        {
            string clipboardData = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboardData))
            {
                Debug.LogWarning("Can't generate from clipboard; no data");
                return new List<ClipboardData>();
            }

            return Parse(clipboardData);
        }

        public static List<ClipboardData> Parse(string clipboardData)
        {
            clipboardData = clipboardData.Replace("\r\n", "\n");		// trim out the double Carriage return + new Line 
			clipboardData = clipboardData.Replace("\u201C", "\"");
			clipboardData = clipboardData.Replace("\u201D", "\"");
			List<string> lines = clipboardData.Split('\n').ToList();
			string emotion = "";
			string currentNarrator = "";

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines.Last()))
            {
                lines.RemoveAt(lines.Count-1);
            }

			List<ClipboardData> output = new List<ClipboardData>();
			foreach (var t in lines) 
			{

				string txt = t.Trim();	// leading or trailing spaces
				if (txt.Length <= 0) continue;
				if (txt.Length >= 2 && txt[0] == '/' && txt[1] == '/') continue; // leading comment


				if (txt[0] == '(') // looks like a character emote
				{
					int length = txt.IndexOf(')');
					if (length > 0)
					{
						emotion = txt.Substring(1, length-1);
						txt = txt.Substring(length+1, txt.Length-(length+1)).Trim();
					} else
					{
						Debug.LogError("Invalid emotion sequence in line: " + txt);
					}
				}
				if (txt[0] == '[')
				{
					// this looks like a character name
					int length = txt.IndexOf(']');
					if (length > 0)
					{
						string characterName = txt.Substring(1, length-1);
						txt = txt.Substring(length+1, txt.Length-(length+1)).Trim();

						currentNarrator = characterName;
					} else
					{
						Debug.LogError("Invalid control sequence in line: " + txt);
					}
				}
				if (txt[0] == '(') // ugly hack, but we *know* they're never going to remember that emotions come before character names
				{
					int length = txt.IndexOf(')');
					if (length > 0)
					{
						emotion = txt.Substring(1, length-1);
						txt = txt.Substring(length+1, txt.Length-(length+1)).Trim();
					} else
					{
						Debug.LogError("Invalid emotion sequence in line: " + txt);
					}
				}
				ClipboardData entry = new ClipboardData();
				entry.text = txt;
				entry.narrator = currentNarrator;
				entry.emotion = emotion;
				output.Add(entry);
			}
			return output;            
        }
    }
}
