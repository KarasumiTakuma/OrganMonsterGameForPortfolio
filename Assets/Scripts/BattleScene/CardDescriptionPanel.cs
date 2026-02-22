using UnityEngine;
using TMPro;

public class CardDescriptionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private Vector2 panelOffset = new Vector2(0, 50f);  // // カード位置からのオフセット
    
    private RectTransform rectTransform;

    // RectTransformを確実に取得する。
    private void EnsureRectTransform()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    // カードの詳細説明(cardDescription)とパネルの位置を決め、表示するメソッド
    public void ShowDescriptionPanel(string cardDescription, Vector3 basePosition)
    {
        EnsureRectTransform();  // RectTransformが入手できていなければ、取得する。

        cardDescriptionText.text = cardDescription;
        
        // パネルを表示
        gameObject.SetActive(true);

        // パネルのワールド座標をカードの位置に合わせる
        // カードの場所(ワールド座標における)を、そのままパネルのワールド位置として代入
        transform.position = basePosition;
        
        // オフセット分だけ、RectTransformのローカル座標でずらす
        rectTransform.anchoredPosition += panelOffset;  // 元のカードの位置から少し上を説明パネル位置とする
    }

    // パネルを非表示にするメソッド
    public void HideDescriptionPanel()
    {
        gameObject.SetActive(false);
    }
}