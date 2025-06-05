using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSoundPlayer : MonoBehaviour
{
    [Header("Sound Effects")]
    public List<SoundClip> soundClips = new List<SoundClip>();

    private AudioSource audioSource;
    private Dictionary<string, SoundClip> soundDictionary;

    [System.Serializable]
    public class SoundClip
    {
        public string soundName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeSoundDictionary();
    }

    private void InitializeSoundDictionary()
    {
        soundDictionary = new Dictionary<string, SoundClip>();
        foreach (var soundClip in soundClips)
        {
            if (!string.IsNullOrEmpty(soundClip.soundName) && soundClip.clip != null)
            {
                soundDictionary[soundClip.soundName] = soundClip;
            }
        }
    }

    public void PlaySound(string soundName)
    {
        PlaySoundWithSettings(soundName, 1f, 1f);
    }

    public void PlaySoundWithVolume(string soundName, float volume)
    {
        PlaySoundWithSettings(soundName, volume, 1f);
    }

    public void PlaySoundWithSettings(string soundName, float volume, float pitch)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            var soundClip = soundDictionary[soundName];
            audioSource.clip = soundClip.clip;
            audioSource.volume = soundClip.volume * volume * GetSFXVolumeMultiplier();
            audioSource.pitch = soundClip.pitch * pitch;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"사운드 '{soundName}'를 찾을 수 없습니다!");
        }
    }

    public void PlaySoundOneShot(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            var soundClip = soundDictionary[soundName];
            float finalVolume = soundClip.volume * GetSFXVolumeMultiplier();
            audioSource.pitch = soundClip.pitch;
            audioSource.PlayOneShot(soundClip.clip, finalVolume);
        }
        else
        {
            Debug.LogWarning($"사운드 '{soundName}'를 찾을 수 없습니다!");
        }
    }

    private float GetSFXVolumeMultiplier()
    {
        return SoundManager.Instance != null ? SoundManager.Instance.SFXVolume : 1f;
    }

    [ContextMenu("Test First Sound")]
    private void TestFirstSound()
    {
        if (soundClips.Count > 0)
            PlaySound(soundClips[0].soundName);
    }
}
