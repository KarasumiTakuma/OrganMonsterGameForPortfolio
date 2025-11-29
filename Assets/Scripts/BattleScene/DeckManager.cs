using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Management")]
    public List<Card> deck = new List<Card>();        // 山札
    public List<Card> hand = new List<Card>();        // 手札
    public List<Card> discardPile = new List<Card>(); // 墓地

    private const int FullHandSize = 5;

    [Header("Start Game Hand Setting")]
    private int initialHandSize = FullHandSize; // ゲーム開始時の手札枚数(最大の5枚にする)

    /// <summary>
    /// デッキにカードを追加
    /// </summary>
    public void AddCardToDeck(Card card)
    {
        deck.Add(card);
    }

    /// <summary>
    /// ゲーム開始時に初期手札を引くためのメソッド
    /// </summary>
    public void DrawInitialHand()
    {
        for (int i = 0; i < initialHandSize; i++)
        {
            DrawCard();
        }
    }

    public void DrawCardFull()
    {
        while (hand.Count < FullHandSize)
        {
            Card card = DrawCard();
            if (card == null) break;
        }
    }

    /// デッキから1枚ドロー
    private Card DrawCard()
    {
        if (deck.Count == 0)
        {
            ReshuffleDiscardIntoDeck();
        }

        if (deck.Count == 0)
        {
            Debug.LogWarning("山札も墓地も空です！");
            return null;
        }

        Card drawn = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawn);
        return drawn;
    }

    /// <summary>
    /// 使用済みカードを墓地に送る
    /// </summary>
    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            discardPile.Add(card);
        }
    }

    /// <summary>
    /// 墓地をシャッフルして山札に戻す
    /// </summary>
    private void ReshuffleDiscardIntoDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
    }

    /// <summary>
    /// 山札をシャッフル
    /// </summary>
    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    // 手札リスト取得のためのゲッター
    public List<Card> GetHand() => hand;
    public int GetHandCount() => hand.Count;
}