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
        view.OnCloseClicked += HandleCloseClicked;
    }

    void HandleSlotClick(int slotIndex)
    {
        currentEditingSlotIndex = slotIndex; // どのスロットを変更するか記憶

        var allMonsters = GameManager.Instance.PlayerData.ownedMonsters.Keys.ToList(); // 所持リスト取得
        var currentParty = GameManager.Instance.PlayerData.CurrentParty;

        // ここで「各モンスターの状態」を作ってViewに渡す
        // モンスターデータと選択状態のペア
        var monsterStates = new Dictionary<MonsterData, bool>();

        foreach (var monster in allMonsters)
        {
            // ルール判定はControllerで行う
            bool isSelected = currentParty.Contains(monster);
            monsterStates.Add(monster, isSelected);
        }

        view.OpenMonsterList(monsterStates); // 判定済みのデータを渡す
    }

    void HandleMonsterChange(MonsterData selectedMonster)
    {
        if (currentEditingSlotIndex == -1) return;

        // Modelを更新
        GameManager.Instance.PlayerData.SetPartyMember(currentEditingSlotIndex, selectedMonster);

        // Viewを更新（最新のパーティ情報を再取得して表示）
        var updatedParty = GameManager.Instance.PlayerData.CurrentParty;
        view.RefreshPartyView(updatedParty);

        // モンスターリストの選択状態も更新
        view.RefreshListSelection(updatedParty);
    }

    // OKボタンが押された時の処理
    void HandleCloseClicked()
    {
        view.CloseList();
        currentEditingSlotIndex = -1; // 編集モード終了
    }
}
