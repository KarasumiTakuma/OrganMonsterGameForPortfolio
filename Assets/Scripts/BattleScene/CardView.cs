using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text powerText; // 攻撃力や回復量など

    private Card card;

    public void SetCard(Card cardData)
    {
        card = cardData;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (card == null) return;

        // カード名
        nameText.text = card.GetName();

        // マナコストを「Cost：」付きで表示
        manaText.text = $"Cost：{card.GetManaCost()}";

        // 効果量をカードタイプに応じて表示（攻撃ならAtt、回復ならHealなど）
        switch (card.GetCardType())
        {
            case CardType.AttackToSelected:
                powerText.text = $"Att {card.GetPower()}";
                break;
            case CardType.Heal:
                powerText.text = $"Heal {card.GetPower()}";
                break;
            default:
                powerText.text = card.GetPower().ToString();
                break;
        }

        // カード画像
        cardImage.sprite = card.GetSprite();
    }
}
