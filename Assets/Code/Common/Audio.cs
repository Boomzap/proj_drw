using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Sirenix.OdinInspector;

namespace ho
{

    public class Audio : SimpleSingleton<Audio>
    {
	    [System.Serializable]
	    public class MusicChannel
	    {
		    public AudioSource		primarySrc;	
		    public AudioSource		secondarySrc;	

		    public bool				restartLocalAudio = false;
		    public float			crossFadeDuration = 0.5f;
		    //AudioClip				currentClip = null;
		    RandomAudio				randomAudio = null;
		    float					currentGain = 1.0f;
		    float					xFadeTime, totalxFadeTime;

		    public void Stop()
		    {
			    primarySrc.clip = null;
			    primarySrc.Stop();

			    secondarySrc.clip = null;
			    secondarySrc.Stop();

			    randomAudio = null;
		    }
		    public void Update()
		    {
			    if (xFadeTime > 0)
			    {
				    xFadeTime-=Time.deltaTime;
				    float t = xFadeTime/totalxFadeTime;
				    if (t < 0) t = 0;			
				    primarySrc.volume = Mathf.Lerp(currentGain, 0, t) ;
				    secondarySrc.volume = Mathf.Lerp(0, currentGain, t) ;
				    if (xFadeTime < 0)
				    {
					    secondarySrc.Stop();
				    }
			    }

			    if (!primarySrc.isPlaying)
			    {
				    if (restartLocalAudio)
				    {
					    restartLocalAudio = false;
					    //World.instance.CurrentLocation.PlayMusic();
				    }
				    if (randomAudio != null)
				    {
					    SetMusic(randomAudio.GetClip(primarySrc.clip), 1.0f);
				    }
			    }
		    }

		    public void				SetMusic(AudioClip clip, float gain, float overrideCrossFade = -1, bool loop = false)
		    {
			    if (restartLocalAudio) return; // we're in the middle of a sting or something. Once it's over we'll revert
			    if (clip == null) 
			    {
				    // fade out instead?
				    primarySrc.clip = null;
				    secondarySrc.clip = null;
				    return;
			    }

			    if (clip == primarySrc.clip) return; // already playing
			    if (!primarySrc.isPlaying)
			    {
				    primarySrc.clip = clip;
				    primarySrc.Play();
				    primarySrc.volume = 0.0f;
				    primarySrc.loop = loop;
				    totalxFadeTime = xFadeTime = overrideCrossFade > 0 ? overrideCrossFade : crossFadeDuration;
				    return;				
			    }


			    currentGain = gain;
			    AudioSource tmp = primarySrc;	
			    primarySrc = secondarySrc;
			    secondarySrc = tmp;	// swap around
			    secondarySrc.volume = gain;
			    secondarySrc.loop = false;
		
			    primarySrc.clip = clip;
			    primarySrc.Play();
			    primarySrc.volume = 0.0f;
			    primarySrc.loop = loop;
			    totalxFadeTime = xFadeTime = overrideCrossFade > 0 ? overrideCrossFade : crossFadeDuration;
		    }
		    public void				SetRandomMusic(RandomAudio randomData, float overrideCrossFade = -1)
		    {
			    if (restartLocalAudio) return; // we're in the middle of a sting or something. Once it's over we'll revert
			    if (randomData == null)
			    {
				    randomAudio = null;
				    SetMusic(null, 1.0f);
				    return;
			    }
			    if (randomAudio != null)
			    {
				    if (StrReplace.Equals(randomAudio.name, randomData.name)) return;
			    }

			    randomAudio = randomData;
			    if (randomAudio)
			    {
				    AudioClip clip = randomAudio.GetClip(primarySrc.clip);
				    SetMusic(clip, Audio.instance.audioTracker.GetGain(clip), overrideCrossFade);
			    }
		    }
		    public bool				IsPlaying(AudioClip clip)
		    {
			    if (primarySrc.clip == clip) return true;
			    if (secondarySrc.clip == clip) return true;
			    return false;
		    }
		    public bool				IsPlaying(RandomAudio randomData)
		    {
			    if (randomData == null || randomData.clips == null)
			    {
				    return false;
			    }
			    foreach (var t in randomData.clips)
			    {
				    if (IsPlaying(t)) return true;
			    }
			    return false;
		    }
	    }

	    public AudioSource		vfx;	
	    public MusicChannel		music = new MusicChannel();
	    public MusicChannel		ambient = new MusicChannel();
	    public AudioMixer		mixer;
	    public AudioMixerGroup	sfxMixer { get { return mixer.outputAudioMixerGroup; } } 

	    public AudioTracker		audioTracker;

	    public string		CurrentMusicName { get { return "nyi"; } } 
	    public AudioClip	currentMusic { get { return music.primarySrc.clip; } } 
		
	    // Use this for initialization
	    void Awake() 
	    {

	    }

	    private void Start()
	    {
		    UpdateSound();
	    }

	    public static float		GetAttenuation(float normalizedValue)
	    {
		    float minValue = 0.0000001f;
		    normalizedValue = Mathf.Clamp(normalizedValue, minValue, 1);
		    return Mathf.Log10(normalizedValue) * 20;
	    }

	    public void			UpdateSound()
	    {
		    if (mixer != null && SystemSaveContainer.instance.systemSave != null)
		    {
			    mixer.SetFloat("SFXVol", GetAttenuation(SystemSaveContainer.instance.systemSave.audioVolume));
			    mixer.SetFloat("MusicVol",  GetAttenuation(SystemSaveContainer.instance.systemSave.musicVolume));
			    mixer.SetFloat("AmbientVol", GetAttenuation(SystemSaveContainer.instance.systemSave.ambientVolume));
		    }
	    }
	    private void Update()
	    {
		    music.Update();
		    ambient.Update();
	    }

	    public void StopMusic()
	    {
		    music.Stop();
		    ambient.Stop();
	    }

        public void PlayMusic(AudioClip musicClip)
        {
            PlayMusic(musicClip, audioTracker.GetGain(musicClip));
        }
	    public void PlayMusic(AudioClip musicClip, float gain, float fadeTime = 1.0f)
	    {
		    music.SetMusic(musicClip, gain, fadeTime, true);
	    }
	    public void PlayMusic(string byName, float fadeTime = 1.0f)
	    {
		    RandomAudio random = audioTracker.GetRandom(byName);
		    if (random)
		    {
			    music.SetRandomMusic(random, fadeTime);
			    return;
		    }
		    float gain;
		    AudioClip clip = audioTracker.GetClip(byName, out gain);
		    PlayMusic(clip, gain, fadeTime);
	    }

		public void PlayAmbient(AudioClip musicClip)
		{
			PlayAmbient(musicClip, audioTracker.GetGain(musicClip));
		}

		public void PlayAmbient(AudioClip musicClip, float gain, float fadeTime = 1.0f)
	    {
		    //ambient.SetRandomMusic(null);  -- why?
		    ambient.SetMusic(musicClip, gain, fadeTime);
	    }
	    public void PlayAmbient(RandomAudio musicClip, float fadeTime = 1.0f)
	    {
		    ambient.SetRandomMusic(musicClip, fadeTime);
	    }
	    public void PlayAmbient(string byName, float fadeTime = 1.0f)
	    {
		    RandomAudio random = audioTracker.GetRandom(byName);
		    if (random)
		    {
			    ambient.SetRandomMusic(random, fadeTime);
			    return;
		    }
		    float gain;
		    AudioClip clip = audioTracker.GetClip(byName, out gain);
		    PlayAmbient(clip, gain, fadeTime);
	    }

        public void PlaySound(AudioClip reference)
        {
            if (reference == null) return;    
            float gain = audioTracker.GetGain(reference);
            PlaySound(reference, gain);
        }

	    public void PlaySound(AudioClip reference, float gain)
	    {
		    if (reference == null) return;
		    vfx.PlayOneShot(reference, gain);	
	    }

	    public void			PlaySound(string soundName)
	    {
		    if (string.IsNullOrEmpty(soundName)) return;
		    float gain;
		    AudioClip clip = audioTracker.GetClip(soundName, out gain);
		    PlaySound(clip, gain);
	    }
    }
}