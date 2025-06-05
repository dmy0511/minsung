using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHelper : MonoBehaviour
{
    [Header("Audio Type")]
    public AudioType audioType = AudioType.BGM;

    private AudioSource audioSource;
    private float originalVolume;

    public enum AudioType
    {
        BGM,
        SFX
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
    }

    private void Start()
    {
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        if (SoundManager.Instance == null) return;

        switch (audioType)
        {
            case AudioType.BGM:
                audioSource.volume = originalVolume * SoundManager.Instance.BGMVolume;
                break;
            case AudioType.SFX:
                audioSource.volume = originalVolume * SoundManager.Instance.SFXVolume;
                break;
        }
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
            ApplyVolumeSettings();
    }
}
