using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("카드 설정")]
    public CardType cardType;
    public Vector2 pickupBoxSize = new Vector2(2f, 2f);
    public Vector2 pickupBoxOffset = new Vector2(0f, 0f);

    [Header("사운드 설정")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private Vector3 originalPosition;
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D cardCollider;

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

        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            cardManager.RegisterCard(this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            Vector2 boxCenter = (Vector2)transform.position + pickupBoxOffset;
            Vector2 playerPosition = other.transform.position;

            bool isInPickupArea = (playerPosition.x >= boxCenter.x - pickupBoxSize.x / 2f) &&
                                  (playerPosition.x <= boxCenter.x + pickupBoxSize.x / 2f) &&
                                  (playerPosition.y >= boxCenter.y - pickupBoxSize.y / 2f) &&
                                  (playerPosition.y <= boxCenter.y + pickupBoxSize.y / 2f);

            if (isInPickupArea)
            {
                CollectCard();
            }
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
        return (Vector2)transform.position + pickupBoxOffset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxCenter = (Vector2)transform.position + pickupBoxOffset;
        Gizmos.DrawWireCube(boxCenter, pickupBoxSize);
    }
}
