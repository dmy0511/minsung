using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("카드 순서 설정")]
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
            Debug.Log("7 카드를 수집했습니다! (잘못된 카드)");
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
                    Debug.Log("모든 카드를 올바른 순서로 수집했습니다!");
                }
                else
                {
                    Debug.Log($"올바른 카드! 다음: {GetCardName(correctSequence[collectedCards.Count])}");
                }

                return true;
            }
            else
            {
                hasWrongCard = true;
                Debug.Log($"순서가 틀렸습니다! 현재 필요한 카드: {GetCardName(expectedCard)}");
                return true;
            }
        }
        else
        {
            hasWrongCard = true;
            Debug.Log("이미 모든 카드를 수집했습니다!");
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

        Debug.Log("카드가 모두 리셋되었습니다.");
    }

    public string GetCurrentStatus()
    {
        string status = $"수집된 카드: {collectedCards.Count}/{correctSequence.Length}";
        if (hasWrongCard)
        {
            status += " (잘못된 카드 포함)";
        }
        if (isSequenceComplete)
        {
            status += " (순서 완료)";
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
