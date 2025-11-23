using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyEditController : MonoBehaviour
{
    [SerializeField] private MonsterSelectUI view;

    private int currentEditingSlotIndex = -1; // 変更中のスロット番号

    void Start()
    {
        // viewの初期化
        IReadOnlyList<MonsterData> currentParty = GameManager.Instance.PlayerData.CurrentParty;
        view.RefreshPartyView(currentParty); // 初期表示

        // イベント登録
        view.OnPartySlotClicked += HandleSlotClick;
        view.OnMonsterSelected += HandleMonsterChange;
    }

    void HandleSlotClick(int slotIndex)
    {
        currentEditingSlotIndex = slotIndex; // どのスロットを変更するか記憶

        var allMonsters = GameManager.Instance.PlayerData.ownedMonsters.Keys.ToList(); // 所持リスト取得
        var currentParty = GameManager.Instance.PlayerData.CurrentParty;

        view.OpenMonsterList(allMonsters, currentParty); // 一覧を開かせる
    }

    void HandleMonsterChange(MonsterData selectedMonster)
    {
        if (currentEditingSlotIndex == -1) return;

        // Modelを更新
        GameManager.Instance.PlayerData.SetPartyMember(currentEditingSlotIndex, selectedMonster);

        // Viewを更新（最新のパーティ情報を再取得して表示）
        var updatedParty = GameManager.Instance.PlayerData.CurrentParty;
        view.RefreshPartyView(updatedParty);

        // 状態リセット
        currentEditingSlotIndex = -1;
    }
}
