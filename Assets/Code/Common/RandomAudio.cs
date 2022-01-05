using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Sirenix.OdinInspector;

namespace ho
{
    [CreateAssetMenu(fileName = "RandomAudio", menuName = "HO/RandomAudio", order = 1)]
    [System.Serializable]
    public class RandomAudio : ScriptableObject
    {
	    public AudioClip[]							clips;
	    public bool									doNotRepeatCurrent;
	
	    public AudioClip							GetClip(AudioClip current)
	    {
		    if (clips == null || clips.Length == 0) return null;
		    if (clips.Length == 1) return clips[0];

		    int start = Random.Range(0, clips.Length);
		    for (int i=0; i<clips.Length; i++)
		    {
			    int idx = (start+i) % clips.Length;
			    if (doNotRepeatCurrent && clips[idx] == current) continue;
			    return clips[idx];
		    }
		    // all identical, likely. Just keep playing
		    return clips[start];
	    }

	    public bool IsValid { get { return clips != null && clips.Length > 0; } } 
    }
}
