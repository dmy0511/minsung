using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("���� �г�")]
    public GameObject settingsPanel;

    [Header("�Է� ����")]
    public KeyCode settingsKey = KeyCode.Escape;

    [Header("�Ͻ����� ����")]
    public bool pauseGameWhenSettingsOpen = true;

    [Header("�����")]
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
            if (objName.Contains("settings") || objName.Contains("����") ||
                objName.Contains("option") || objName.Contains("menu"))
            {
                if (obj.GetComponent<Canvas>() != null || obj.GetComponent<Image>() != null)
                {
                    settingsPanel = obj;
                    if (showDebugInfo)
                    {
                        Debug.Log($"���� �г��� �ڵ����� ã�ҽ��ϴ�: {obj.name}");
                    }
                    break;
                }
            }
        }

        if (settingsPanel == null && showDebugInfo)
        {
            Debug.LogWarning("���� �г��� ã�� �� �����ϴ�. Inspector���� ���� �������ּ���.");
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("���� �г��� �������� �ʾҽ��ϴ�!");
            return;
        }

        isSettingsOpen = !isSettingsOpen;

        if (showDebugInfo)
        {
            Debug.Log($"���� �г� {(isSettingsOpen ? "����" : "����")}");
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
                Debug.Log("���� �Ͻ�����");
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
                Debug.Log("���� �簳");
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
