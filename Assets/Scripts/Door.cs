using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    [Header("씬 전환 설정")]
    public string nextSceneName;
    public bool useTransition = true;
    public float transitionTime = 1.0f;

    [Header("적 설정")]
    public string enemyTag = "Enemy";

    [Header("메시지 설정")]
    public GameObject messageCanvas;
    public GameObject enemiesRemainingMessage;
    public float messageDisplayTime = 3.0f;

    private bool isTransitioning = false;
    private bool playerInRange = false;

    private void Start()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("다음 씬 이름이 설정되지 않았습니다!");
        }

        if (messageCanvas != null)
        {
            messageCanvas.SetActive(false);
        }

        if (enemiesRemainingMessage != null)
        {
            enemiesRemainingMessage.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            CheckEnemiesAndProceed();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void CheckEnemiesAndProceed()
    {
        if (isTransitioning)
            return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0)
        {
            LoadNextScene();
        }
        else
        {
            ShowEnemiesRemainingMessage();
        }
    }

    private void ShowEnemiesRemainingMessage()
    {
        if (messageCanvas != null && enemiesRemainingMessage != null)
        {
            messageCanvas.SetActive(true);
            enemiesRemainingMessage.SetActive(true);

            StartCoroutine(HideMessageAfterDelay());
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);

        if (messageCanvas != null)
        {
            messageCanvas.SetActive(false);
        }

        if (enemiesRemainingMessage != null)
        {
            enemiesRemainingMessage.SetActive(false);
        }
    }

    // 다음 씬 로드
    private void LoadNextScene()
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("다음 씬 이름이 설정되지 않았습니다!");
            return;
        }

        isTransitioning = true;

        if (useTransition)
        {
            StartCoroutine(TransitionToNextScene());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator TransitionToNextScene()
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
        UnityEngine.UI.Image fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            float alpha = Mathf.Clamp01(elapsedTime / transitionTime);
            fadeImage.color = new Color(0, 0, 0, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
        SceneManager.LoadScene(nextSceneName);
    }
}
