using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandAreaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform[] cardSlots; // 各スロット(5枚のカードを配置する場所)を個別に指定

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private List<Card> handCardData;

    public void setHandCardData(List<Card> handCardData)
    {
        this.handCardData = handCardData;
    }

    // 手札UIを更新するメソッド。
    // 引数 onCardPlayed は「カードがプレイされたときに呼ばれる処理をするメソッド」を格納するデリゲート。
    // onCardPlayedに登録されるメソッドは、第一引数をCard型、
    // 第二引数をAction<bool>型(bool型を引数とする、返り値void)のメソッド
    // とする、返り値voidのメソッドである。
    public void UpdateHandUI(System.Action<Card, System.Action<bool>> onCardPlayed)
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
                dragDrop.Setup(cardData);

                // 前回登録済みのイベントをクリア（同じイベントの重複防止）
                dragDrop.ClearOnCardPlayed();
                // イベント変数にイベントを登録
                // onCardPlayed変数には、UpdateHandUI()メソッドが呼び出された際に
                // onCardPlayedに登録されるメソッドは、第一引数をCard型、
                // 第二引数をAction<bool>型のメソッドとするメソッド群が登録されている。
                // onCardPlayedにあるメソッド群を、dragDrop.OnCardPlayedに登録する
                dragDrop.OnCardPlayed += (card, callback) => onCardPlayed?.Invoke(card, callback);
            }

            spawnedCards.Add(cardObj);
            dataIndex++;
        }
    }
    // 他クラスから生成したカードUIリスト取得
    public List<GameObject> GetSpawnedCards() => spawnedCards;
}
