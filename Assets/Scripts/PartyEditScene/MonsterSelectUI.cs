using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

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
    public event Action OnCloseClicked;

    void Start()
    {
        // 各スロットのイベントを中継する
        foreach (var slot in partySlots)
        {
            slot.OnClick += index => OnPartySlotClicked?.Invoke(index);
        }

        closeListButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        goBackLabButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());

        memberListPanel.SetActive(false);
    }

    // --- Controllerから呼ばれるメソッド ---

    // 現在のパーティを表示更新する
    public void RefreshPartyView(IReadOnlyList<MonsterData> currentParty)
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

    /// <summary>
    /// モンスターリストを開く
    /// </summary>
    /// <param name="monsterStates">モンスターデータとパーティにいるかの辞書</param>
    public void OpenMonsterList(Dictionary<MonsterData, bool> monsterStates)
    {
        memberListPanel.SetActive(true);

        // 既存の中身をクリア
        foreach (Transform child in listContent) Destroy(child.gameObject);
        var sortedList = monsterStates.OrderBy(entry => entry.Key.GetID()).ToList();

        // リストを生成
        foreach (var entory in sortedList)
        {
            MonsterData monster = entory.Key;
            bool isAlreadyInParty = entory.Value;

            // プレハブを生成し、gridContentの子にする
            GameObject obj = Instantiate(listSlotPrefab, listContent);
            // スロットのスクリプトを取得
            MonsterSelectSlotUI slot = obj.GetComponent<MonsterSelectSlotUI>();
            // スロットをセットアップ
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

    public void RefreshListSelection(IReadOnlyList<MonsterData> currentParty)
    {
        // リスト内の各スロットを確認し、選択状態を更新する
        foreach (Transform child in listContent)
        {
            MonsterSelectSlotUI slot = child.GetComponent<MonsterSelectSlotUI>();
            if (slot != null)
            {
                // スロットが持っているモンスターデータを取得
                MonsterData data = slot.GetMonsterData();
                // そのモンスターが現在のパーティに含まれているか確認
                bool isInParty = currentParty.Contains(data);
                // スロットの表示を更新
                slot.Setup(data, isInParty);
            }
        }
    }
}
