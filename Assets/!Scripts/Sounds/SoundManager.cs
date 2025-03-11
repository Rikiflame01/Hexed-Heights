using UnityEngine;
using System;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [SerializeField] private AudioClip backgroundMusic;

    [SerializeField] private AudioClip endGameSoundtrack;

    [Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();

    private Dictionary<string, AudioClip> soundEffectDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSoundEffectDictionary();

        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    private void InitializeSoundEffectDictionary()
    {
        soundEffectDictionary = new Dictionary<string, AudioClip>();
        foreach (var sfx in soundEffects)
        {
            if (sfx.clip != null && !string.IsNullOrEmpty(sfx.name))
            {
                if (!soundEffectDictionary.ContainsKey(sfx.name))
                {
                    soundEffectDictionary.Add(sfx.name, sfx.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate sound effect name '{sfx.name}' detected. Please use unique names.");
                }
            }
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayEndGameSoundtrack()
    {
        if (musicSource == null || endGameSoundtrack == null)
        {
            Debug.LogWarning("End game soundtrack not set up properly. Make sure musicSource and endGameSoundtrack are assigned in the Inspector.");
            return;
        }

        StopBackgroundMusic();
        musicSource.clip = endGameSoundtrack;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (effectsSource == null || clip == null) return;

        effectsSource.PlayOneShot(clip);
    }

    public void PlaySoundEffectByName(string soundName)
    {
        PlaySoundEffectByName(soundName, 1f);
    }

    public void PlaySoundEffectByName(string soundName, float volume)
    {
        if (effectsSource == null || string.IsNullOrEmpty(soundName)) return;

        if (soundEffectDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            effectsSource.PlayOneShot(clip, volume * effectsSource.volume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found in the sound effects list.");
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetEffectsVolume(float volume)
    {
        if (effectsSource != null)
        {
            effectsSource.volume = Mathf.Clamp01(volume);
        }
    }
}