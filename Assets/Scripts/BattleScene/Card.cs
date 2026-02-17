using UnityEngine;
// ゲーム内で扱うカードの情報をまとめたモデル。このクラスから成るインスタンスは、そのカードインスタンスが持つ情報をやり取りする
// 具体的には、カード名、マナコスト、カードタイプ、攻撃力や回復量、カード画像などを保持する
// ゲーム内では、このCardオブジェクトを使って手札、デッキ、墓地、場に出たカードの状態を管理する
// CardDataから生成され、UI表示やカード使用時の処理と連携することで、ゲームロジックと表示を統一的に扱える
public class Card
{
    private string cardName;
    private int manaCost;
    private CardEffectType cardEffectType;
    private int power;
    private int durationTurn;
    private Sprite cardImage;

    public Card(CardData cardData)
    {
        cardName = cardData.GetCardName();
        manaCost = cardData.GetManaCost();
        cardEffectType = cardData.GetCardEffectType(); 
        power = cardData.GetPower();
        durationTurn = cardData.GetDurationTurn();
        cardImage = cardData.GetCardImage();
    }

    public string GetName() => cardName;
    public int GetManaCost() => manaCost;
    public CardEffectType GetCardEffectType() => cardEffectType;
    public int GetPower() => power;
    public int GetDurationTurn() => durationTurn;
    public Sprite GetSprite() => cardImage;
}
