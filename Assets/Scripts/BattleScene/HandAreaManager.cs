using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandAreaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;
    [SerializeField] private BattleManager battleManager; // カードプレイ処理に必要

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    public void UpdateHandUI()
    {
        // 1. 既存UI削除
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();

        // 2. 新規UI生成
        foreach (var cardData in deckManager.GetHand())
        {
            GameObject cardObj = Instantiate(cardPrefab, handArea);

            // カード名
            // ? は null 条件演算子。参照がnullじゃなければアクセスして、nullなら処理をスキップする
            // TMP_Text型の構造体の.text変数にテキストを代入すると、それをUI表示してくれる
            TMP_Text nameText = cardObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (nameText) nameText.text = cardData.GetName();

            // マナコスト
            TMP_Text manaText = cardObj.transform.Find("ManaCostText")?.GetComponent<TMP_Text>();
            if (manaText) manaText.text = "Cost：" + cardData.GetManaCost().ToString();

            // 攻撃力
            TMP_Text powerText = cardObj.transform.Find("PowerText")?.GetComponent<TMP_Text>();
            if (powerText) powerText.text = "Att" + cardData.GetPower().ToString();

            // 画像
            Image cardImage = cardObj.transform.Find("CardImage")?.GetComponent<Image>();
            if (cardImage) cardImage.sprite = cardData.GetSprite();

            // CardUI_DragDropをセットアップ
            var dragDrop = cardObj.GetComponent<CardUI_DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.Setup(cardData, battleManager);
            }

            spawnedCards.Add(cardObj);
        }
    }
}
