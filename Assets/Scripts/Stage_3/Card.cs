using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("카드 설정")]
    public CardType cardType;

    [Header("상호작용 설정")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI 설정")]
    public Canvas interactionCanvas;
    public float blinkSpeed = 2f;

    [Header("사운드 설정")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private Vector3 originalPosition;
    private bool isCollected = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D cardCollider;
    private TMP_Text interactionText;
    private Coroutine blinkCoroutine;

    public enum CardType
    {
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
        Seven = 7
    }

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardCollider = GetComponent<Collider2D>();

        if (interactionCanvas != null)
        {
            interactionText = interactionCanvas.GetComponentInChildren<TMP_Text>();
            interactionCanvas.gameObject.SetActive(false);
        }

        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            cardManager.RegisterCard(this);
        }
    }

    void Update()
    {
        if (playerInRange && !isCollected && Input.GetKeyDown(interactionKey))
        {
            CollectCard();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            ShowInteractionUI();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideInteractionUI();
        }
    }

    void ShowInteractionUI()
    {
        playerInRange = true;

        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(true);

            if (interactionText != null)
            {
                interactionText.text = $"{interactionKey}";
            }

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            blinkCoroutine = StartCoroutine(BlinkText());
        }
    }

    void HideInteractionUI()
    {
        playerInRange = false;

        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    IEnumerator BlinkText()
    {
        if (interactionText == null) yield break;

        while (playerInRange && !isCollected)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            Color textColor = interactionText.color;
            textColor.a = alpha;
            interactionText.color = textColor;

            yield return null;
        }

        if (interactionText != null)
        {
            Color textColor = interactionText.color;
            textColor.a = 1f;
            interactionText.color = textColor;
        }
    }

    void CollectCard()
    {
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            bool canCollect = cardManager.TryCollectCard(this);
            if (canCollect)
            {
                PlayCollectSoundAtPoint();
                isCollected = true;
                HideInteractionUI();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"잘못된 카드입니다: {cardType}");
            }
        }
    }

    void PlayCollectSoundAtPoint()
    {
        if (collectSound != null)
        {
            float finalVolume = soundVolume * GetSFXVolumeMultiplier();
            AudioSource.PlayClipAtPoint(collectSound, transform.position, finalVolume);
        }
    }

    private float GetSFXVolumeMultiplier()
    {
        return SoundManager.Instance != null ? SoundManager.Instance.SFXVolume : 1f;
    }

    public void ResetCard()
    {
        isCollected = false;
        playerInRange = false;
        transform.position = originalPosition;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (cardCollider != null)
        {
            cardCollider.enabled = true;
        }

        HideInteractionUI();
    }

    public bool IsCollected()
    {
        return isCollected;
    }

    public CardType GetCardType()
    {
        return cardType;
    }

    public Vector2 GetPickupBoxCenter()
    {
        return (Vector2)transform.position;
    }
}
