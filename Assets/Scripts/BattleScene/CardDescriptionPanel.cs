using UnityEngine;
using TMPro;

/// <summary>
/// カードの詳細説明を表示するためのUIパネルを制御するクラス。
/// カードにホバーした際などに、
/// 説明文を設定し、カード位置を基準にパネルを表示する役割を持つ。
/// </summary>
public class CardDescriptionPanel : MonoBehaviour
{
    /// <summary>カードの説明文を表示する TextMeshPro テキスト</summary>
    [SerializeField] private TMP_Text cardDescriptionText;

    /// <summary>カードの位置を基準としたパネル表示位置のオフセット。通常はカードより少し上に表示する目的で使用</summary>
    [SerializeField] private Vector2 offsetFromCard = new Vector2(0, 50f);
    
    /// <summary>このパネル自身の RectTransform。anchoredPosition を操作するために保持。</summary>
    private RectTransform rectTransform;


    /// <summary>
    /// RectTransform を確実に取得するための補助メソッド。
    /// 未取得の場合のみ GetComponent を行う。
    /// </summary>
    private void EnsureRectTransform()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// カードの説明文を設定し、カード位置を基準に
    /// 説明パネルを表示する。
    /// </summary>
    /// <param name="cardDescription">表示するカードの詳細説明文。</param>
    /// <param name="cardWorldPosition">カードのワールド座標。パネルはこの位置を基準に表示される。</param>
    public void ShowDescriptionPanel(string cardDescription, Vector3 cardWorldPosition)
    {
        // RectTransform が未取得の場合は取得する
        EnsureRectTransform();

        // 説明文を設定
        cardDescriptionText.text = cardDescription;

        // パネルを表示
        gameObject.SetActive(true);

        // パネルのワールド座標をカード位置に合わせる
        transform.position = cardWorldPosition;

        // カード位置からのオフセット分だけローカル座標をずらす
        rectTransform.anchoredPosition += offsetFromCard;
    }

    /// <summary>
    /// カード説明パネルを非表示にする。
    /// </summary>
    public void HideDescriptionPanel()
    {
        gameObject.SetActive(false);
    }
}