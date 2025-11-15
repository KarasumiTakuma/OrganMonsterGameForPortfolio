using UnityEngine;

/// カード1枚のデータをアセットとして管理するクラス
[CreateAssetMenu(fileName = "NewCardData", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    public string cardName;          // カード名
    public int manaCost;             // 消費マナ
    public CardType cardType;        // カードの種類（攻撃・回復など）

    [Header("効果設定")]
    public int power;                // 効果量（攻撃力や回復量など）

    [Header("UI用情報")]
    [TextArea]
    public string description;       // 説明文（UI表示用）

    [Header("カード画像")]
    public Sprite cardImage;         // カードの絵
}

public enum CardType
{
    AttackToSelected,  // 単体攻撃用のカードタイプ。選択した敵に対する攻撃
    Heal,
    Magic,
    Buff,
    Debuff
}
