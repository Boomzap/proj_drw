using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace ho
{
    [CreateAssetMenu(fileName = "AudioTracker", menuName = "HO/Trackers/AudioTracker", order = 1)]
    public class AudioTracker : ScriptableObject
    {
	    public RandomAudio[]		randomClips;

	    #if UNITY_EDITOR
	    [Button]
	    private void OnEnable()
	    {		
		    randomClips = AssetTracker<RandomAudio>.GetAllInstances<RandomAudio>();	

		    // find the clips
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(AudioClip).Name); 
        
	
		    List<Clip> newClips = new List<Clip>();
            for(int i =0;i<guids.Length;i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			    AudioClip entry = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
			
			    if (entry == null) continue;
			    Clip previous = null;
			    if (allClips != null)
			    {
				    previous = SearchArray<Clip>.FindFirst(allClips, (Clip t) => (t.clip == null) ? false : StrReplace.Equals(t.clip.name, entry.name));
			    }

			    Clip newClip = new Clip
			    {
				    clip = entry,
				    gain = (previous!=null) ? previous.gain : 1.0f
			    };
			    newClips.Add(newClip);
            }
		    allClips = newClips.ToArray();
	    }
    #endif

	    [System.Serializable]
	    public class Clip
	    {
		    public AudioClip	clip;

		    [TableColumnWidth(50, Resizable = false)]
		    [Range(0, 1.5f)]
		    public float		gain = 1.0f;
	    }

	    [TableList(ShowIndexLabels = true)]
	    public Clip[]			allClips;


	    public RandomAudio		GetRandom(string findEntry)
	    {
		    return SearchArray<RandomAudio>.FindFirst(randomClips, (RandomAudio t) => StrReplace.Equals(t.name, findEntry));
	    }

	    public float			GetGain(AudioClip clip)
	    {
			if (clip == null)
				return 0f;

		    Clip entry = SearchArray<Clip>.FindFirst(allClips, (Clip t) => StrReplace.Equals(t.clip.name, clip.name));
		    if (entry!=null) return entry.gain;
		    return 1.0f;
	    }

	    public AudioClip		GetClip(string findClip, out float gain)
	    {
		    Clip entry = SearchArray<Clip>.FindFirst(allClips, (Clip t) => StrReplace.Equals(t.clip.name, findClip));

		    if (entry!=null)
		    {
			    gain = entry.gain;
			    return entry.clip;
		    }
		    gain = 1.0f;
		    return null;
	    }
    }

}