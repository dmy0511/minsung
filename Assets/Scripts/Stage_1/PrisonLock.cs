using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonLock : MonoBehaviour
{
    [Header("참조")]
    public GameObject prisonBars;
    public GameObject player;

    [Header("메시지 설정")]
    public GameObject messageCanvas;
    public GameObject unlockMessage;
    public float messageDisplayTime = 3.0f;

    [Header("설정")]
    public float unlockDelay = 0.5f;
    public bool enablePlayerAfterUnlock = true;

    private SpriteRenderer spriteRenderer;
    private bool isUnlocked = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (messageCanvas != null)
        {
            messageCanvas.SetActive(false);
        }

        if (unlockMessage != null)
        {
            unlockMessage.SetActive(false);
        }

        if (player != null && enablePlayerAfterUnlock)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
    }

    private void OnMouseDown()
    {
        if (isUnlocked)
            return;

        StartCoroutine(UnlockPrison());
    }

    private IEnumerator UnlockPrison()
    {
        isUnlocked = true;

        ShowUnlockMessage();

        float shakeTime = 0.3f;
        float elapsedTime = 0;
        Vector3 originalPosition = transform.position;

        while (elapsedTime < shakeTime)
        {
            transform.position = originalPosition + new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        yield return new WaitForSeconds(unlockDelay);

        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            elapsedTime = 0;
            Color originalColor = spriteRenderer.color;

            while (elapsedTime < fadeTime)
            {
                float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        if (prisonBars != null)
        {
            StartCoroutine(FadeOutObject(prisonBars));
        }

        if (player != null && enablePlayerAfterUnlock)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    private void ShowUnlockMessage()
    {
        if (messageCanvas != null && unlockMessage != null)
        {
            messageCanvas.SetActive(true);
            unlockMessage.SetActive(true);

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

        if (unlockMessage != null)
        {
            unlockMessage.SetActive(false);
        }
    }

    private IEnumerator FadeOutObject(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        float fadeTime = 1.0f;
        float elapsedTime = 0;

        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);

            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, alpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (!isUnlocked && spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.8f, 0.8f);
        }
    }

    private void OnMouseExit()
    {
        if (!isUnlocked && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
}
