using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("카드 설정")]
    public CardType cardType;
    public Vector2 pickupBoxSize = new Vector2(2f, 2f);

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
            CollectCard();
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
                isCollected = true;
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"잘못된 카드입니다: {cardType}");
            }
        }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, pickupBoxSize);
    }
}
