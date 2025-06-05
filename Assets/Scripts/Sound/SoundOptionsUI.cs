using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundOptionsUI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Volume Display (Optional)")]
    public TMP_Text bgmVolumeText;
    public TMP_Text sfxVolumeText;

    private void Start()
    {
        InitializeSliders();
        SetupSliderEvents();
    }

    private void InitializeSliders()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager가 없습니다! 먼저 SoundManager를 씬에 추가하세요.");
            return;
        }

        if (bgmSlider != null)
            bgmSlider.value = SoundManager.Instance.BGMVolume;

        if (sfxSlider != null)
            sfxSlider.value = SoundManager.Instance.SFXVolume;

        UpdateVolumeTexts();
    }

    private void SetupSliderEvents()
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);
            UpdateBGMVolumeText();
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
            UpdateSFXVolumeText();
        }
    }

    private void UpdateVolumeTexts()
    {
        UpdateBGMVolumeText();
        UpdateSFXVolumeText();
    }

    private void UpdateBGMVolumeText()
    {
        if (bgmVolumeText != null && SoundManager.Instance != null)
        {
            bgmVolumeText.text = Mathf.RoundToInt(SoundManager.Instance.BGMVolume * 100) + "%";
        }
    }

    private void UpdateSFXVolumeText()
    {
        if (sfxVolumeText != null && SoundManager.Instance != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(SoundManager.Instance.SFXVolume * 100) + "%";
        }
    }

    public void ResetToDefault()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResetToDefault();
            InitializeSliders();
        }
    }

    private void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
