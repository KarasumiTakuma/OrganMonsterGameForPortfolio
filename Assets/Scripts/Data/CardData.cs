using UnityEngine;

/// <summary>
/// カード1枚分の静的データを保持する ScriptableObject。
/// カード名・消費マナ・効果内容・UI表示情報など、
/// バトル中に参照される「カードの定義情報」をまとめて管理する。
/// </summary>
[CreateAssetMenu(fileName = "NewCardData", menuName = "Card Resource/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    /// <summary>カードの表示名</summary>
    [SerializeField] private string cardName;

    /// <summary>このカードを使用する際に消費されるマナ量</summary>
    [SerializeField] private int manaCost;

    /// <summary>カードが持つ効果の種類。攻撃・回復・継続効果などの挙動分岐に使用される</summary>
    [Header("効果情報")]
    [SerializeField] private CardEffectType cardEffectType;

    /// <summary>カード効果の数値量。攻撃カードではダメージ量、回復カードでは回復量として意味付け</summary>
    [Header("効果設定")]
    [SerializeField] private int amount;

    /// <summary>
    /// 継続効果の持続ターン数。
    /// DamageOverTime / HealOverTime 系のカードで使用される。
    /// 単発効果の場合は 0 を想定。
    /// </summary>
    [SerializeField] private int durationTurns;

    /// <summary>カードの説明文。カード詳細表示で使用される。</summary>
    [TextArea]
    [SerializeField] private string description;

    /// <summary>カードの見た目として表示される画像</summary>
    [SerializeField] private Sprite cardSprite;

    /// <summary>カード名を取得する</summary>
    public string GetCardName() => cardName;

    /// <summary>消費マナ量を取得する</summary>
    public int GetManaCost() => manaCost;

    /// <summary>カード効果の種類を取得する</summary>
    public CardEffectType GetCardEffectType() => cardEffectType;

    /// <summary>効果量（ダメージ量・回復量など）を取得する</summary>
    public int GetAmount() => amount;

    /// <summary>継続効果のターン数を取得する</summary>
    public int GetDurationTurns() => durationTurns;

    /// <summary>カードの説明文を取得する</summary>
    public string GetDescription() => description;

    /// <summary>カード画像を取得する</summary>
    public Sprite GetCardSprite() => cardSprite;
}

/// <summary>
/// カードが持つ効果の種類を表す列挙型。
/// カード使用時の処理分岐や演出制御に利用される。
/// </summary>
public enum CardEffectType
{
    AttackToSelected,   // 選択した敵1体に攻撃する
    AttackToAll,        // 敵全体に攻撃する
    Heal,               // 味方を回復する
    Buff,               // 能力を強化するバフ効果
    DamageOverTime,     // ターンごとにダメージを与える継続効果
    HealOverTime        // ターンごとに回復を行う継続効果
}
