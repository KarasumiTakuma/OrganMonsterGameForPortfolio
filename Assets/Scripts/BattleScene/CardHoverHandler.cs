using UnityEngine;
using UnityEngine.EventSystems;

// カードへのHoverを検知し、それに応じた処理(カードの説明パネルの表示・非表示)をするクラス
// CardPrefabにアタッチする
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Card card;
    private CardDescriptionPanel cardDescriptionPanel;

    public void Setup(Card card, CardDescriptionPanel descriptionPanel)
    {
        this.card = card;
        this.cardDescriptionPanel = descriptionPanel;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (card == null) return;

        Vector3 cardPosition = transform.position;

        cardDescriptionPanel.ShowDescriptionPanel(card.GetDescription(), cardPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardDescriptionPanel.HideDescriptionPanel();
    }
}