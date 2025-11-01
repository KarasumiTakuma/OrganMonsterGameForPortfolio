using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandAreaManager2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform[] cardSlots; // ← 各スロット(5枚のカードを配置する場所)を個別に指定

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    public void UpdateHandUI()
    {
        // 1. 既存UI削除
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();

        // 2. スロットに応じてカード生成
        List<Card> handCardData = deckManager.GetHand();
        int dataIndex = 0;
        foreach (var slotPoint in cardSlots)
        {
            // カードスロット5つに対して手札カードが4枚以下の場合に、ないはずの5枚目のデータ(空値)にアクセスしないようにする
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
            
            TMP_Text nameText = cardObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (nameText) nameText.text = cardData.GetName();

            TMP_Text manaText = cardObj.transform.Find("ManaCostText")?.GetComponent<TMP_Text>();
            if (manaText) manaText.text = "Cost：" + cardData.GetManaCost();

            TMP_Text powerText = cardObj.transform.Find("PowerText")?.GetComponent<TMP_Text>();
            if (powerText) powerText.text = "Att" + cardData.GetPower();

            Image cardImage = cardObj.transform.Find("CardImage")?.GetComponent<Image>();
            if (cardImage) cardImage.sprite = cardData.GetSprite();

            var dragDrop = cardObj.GetComponent<CardUI_DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.Setup(cardData, battleManager);
            }

            spawnedCards.Add(cardObj);
            dataIndex++;
        }
    }
}
