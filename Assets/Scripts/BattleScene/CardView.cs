// CardView.csは、HandAreaManager.csからスポーンした手札カードの情報を受け取って、
// 手札カードの見た目(UI：消費マナや攻撃量やカード名のテキスト、画像)を更新する役割を担う。

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

        // 効果量をカードタイプに応じて表示（攻撃ならAck、回復ならHealなど）
        switch (card.GetCardEffectType())
        {
            case CardEffectType.AttackToSelected:
                powerText.text = $"攻撃 {card.GetPower()}";
                break;

            case CardEffectType.AttackToAll:
                powerText.text = $"全攻 {card.GetPower()}";
                break;

            case CardEffectType.Heal:
                powerText.text = $"回復 {card.GetPower()}";
                break;

            case CardEffectType.DamageOverTime:
                powerText.text = $"継続ダメージ {card.GetPower()} / {card.GetDurationTurn()}ターン";
                break;

            case CardEffectType.HealOverTime:
                powerText.text = $"継続回復 {card.GetPower()} / {card.GetDurationTurn()}ターン";
                break;

            default:
                powerText.text = card.GetPower().ToString();
                break;
        }

        // カード画像
        cardImage.sprite = card.GetSprite();
    }
}
