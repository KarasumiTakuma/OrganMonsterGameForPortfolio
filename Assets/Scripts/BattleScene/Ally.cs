using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 味方モンスター1体を表すクラス
/// </summary>
[System.Serializable]
public class Ally
{
    // 基本情報
    public string allyName;      // 名前
    public int maxHP;            // 最大HP
    public int currentHP;        // 現在HP
    public Sprite allyImage;     // UIで表示する画像

    // この味方のカード山札
    private List<Card> deck = new List<Card>();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="name">味方の名前</param>
    /// <param name="hp">最大HP</param>
    /// <param name="image">UI表示用の画像</param>
    /// <param name="initialDeck">初期カードリスト</param>
    public Ally(string name, int hp, Sprite image, List<Card> initialDeck)
    {
        allyName = name;
        maxHP = hp;
        currentHP = maxHP;
        allyImage = image;

        if (initialDeck != null)
        {
            deck = new List<Card>(initialDeck); // デッキをコピーして保持
        }
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(currentHP - dmg, 0);
    }

    /// <summary>
    /// 回復する
    /// </summary>
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    /// <summary>
    /// 生存確認
    /// </summary>
    public bool IsAlive() => currentHP > 0;

    /// <summary>
    /// デッキから1枚ドロー
    /// </summary>
    public Card DrawCard()
    {
        if (deck.Count == 0) return null;

        Card drawn = deck[0];
        deck.RemoveAt(0);
        return drawn;
    }
}