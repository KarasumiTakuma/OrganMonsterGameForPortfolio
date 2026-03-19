// CardView.csは、HandAreaManager.csからスポーンした手札カードの情報を受け取って、
// 手札カードの見た目(UI：消費マナや攻撃量やカード名のテキスト、画像)を更新する役割を担う。

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 手札に表示されるカード1枚分のUI表示を担当するクラス。
/// HandAreaManager から生成されたカードに対応し、
/// カード名・マナコスト・効果量・カード画像などの
/// 見た目(UI)を更新する責務を持つ。
/// ゲームロジックは持たず、表示専用(View)として機能する。
/// </summary>
public class CardView : MonoBehaviour
{
    /// <summary>カード名を表示するTextMeshProテキスト</summary>
    [SerializeField] private TMP_Text nameText;

    /// <summary>マナコストを表示するTextMeshProテキスト</summary>
    [SerializeField] private TMP_Text manaCostText;

    /// <summary>カード画像を表示するImageコンポーネント</summary>
    [SerializeField] private Image cardImage;

    /// <summary>
    /// カード効果の種類を表示する TextMeshPro テキスト。
    /// </summary>
    [SerializeField] private TMP_Text cardEffectTypeText;

    /// <summary>このCardViewが表示対象として保持しているカードモデル</summary>
    private Card card;

    /// <summary>
    /// 表示対象となるカードを設定し、UIを更新する。
    /// HandAreaManager などから生成直後に呼び出される。
    /// </summary>
    /// <param name="cardData">表示対象となるカードモデル</param>
    public void SetCard(Card cardData)
    {
        card = cardData;
        UpdateUI();
    }

    /// <summary>
    /// 現在保持しているカード情報を使って表示を再更新する。
    /// </summary>
    public void Refresh()
    {
        UpdateUI();
    }

    /// <summary>
    /// 現在設定されているカード情報をもとに、
    /// カードUI（名前・マナ・効果量・画像）を更新する。
    /// </summary>
    private void UpdateUI()
    {
        if (card == null) return;

        nameText.text = card.GetName();

        manaCostText.text = $"Cost : {card.GetManaCost()}";

        // カード効果タイプに応じて効果量テキストを切り替える
        switch (card.GetCardEffectType())
        {
            case CardEffectType.AttackToSelected:
                cardEffectTypeText.text = $"攻撃";
                break;

            case CardEffectType.AttackToAll:
                cardEffectTypeText.text = $"全体攻撃";
                break;

            case CardEffectType.Heal:
                cardEffectTypeText.text = $"回復";
                break;

            case CardEffectType.DamageOverTime:
                cardEffectTypeText.text = $"継続ダメージ";
                break;

            case CardEffectType.HealOverTime:
                cardEffectTypeText.text = $"継続回復";
                break;

            default:
                cardEffectTypeText.text = $"{card.GetCardEffectType()}";
                break;
        }

        cardImage.sprite = card.GetSprite();
    }
}
