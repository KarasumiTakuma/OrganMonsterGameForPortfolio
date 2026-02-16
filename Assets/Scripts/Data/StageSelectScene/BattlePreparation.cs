using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

// BattleSessionDataに戦闘前のデータ(プレイヤーパーティとカード情報)を準備するクラス

public class BattlePreparation : MonoBehaviour
{

    // 戦闘前データを準備する処理を外部から呼び出すメソッド。返り値がtrueなら準備できたことを示す。
    public static bool TryPrepareBattle()
    {

        // 現在のプレイヤーパーティリストを取得し、味方モンスターが3体揃っているかを確認(非nullの数でカウント)
        List<MonsterData> currentPartyList = GameManager.Instance.PlayerData.CurrentParty.ToList();
        if (currentPartyList.Count(partyMonster => partyMonster != null) < 3)
        {
            Debug.Log("パーティが3体揃っていません");
            return false;
        }

        var battleSessionData = BattleSessionData.Instance;

        // プレイヤーパーティのMonsterData型オブジェクト1体1体に対して、
        // AllyMonsterData型にキャストまたは変換した後、まとめて味方リストに味方データとしてを追加する
        List<AllyMonsterData> allyMonsters = new List<AllyMonsterData>();
        foreach (var monster in currentPartyList)
        {
            if (monster == null) continue;

            AllyMonsterData ally;

            if (monster is AllyMonsterData existingAlly) // AllyMonsterData型にキャストする
            {
                ally = existingAlly;
            }
            else //キャストできなければ
            {
                // MonsterDataをAllyMonsterDataに変換
                ally = MonsterDataConverter.ToAllyMonster(monster);
            }

            allyMonsters.Add(ally);
        }
        // BattleSessionData の味方リストに味方データを追加する
        battleSessionData.SetPlayerAlliesList(allyMonsters);


        // 味方データからカードデッキを生成
        List<Card> deck = new List<Card>();
        foreach (var ally in allyMonsters) // 味方モンスターそれぞれに対して
        {
            if (ally.cards == null) continue;  // カード情報がセットされていなければ、次のモンスターへ処理を移す

            foreach (var cardData in ally.cards) // 味方モンスター1体が持つカードリスト内の複数カード(10枚分)を取得していく
            {
                if (cardData != null) // カードデータが空でなければ
                    deck.Add(new Card(cardData)); // そのカードをデッキとして一時的なリストに追加
            }
        }

        if (deck.Count == 0)
        {
            Debug.LogError("カードが1枚もありません");
            return false;
        }

        // プレイヤーが使うカードの山札情報としてBattleSessionDataにセットしておく
        battleSessionData.SetPlayerCardList(deck);

        return true;
    }

}