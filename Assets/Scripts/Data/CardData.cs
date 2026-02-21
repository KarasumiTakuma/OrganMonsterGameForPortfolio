using UnityEngine;

/// カード1枚のデータをアセットとして管理するクラス
[CreateAssetMenu(fileName = "NewCardData", menuName = "Card Resource/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string cardName;          // カード名
    [SerializeField] private int manaCost;             // 消費マナ

    [Header("効果情報")]
    [SerializeField] private CardEffectType cardEffectType;  // カードの効果の種類

    [Header("効果設定")]
    [SerializeField] private int power;                // 効果量（攻撃力や回復量など）

    [Header("継続効果用")]
    [SerializeField] private int durationTurn;         // 継続ターン数

    [Header("UI用情報")]
    [TextArea]
    [SerializeField] private string description;       // 説明文（UI表示用）

    [Header("カード画像")]
    [SerializeField] private Sprite cardImage;         // カードの絵

    public string GetCardName() => cardName;
    public int GetManaCost() => manaCost;
    public CardEffectType GetCardEffectType() => cardEffectType;
    public int GetPower() => power;
    public int GetDurationTurn() => durationTurn;
    public Sprite GetCardImage() => cardImage;
}

// カードが持つ効果をCardEffectTypeとして分類
// enumは列挙型
public enum CardEffectType
{
    AttackToSelected,  // 単体攻撃タイプ(選択した敵へ)
    AttackToAll,       // 全体攻撃タイプ(敵全体への攻撃)
    Heal,              // 回復タイプ
    Buff,              // バフ(強化)タイプ
    DamageOverTime,    // 継続ダメージ
    HealOverTime       // 継続回復
}
