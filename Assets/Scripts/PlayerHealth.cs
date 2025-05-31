using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("ü�� ����")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("ü�� UI")]
    public Image[] healthImages;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;

    [Header("���� �ð� ����")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;

    [Header("���� ���� ����")]
    public GameObject gameOverPanel;
    public float gameOverDelay = 1.5f;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    private CanvasGroup gameOverCanvasGroup;

    private bool isInvincible = false;
    private SpriteRenderer playerSpriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;

        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (gameOverPanel != null)
        {
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }

            gameOverCanvasGroup.alpha = 0f;
            gameOverPanel.SetActive(false);
        }

        UpdateHealthUI();

        if (healthImages == null || healthImages.Length == 0)
        {
            FindHealthUI();
        }
    }

    void FindHealthUI()
    {
        GameObject[] healthUIObjects = GameObject.FindGameObjectsWithTag("HealthUI");

        if (healthUIObjects.Length > 0)
        {
            healthImages = new Image[healthUIObjects.Length];
            for (int i = 0; i < healthUIObjects.Length; i++)
            {
                healthImages[i] = healthUIObjects[i].GetComponent<Image>();
            }

            System.Array.Sort(healthImages, (x, y) => x.name.CompareTo(y.name));
        }
        else
        {
            Debug.LogWarning("HealthUI �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"�÷��̾ {damage} �������� �޾ҽ��ϴ�. ���� ü��: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    public void HealHealth(int healAmount = 1)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"�÷��̾ {healAmount} ü���� ȸ���߽��ϴ�. ���� ü��: {currentHealth}/{maxHealth}");

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthImages == null) return;

        for (int i = 0; i < healthImages.Length; i++)
        {
            if (healthImages[i] != null)
            {
                if (i < currentHealth)
                {
                    healthImages[i].sprite = fullHealthSprite;
                }
                else
                {
                    healthImages[i].sprite = emptyHealthSprite;
                }
            }
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsedTime = 0f;
        Color originalColor = playerSpriteRenderer.color;

        while (elapsedTime < invincibilityTime)
        {
            if (playerSpriteRenderer != null)
            {
                Color currentColor = playerSpriteRenderer.color;
                playerSpriteRenderer.color = (currentColor == originalColor) ? Color.red : originalColor;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalColor;
        }

        isInvincible = false;
    }

    void PlayerDeath()
    {
        Debug.Log("�÷��̾ �׾����ϴ�!");

        Time.timeScale = 0f;

        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        if (gameOverPanel != null && gameOverCanvasGroup != null)
        {
            gameOverPanel.SetActive(true);

            yield return StartCoroutine(FadeIn());

            yield return new WaitForSecondsRealtime(gameOverDelay);

            yield return StartCoroutine(FadeOut());
        }
        else
        {
            yield return new WaitForSecondsRealtime(gameOverDelay);
        }

        QuitGame();
    }

    IEnumerator FadeIn()
    {
        if (gameOverCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        gameOverCanvasGroup.alpha = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        if (gameOverCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        gameOverCanvasGroup.alpha = 1f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        gameOverCanvasGroup.alpha = 0f;
    }

    void QuitGame()
    {
        Debug.Log("������ �����մϴ�.");

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ���
        Application.Quit();
#endif
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spear") || other.GetComponent<Spear>() != null)
        {
            TakeDamage(1);
        }

        SpadeController spade = other.GetComponent<SpadeController>();
        if (spade != null)
        {
            TakeDamage(1);
        }
    }
}
