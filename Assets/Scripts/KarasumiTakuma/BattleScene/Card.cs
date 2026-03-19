using UnityEngine;

/// <summary>
/// ゲーム内で使用される「カード1枚」を表すランタイム用モデルクラス。
/// ScriptableObject である CardData から生成され、
/// 実際のバトル中における手札・デッキ・墓地・場などでの
/// カード状態管理やロジック処理に使用される。
/// 本クラスはカードの表示情報と効果情報を保持し、
/// UI 表示およびカード使用時の処理を統一的に扱うための中核となる。
/// </summary>
public class Card
{
    /// <summary>カードの表示名</summary>
    private string cardName;

    /// <summary>カード使用時に消費されるマナ量</summary>
    private int manaCost;

    /// <summary>カードの効果タイプ（攻撃・回復・継続効果など）</summary>
    private CardEffectType cardEffectType;

    /// <summary>カードの効果量。 攻撃ダメージ量・回復量・継続効果の強さなど</summary>
    private int amount;

    /// <summary>継続効果の持続ターン数。継続効果を持たないカードの場合は 0 が設定される。</summary>
    private int durationTurns;

    /// <summary>カードの説明文（UI表示用）</summary>
    private string description;

    /// <summary>カードの表示用スプライト</summary>
    private Sprite cardSprite;

    /// <summary>
    /// CardData から Card インスタンスを生成するコンストラクタ。
    /// 静的な定義データ（CardData）をもとに、
    /// バトル中で使用されるカードの実体を生成する。
    /// </summary>
    /// <param name="cardData">元となるカード定義データ。
    /// カードの基本情報・効果・UI情報を取得するために使用される。
    /// </param>
    public Card(CardData cardData)
    {
        cardName = cardData.GetCardName();
        manaCost = cardData.GetManaCost();
        cardEffectType = cardData.GetCardEffectType();
        amount = cardData.GetAmount();
        durationTurns = cardData.GetDurationTurns();
        description = cardData.GetDescription();
        cardSprite= cardData.GetCardSprite();
    }

    /// <summary>カード名を取得する</summary>
    public string GetName() => cardName;

    /// <summary>カードの消費マナ量を取得する</summary>
    public int GetManaCost() => manaCost;

    /// <summary>カードの効果タイプを取得する</summary>
    public CardEffectType GetCardEffectType() => cardEffectType;

    /// <summary>カード効果の量を取得する</summary>
    public int GetAmount() => amount;

    /// <summary>カード効果の継続ターン数を取得する</summary>
    public int GetDurationTurns() => durationTurns;

    /// <summary>カードの説明文を取得する</summary>
    public string GetDescription() => description;

    /// <summary>カードの表示用スプライトを取得する</summary>
    public Sprite GetSprite() => cardSprite;
}
