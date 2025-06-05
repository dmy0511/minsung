using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixers")]
    public AudioMixerGroup bgmMixer;
    public AudioMixerGroup sfxMixer;

    [Header("Default Volumes")]
    [Range(0f, 1f)] public float defaultBGMVolume = 0.8f;
    [Range(0f, 1f)] public float defaultSFXVolume = 0.8f;

    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSound();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSound()
    {
        BGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);

        ApplyVolumeSettings();
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
        ApplyBGMVolume();
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXVolume);
        ApplySFXVolume();
        PlayerPrefs.Save();
    }

    private void ApplyVolumeSettings()
    {
        ApplyBGMVolume();
        ApplySFXVolume();
    }

    private void ApplyBGMVolume()
    {
        float dbValue = BGMVolume > 0 ? Mathf.Log10(BGMVolume) * 20 : -80f;
        if (bgmMixer != null)
            bgmMixer.audioMixer.SetFloat("BGMVolume", dbValue);
    }

    private void ApplySFXVolume()
    {
        float dbValue = SFXVolume > 0 ? Mathf.Log10(SFXVolume) * 20 : -80f;
        if (sfxMixer != null)
            sfxMixer.audioMixer.SetFloat("SFXVolume", dbValue);
    }

    public void ResetToDefault()
    {
        SetBGMVolume(defaultBGMVolume);
        SetSFXVolume(defaultSFXVolume);
    }
}
