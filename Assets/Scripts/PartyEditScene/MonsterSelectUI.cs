using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
///　モンスター選択UIの管理クラス（viewの管理）
/// </summary>
public class MonsterSelectUI : MonoBehaviour
{
    [Header("UIパネル")]
    // パーティ編成パネル
    [SerializeField] private GameObject partyEditPanel;
    // 選択用ポップアップパネル
    [SerializeField] private GameObject memberListPanel;
    [Header("Party Slots")]
    [SerializeField] private List<PartySlotUI> partySlots;

    [Header("Monster List")]
    [SerializeField] private Transform listContent; // ScrollViewのContent
    [SerializeField] private GameObject listSlotPrefab; // MonsterSelectSlotUIのプレハブ
    [SerializeField] private Button closeListButton;

    [SerializeField] private Button goBackLabButton;

    // Controllerに伝えるイベント
    public event Action<int> OnPartySlotClicked; // どのスロットの枠を変えるか
    public event Action<MonsterData> OnMonsterSelected;  // 「こいつに変える」
    public event Action OnBackButtonClicked;             // 「戻る」

    void Start()
    {
        // 各スロットのイベントを中継する
        foreach (var slot in partySlots)
        {
            slot.OnClick += index => OnPartySlotClicked?.Invoke(index);
        }

        closeListButton.onClick.AddListener(CloseList);
        goBackLabButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());

        memberListPanel.SetActive(false);
    }

    // --- Controllerから呼ばれるメソッド ---

    // 1. 現在のパーティを表示更新する
    public void RefreshPartyView(List<MonsterData> currentParty)
    {
        for (int i = 0; i < partySlots.Count; i++)
        {
            // パーティデータがあればそれを、なければnullを渡して表示更新
            if (i < currentParty.Count)
                partySlots[i].Setup(currentParty[i]);
            else
                partySlots[i].Setup(null);
        }
    }

    // 2. 選択用リストを表示する
    public void OpenMonsterList(List<MonsterData> allMonsters, List<MonsterData> currentParty)
    {
        memberListPanel.SetActive(true);

        // 既存の中身をクリア
        foreach (Transform child in listContent) Destroy(child.gameObject);

        // リストを生成
        foreach (var monster in allMonsters)
        {
            // プレハブを生成し、gridContentの子にする
            GameObject obj = Instantiate(listSlotPrefab, listContent);
            // スロットのスクリプトを取得
            MonsterSelectSlotUI slot = obj.GetComponent<MonsterSelectSlotUI>();

            // 既にパーティにいるかチェック
            bool isAlreadyInParty = currentParty.Contains(monster);
            // スロットにモンスターをセットアップ(isAlreadyInPartyで有効化、無効化をMonsterSelectSlotUIが決定)
            slot.Setup(monster, isAlreadyInParty);

            // クリックされたらイベントを通知
            slot.OnSelected += (selectedMonster) =>
            {
                OnMonsterSelected?.Invoke(selectedMonster);
                //CloseList();
            };
        }
    }

    public void CloseList()
    {
        memberListPanel.SetActive(false);
    }
}
