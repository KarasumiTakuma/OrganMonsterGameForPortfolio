using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// カードへのポインターホバーを検知し、
/// カード説明パネルの表示・非表示を制御するクラス。
/// CardPrefab にアタッチして使用され、
/// UI 上でのカード操作（マウスオーバー）に反応する。
/// </summary>
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>このハンドラが対象とするカード情報</summary>
    private Card card;

    /// <summary>カードの説明文を表示するための UI パネル。ホバー時に表示、ホバー解除時に非表示にされる</summary>
    private CardDescriptionPanel cardDescriptionPanel;

    /// <summary>
    /// このハンドラの初期設定を行う。
    /// 対象カードと、表示に使用する説明パネルを外部から受け取る。
    /// </summary>
    /// <param name="card">この CardHoverHandler が扱うカードの情報。</param>
    /// <param name="descriptionPanel">カード説明を表示するための UI パネル。ホバー時に操作される。</param>
    public void Setup(Card card, CardDescriptionPanel descriptionPanel)
    {
        this.card = card;
        this.cardDescriptionPanel = descriptionPanel;
    }

    /// <summary>
    /// ポインターがカード上に乗った瞬間に呼び出される。
    /// カードの説明パネルを、カード位置を基準に表示する。
    /// </summary>
    /// <param name="eventData">ポインターイベントの情報。 本処理では参照していないが、UIイベント仕様上必要。</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (card == null) return;

        // カードのワールド座標を取得
        Vector3 cardPosition = transform.position;

        // 説明パネルにカード説明文を設定し、表示する
        cardDescriptionPanel.ShowDescriptionPanel(card.GetDescription(), cardPosition);
    }

    /// <summary>
    /// ポインターがカードから離れた瞬間に呼び出される。
    /// 表示中のカード説明パネルを非表示にする。
    /// </summary>
    /// <param name="eventData"> ポインターイベントの情報。本処理では参照していないが、UIイベント仕様上必要。</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardDescriptionPanel == null) return;
        
        // 説明パネルを非表示にする
        cardDescriptionPanel.HideDescriptionPanel();
    }
}