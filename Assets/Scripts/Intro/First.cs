using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [Header("타임라인 설정")]
    public PlayableDirector timeline;
    public string nextSceneName;

    [Header("스킵 설정")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("전환 효과 설정")]
    public bool useTransitionEffect = true;
    public float transitionDuration = 1.0f;
    public Color fadeColor = Color.black;

    private Canvas fadeCanvas;
    private Image fadeImage;

    void Start()
    {
        if (timeline == null)
            timeline = GetComponent<PlayableDirector>();

        timeline.stopped += OnTimelineStopped;

        SetupFadeUI();
    }

    void SetupFadeUI()
    {
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        fadeCanvasObj.transform.SetParent(transform);

        fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        CanvasScaler scaler = fadeCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvas.transform, false);

        fadeImage = fadeImageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        fadeCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            timeline.Stop();

            LoadNextScene();
        }
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("다음 씬 이름이 지정되지 않았습니다! 게임을 종료합니다.");

            if (useTransitionEffect)
            {
                StartCoroutine(FadeAndQuitGame());
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }

            return;
        }

        if (useTransitionEffect)
        {
            StartCoroutine(FadeAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        fadeCanvas.gameObject.SetActive(true);

        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / transitionDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeAndQuitGame()
    {
        fadeCanvas.gameObject.SetActive(true);

        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / transitionDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineStopped;
    }
}
