using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("ī�� ���� ����")]
    public Card.CardType[] correctSequence = {
        Card.CardType.Ten,
        Card.CardType.Jack,
        Card.CardType.Queen,
        Card.CardType.King,
        Card.CardType.Ace
    };

    private List<Card> allCards = new List<Card>();
    private List<Card.CardType> collectedCards = new List<Card.CardType>();
    private bool isSequenceComplete = false;
    private bool hasWrongCard = false;

    public void RegisterCard(Card card)
    {
        if (!allCards.Contains(card))
        {
            allCards.Add(card);
        }
    }

    public bool TryCollectCard(Card card)
    {
        Card.CardType cardType = card.GetCardType();

        if (cardType == Card.CardType.Seven)
        {
            hasWrongCard = true;
            Debug.Log("7 ī�带 �����߽��ϴ�! (�߸��� ī��)");
            return true;
        }

        if (collectedCards.Count < correctSequence.Length)
        {
            Card.CardType expectedCard = correctSequence[collectedCards.Count];

            if (cardType == expectedCard)
            {
                collectedCards.Add(cardType);

                if (collectedCards.Count >= correctSequence.Length)
                {
                    isSequenceComplete = true;
                    Debug.Log("��� ī�带 �ùٸ� ������ �����߽��ϴ�!");
                }
                else
                {
                    Debug.Log($"�ùٸ� ī��! ����: {GetCardName(correctSequence[collectedCards.Count])}");
                }

                return true;
            }
            else
            {
                hasWrongCard = true;
                Debug.Log($"������ Ʋ�Ƚ��ϴ�! ���� �ʿ��� ī��: {GetCardName(expectedCard)}");
                return true;
            }
        }
        else
        {
            hasWrongCard = true;
            Debug.Log("�̹� ��� ī�带 �����߽��ϴ�!");
            return true;
        }
    }

    public bool IsSequenceComplete()
    {
        return isSequenceComplete && !hasWrongCard;
    }

    public bool HasWrongCard()
    {
        return hasWrongCard;
    }

    public bool IsCorrectSequenceCollected()
    {
        return isSequenceComplete;
    }

    public void ResetAllCards()
    {
        collectedCards.Clear();
        isSequenceComplete = false;
        hasWrongCard = false;

        foreach (Card card in allCards)
        {
            card.ResetCard();
        }

        Debug.Log("ī�尡 ��� ���µǾ����ϴ�.");
    }

    public string GetCurrentStatus()
    {
        string status = $"������ ī��: {collectedCards.Count}/{correctSequence.Length}";
        if (hasWrongCard)
        {
            status += " (�߸��� ī�� ����)";
        }
        if (isSequenceComplete)
        {
            status += " (���� �Ϸ�)";
        }
        return status;
    }

    string GetCardName(Card.CardType cardType)
    {
        switch (cardType)
        {
            case Card.CardType.Ten: return "10";
            case Card.CardType.Jack: return "J";
            case Card.CardType.Queen: return "Q";
            case Card.CardType.King: return "K";
            case Card.CardType.Ace: return "A";
            case Card.CardType.Seven: return "7";
            default: return cardType.ToString();
        }
    }
}
