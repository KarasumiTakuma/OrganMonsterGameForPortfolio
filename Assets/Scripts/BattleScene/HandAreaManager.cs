using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandAreaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform[] cardSlots; // 各スロット(5枚のカードを配置する場所)を個別に指定

    [Header("Visual Control")]
    [SerializeField] private CanvasGroup canvasGroup;  // CanvasGroupは、子オブジェクトを含めたUI全体を制御するためのもの

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private List<Card> handCardData;

    public void setHandCardData(List<Card> handCardData)
    {
        this.handCardData = handCardData;
    }

    // 手札UIを更新するメソッド。
    // 引数 onCardPlayed は「カードがプレイされたときに呼ばれる処理をするメソッド」を格納するデリゲート。
    // onCardPlayedに登録されるメソッドは、第一引数をCard型、
    // 第二引数をVector2型(カードのドロップ位置)
    // 第三引数をAction<bool>型(bool型を引数とする、返り値void)のメソッド
    // とする、返り値voidのメソッドである。
    public void UpdateHandUI(System.Action<Card, Vector2, System.Action<bool>> onCardPlayed, EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager)
    {
        // 1. 既存UI削除
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();

        // 2. 各スロットの位置に応じてカード生成

        int dataIndex = 0;
        foreach (var slotPoint in cardSlots)
        {
            // カードスロット5つに対して手札カードが4枚以下の場合に、ないはずの5枚目のデータ(空値)にアクセスしないようにする
            // 手札枚数よりスロット数が多い場合は生成しない
            if (dataIndex >= handCardData.Count) break;

            // cardObjはそのスポーン位置(slotPoint)を親とする
            GameObject cardObj = Instantiate(cardPrefab, slotPoint);

            // cardObjオブジェとそのスポーンポイント(slotPoint)のRectTransformを取得
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            RectTransform slotRect = slotPoint.GetComponent<RectTransform>();

            if (cardRect != null && slotRect != null) //RectTransformが空値でないかを確認
            {
                //Inspector上で各オブジェクトのサイズ設定を誤っても正しいサイズにするため

                // card(手札カード)オブジェクトのサイズをslotPointに合わせる
                cardRect.sizeDelta = slotRect.sizeDelta;

                // 各手札カードオブジェクトの位置をそれぞれの親であるslotPointのpivot位置(中心)にする
                cardRect.anchoredPosition = Vector2.zero;

                // 手札カードのサイズは1に固定
                cardRect.localScale = Vector3.one;
            }

            Card cardData = handCardData[dataIndex];

            // カードデータをCardViewにセット
            CardView cardView = cardObj.GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.SetCard(cardData);
            }

            var dragDrop = cardObj.GetComponent<CardUI_DragDrop>();
            if (dragDrop != null && onCardPlayed != null)
            {
                dragDrop.Setup(cardData, enemyAreaManager, allyAreaManager);

                // 前回登録済みのイベントをクリア（同じイベントの重複防止）
                dragDrop.ClearOnCardPlayed();
                // イベント変数にイベントを登録
                // onCardPlayed変数には、UpdateHandUI()メソッドが呼び出された際に
                // onCardPlayedに登録されるメソッドは、第一引数をCard型、
                // 第二引数をVector2型
                // 第三引数をAction<bool>型のメソッドとするメソッド群が登録されている。
                // onCardPlayedにあるメソッド群を、dragDrop.OnCardPlayedに登録する
                dragDrop.OnCardPlayed += (card, dropPosition, callback) => onCardPlayed?.Invoke(card, dropPosition, callback);
            }

            spawnedCards.Add(cardObj);
            dataIndex++;
        }
    }
    
    // 手札エリア(および手札)の表示/非表示を外部から制御するためのメソッド
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // 手札の操作可能/不可能 状態の切り替え処理を外部から制御するためのメソッド
    public void SetInteractable(bool interactable)
    {
        if (canvasGroup == null) return;

        // 手札が操作可能状態なら、手札の明るさを1(通常)に、
        // 不可能状態なら、手札を少し暗くして、
        // 操作不可能(interactable = false)と
        // クリック不可状態(blocksRaycasts = false)にする
        canvasGroup.alpha = interactable ? 1.0f : 0.4f;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    // 他クラスから生成したカードUIリスト取得
    public List<GameObject> GetSpawnedCards() => spawnedCards;
}
