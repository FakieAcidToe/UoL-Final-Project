using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEditor.PlayerSettings;

public class SoundManager : MonoBehaviour
{
	[System.Serializable]
	class AudioData
	{
		public string clipName;
		public AudioClip audioClip;

		public AudioData(string _clipName, AudioClip _audioClip)
		{
			clipName = _clipName;
			audioClip = _audioClip;
		}
	}

	[SerializeField] AudioSource effectsSource;
	[SerializeField] AudioSource musicSource;

	[SerializeField] List<AudioData> audioList = new List<AudioData>();

	public static SoundManager Instance = null;

	AudioClip bgmIntro = null;
	AudioClip bgmLoop = null;
	bool bgmInIntro = false;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}

	void Start()
	{
		// load saved volume
		SetMusicVolume(SaveManager.Instance.CurrentSaveData.musicVolume);
		SetSFXVolume(SaveManager.Instance.CurrentSaveData.sfxVolume);
	}

	public void SetMusicVolume(float musicVolume)
	{
		SetMixerVolume(musicVolume, musicSource.outputAudioMixerGroup.audioMixer, "Music Volume");
	}

	public void SetSFXVolume(float sfxVolume)
	{
		SetMixerVolume(sfxVolume, effectsSource.outputAudioMixerGroup.audioMixer, "SFX Volume");
	}

	void SetMixerVolume(float sliderValue, AudioMixer audioMixer, string exposedName)
	{
		float normalizedValue = Mathf.InverseLerp(0, 200, sliderValue);
		float remappedValue = Mathf.Lerp(0.0001f, 1, normalizedValue);

		float dB = Mathf.Log10(Mathf.Clamp(remappedValue, 0.0001f, 1)) * 20;
		audioMixer.SetFloat(exposedName, dB);
	}

	void Update()
	{
		if (bgmInIntro && !musicSource.isPlaying)
		{
			PlayMusicLoop(bgmLoop);
		}
	}

	public void Play(AudioClip _clip)
	{
		effectsSource.clip = _clip;
		effectsSource.PlayOneShot(_clip);
	}

	public void Play(string _clipName)
	{
		foreach (AudioData data in audioList) if (data.clipName == _clipName)
			{
				Play(data.audioClip);
				return;
			}
		Debug.LogWarning("Could not find AudioClip '" + _clipName + "'.");
	}

	public void PlayMusic(AudioClip _loop, AudioClip _intro = null, bool forceRestartIfSameTrack = false)
	{
		if (!forceRestartIfSameTrack && bgmLoop == _loop) // if same track
			return;

		bgmLoop = _loop;
		bgmIntro = _intro;
		if (bgmIntro != null)
		{
			PlayMusicLoop(bgmIntro);
			bgmInIntro = true;
			musicSource.loop = false;
		}
		else
		{
			PlayMusicLoop(bgmLoop);
		}
	}

	public void PlayMusicLoop(AudioClip _clip)
	{
		bgmInIntro = false;
		musicSource.loop = true;
		musicSource.clip = _clip;
		musicSource.Play();
	}
}