using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// パーティ編成画面のControllerクラス
/// </summary>
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
        view.OnBackButtonClicked += GameManager.Instance.GoToLab;
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

        // 現在のパーティ情報を取得
        var currentParty = GameManager.Instance.PlayerData.CurrentParty;

        // 1. パーティ全体を調べて、既に同じモンスターがいるか探す
        int existingSlotIndex = -1; // 見つからなかったら -1

        for (int i = 0; i < currentParty.Count; i++)
        {
            // 空のスロットはスキップ
            if (currentParty[i] == null) continue;

            // IDで比較して、同じモンスターか確認
            if (currentParty[i].GetID() == selectedMonster.GetID())
            {
                existingSlotIndex = i; // 「i番目のスロットに既にいた！」と記録
                break; // 見つかったらループ終了
            }
        }

        // 2. 状況に応じて処理を分岐

        // パターンA: 「今編集中のスロット」に既にそのモンスターがいる
        // → つまり、自分で自分を選んだ状態（選択解除）
        if (existingSlotIndex == currentEditingSlotIndex)
        {
            GameManager.Instance.PlayerData.SetPartyMember(currentEditingSlotIndex, null);
            Debug.Log("選択解除しました");
        }
        // パターンB: 「別のスロット」に既にそのモンスターがいる
        // → モンスターを移動させる（元の場所を消して、今の場所に入れる）
        else if (existingSlotIndex != -1)
        {
            // 先に元の場所を空にする
            GameManager.Instance.PlayerData.SetPartyMember(existingSlotIndex, null);

            // 新しい場所にセットする
            GameManager.Instance.PlayerData.SetPartyMember(currentEditingSlotIndex, selectedMonster);

            Debug.Log($"スロット{existingSlotIndex} から スロット{currentEditingSlotIndex} へ移動しました");
        }
        // パターンC: パーティにまだいない（新規）
        // → そのままセットする
        else
        {
            GameManager.Instance.PlayerData.SetPartyMember(currentEditingSlotIndex, selectedMonster);
            Debug.Log("新規セットしました");
        }

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
