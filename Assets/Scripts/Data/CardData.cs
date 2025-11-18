using UnityEngine;

/// カード1枚のデータをアセットとして管理するクラス
[CreateAssetMenu(fileName = "NewCardData", menuName = "Card Resource/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string cardName;          // カード名
    [SerializeField] private int manaCost;             // 消費マナ
    [SerializeField] private CardType cardType;        // カードの種類（攻撃・回復など）

    [Header("効果設定")]
    [SerializeField] private int power;                // 効果量（攻撃力や回復量など）

    [Header("UI用情報")]
    [TextArea]
    [SerializeField] private string description;       // 説明文（UI表示用）

    [Header("カード画像")]
    [SerializeField] private Sprite cardImage;         // カードの絵

    public string GetCardName() => cardName;
    public int GetManaCost() => manaCost;
    public CardType GetCardType() => cardType;
    public int GetPower() => power;
    public Sprite GetCardImage() => cardImage;
}

public enum CardType
{
    AttackToSelected,  // 単体攻撃用のカードタイプ。選択した敵に対する攻撃
    Heal,
    Magic,
    Buff,
    Debuff
}
