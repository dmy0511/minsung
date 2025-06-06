using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("설정 패널")]
    public GameObject settingsPanel;

    [Header("입력 설정")]
    public KeyCode settingsKey = KeyCode.Escape;

    [Header("일시정지 설정")]
    public bool pauseGameWhenSettingsOpen = true;

    [Header("디버그")]
    public bool showDebugInfo = false;

    private bool isSettingsOpen = false;
    private bool wasTimeScaleZero = false;

    void Start()
    {
        if (settingsPanel == null)
        {
            FindSettingsPanel();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        isSettingsOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(settingsKey))
        {
            ToggleSettings();
        }
    }

    void FindSettingsPanel()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if (objName.Contains("settings") || objName.Contains("설정") ||
                objName.Contains("option") || objName.Contains("menu"))
            {
                if (obj.GetComponent<Canvas>() != null || obj.GetComponent<Image>() != null)
                {
                    settingsPanel = obj;
                    if (showDebugInfo)
                    {
                        Debug.Log($"설정 패널을 자동으로 찾았습니다: {obj.name}");
                    }
                    break;
                }
            }
        }

        if (settingsPanel == null && showDebugInfo)
        {
            Debug.LogWarning("설정 패널을 찾을 수 없습니다. Inspector에서 직접 연결해주세요.");
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("설정 패널이 설정되지 않았습니다!");
            return;
        }

        isSettingsOpen = !isSettingsOpen;

        if (showDebugInfo)
        {
            Debug.Log($"설정 패널 {(isSettingsOpen ? "열림" : "닫힘")}");
        }

        settingsPanel.SetActive(isSettingsOpen);

        if (pauseGameWhenSettingsOpen)
        {
            if (isSettingsOpen)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void OpenSettings()
    {
        if (!isSettingsOpen)
        {
            ToggleSettings();
        }
    }

    public void CloseSettings()
    {
        if (isSettingsOpen)
        {
            ToggleSettings();
        }
    }

    void PauseGame()
    {
        wasTimeScaleZero = (Time.timeScale == 0f);
        if (!wasTimeScaleZero)
        {
            Time.timeScale = 0f;
            if (showDebugInfo)
            {
                Debug.Log("게임 일시정지");
            }
        }
    }

    void ResumeGame()
    {
        if (!wasTimeScaleZero)
        {
            Time.timeScale = 1f;
            if (showDebugInfo)
            {
                Debug.Log("게임 재개");
            }
        }
    }

    public void OnCloseButton()
    {
        CloseSettings();
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainScene");
    }

    public void OnResumeButton()
    {
        CloseSettings();
    }

    public bool IsSettingsOpen()
    {
        return isSettingsOpen;
    }

    public void SetSettingsPanel(GameObject panel)
    {
        settingsPanel = panel;
    }

    public void SetPauseWhenSettingsOpen(bool pause)
    {
        pauseGameWhenSettingsOpen = pause;
    }

    void OnDestroy()
    {
        if (Time.timeScale == 0f && !wasTimeScaleZero)
        {
            Time.timeScale = 1f;
        }
    }
}
