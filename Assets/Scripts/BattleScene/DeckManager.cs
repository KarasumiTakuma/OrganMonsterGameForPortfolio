using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("デッキ管理")]
    /// <summary>山札。ドロー時にここからカードが引かれる。</summary>
    private List<Card> deck = new List<Card>();

    /// <summary>現在プレイヤーが所持している手札。</summary>
    private List<Card> hand = new List<Card>();

    /// <summary>使用済みカードを保持する墓地。</summary>
    private List<Card> discardPile = new List<Card>();

    [Header("UI表示")]
    /// <summary>山札の残り枚数を表示するTextMeshPro</summary>
    [SerializeField] private TMP_Text deckText;

    /// <summary>墓地のカード枚数を表示するTextMeshPro。</summary>
    [SerializeField] private TMP_Text discardPileText;

    /// <summary>手札の最大枚数。</summary>
    private const int FullHandSize = 5;

    /// <summary>ゲーム開始時に引く初期手札の枚数</summary>
    [Header("手札初期設定")]
    [SerializeField] private int initialHandSize = FullHandSize;

    /// <summary>
    /// 山札にカードを1枚追加する。
    /// デッキ構築や初期化時に使用される。
    /// </summary>
    /// <param name="card">山札に追加するカード</param>
    public void AddCardToDeck(Card card)
    {
        deck.Add(card);
    }

    /// <summary>
    /// ゲーム開始時に初期手札を引く。
    /// initialHandSize の枚数分ドローを行う。
    /// </summary>
    public void DrawInitialHand()
    {
        for (int i = 0; i < initialHandSize; i++)
        {
            DrawCard();
        }
        UpdateCardUI();
    }

    /// <summary>
    /// 手札が最大枚数になるまでカードをドローする。
    /// 山札・墓地が尽きた場合は途中で終了する。
    /// </summary>
    public void DrawUntilFullHand()
    {
        while (hand.Count < FullHandSize)
        {
            Card card = DrawCard();
            if (card == null) break;
        }
        UpdateCardUI();
    }


    /// <summary>
    /// 使用したカードを手札から墓地へ移動する。
    /// </summary>
    /// <param name="card">墓地へ送るカード</param>
    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            discardPile.Add(card);
        }
        UpdateCardUI();
    }

    /// <summary>
    /// 墓地のカードを山札に戻し、シャッフルする
    /// </summary>
    private void ReshuffleDiscardIntoDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
    }

    /// <summary>
    /// 山札をランダムにシャッフルする
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
        UpdateCardUI();
    }

    /// <summary>
    /// 山札・手札・墓地をすべて空にする。
    /// バトル終了時やリセット処理で使用。
    /// </summary>
    public void ClearDeck()
    {
        deck.Clear();
        hand.Clear();
        discardPile.Clear();
        UpdateCardUI();
    }

    /// <summary>
    /// 山札からカードを1枚ドローする内部処理。
    /// 山札が空の場合は墓地をシャッフルして補充する。
    /// </summary>
    /// <returns>
    /// ドローしたカード。
    /// 山札・墓地ともに空の場合は null。
    /// </returns>
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
    /// 山札・墓地の枚数表示UIを更新する。
    /// </summary>
    private void UpdateCardUI()
    {
        if (deckText != null)
        {
            deckText.text = this.GetDeckCount().ToString();
        }

        if (discardPileText != null)
        {
            discardPileText.text = this.GetDiscardPileCount().ToString();
        }
    }


    /// <summary>現在の手札リストを取得する</summary>
    public List<Card> GetHand() => hand;

    /// <summary>山札の残り枚数を取得する</summary>
    public int GetDeckCount() => deck.Count;

    /// <summary>手札の枚数を取得する</summary>
    public int GetHandCount() => hand.Count;

    /// <summary>墓地のカード枚数を取得する</summary>
    public int GetDiscardPileCount() => discardPile.Count;
}