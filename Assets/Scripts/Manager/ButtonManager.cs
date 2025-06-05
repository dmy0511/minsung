using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [Header("�� ����")]
    public string nextSceneName = "IntroStory";

    [Header("���̵� ��ȯ ����")]
    public bool useFadeTransition = true;
    public float fadeTransitionTime = 1.0f;

    [Header("��ư ����")]
    public Button startButton;
    public Button quitButton;

    private bool isTransitioning = false;

    void Start()
    {
        if (startButton == null)
        {
            FindStartButton();
        }

        if (quitButton == null)
        {
            FindExitButton();
        }

        SetupButtonEvents();
    }

    void FindStartButton()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLower();
            if (buttonName.Contains("start") || buttonName.Contains("����") || buttonName.Contains("play"))
            {
                startButton = button;
                break;
            }
        }
    }

    void FindExitButton()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLower();
            if (buttonName.Contains("exit") || buttonName.Contains("������") || buttonName.Contains("quit"))
            {
                quitButton = button;
                break;
            }
        }
    }

    void SetupButtonEvents()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("Start Button�� ã�� �� �����ϴ�. Inspector���� ���� �������ּ���.");
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnExitButtonClicked);
        }
        else
        {
            Debug.LogWarning("Exit Button�� ã�� �� �����ϴ�. Inspector���� ���� �������ּ���.");
        }
    }

    public void OnStartButtonClicked()
    {
        if (isTransitioning) return;

        Debug.Log("���� ��ư�� Ŭ���Ǿ����ϴ�.");

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("���� �� �̸��� �������� �ʾҽ��ϴ�!");
            return;
        }

        LoadNextScene();
    }

    public void OnExitButtonClicked()
    {
        if (isTransitioning) return;

        Debug.Log("������ ��ư�� Ŭ���Ǿ����ϴ�.");
        QuitGame();
    }

    void LoadNextScene()
    {
        isTransitioning = true;

        if (useFadeTransition)
        {
            StartCoroutine(FadeTransitionToScene());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeTransitionToScene()
    {
        GameObject fadeObj = new GameObject("SceneTransition");
        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        CanvasScaler scaler = fadeObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeCanvas.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        float elapsedTime = 0;
        while (elapsedTime < fadeTransitionTime)
        {
            float alpha = Mathf.Clamp01(elapsedTime / fadeTransitionTime);
            fadeImage.color = new Color(0, 0, 0, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);

        SceneManager.LoadScene(nextSceneName);
    }

    void QuitGame()
    {
        Debug.Log("������ �����մϴ�.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ��� ���� ���� ��
        Application.Quit();
#endif
    }

    public void SetNextSceneName(string sceneName)
    {
        nextSceneName = sceneName;
    }

    public void StartGame()
    {
        OnStartButtonClicked();
    }

    public void ExitGame()
    {
        OnExitButtonClicked();
    }
}
