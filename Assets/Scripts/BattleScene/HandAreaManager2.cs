using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandAreaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform[] cardSlots; // ← 各スロットを個別に指定
    [SerializeField] private BattleManager battleManager;

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
        var hand = deckManager.GetHand();
        for (int i = 0; i < hand.Count && i < cardSlots.Length; i++)
        {
            var cardData = hand[i];
            Transform slot = cardSlots[i];

            GameObject cardObj = Instantiate(cardPrefab, slot);

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
        }
    }
}
