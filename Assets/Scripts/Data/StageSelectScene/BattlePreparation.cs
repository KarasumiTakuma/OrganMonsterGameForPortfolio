using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// バトル開始前のデータ準備を担当するクラス。
/// プレイヤーパーティの検証、味方データ変換、デッキ生成を行い、
/// BattleSessionDataへ必要な情報を登録する。
/// </summary>
public class BattlePreparation
{

    /// <summary>
    /// バトル用データの準備を試行する。
    /// パーティ検証 → 味方データ変換 → デッキ生成 の順で処理を行う。
    /// </summary>
    /// <returns>
    /// データ準備が正常に完了した場合true。
    /// パーティ不足やデッキ未構築などの失敗時はfalse。
    /// </returns>
    public static bool TryPrepareBattle()
    {
        
        /// <summary>
        /// パーティ検証処理。
        /// null でないモンスターが3体未満の場合は準備失敗。
        /// </summary>
        List<MonsterData> currentPartyList = GameManager.Instance.PlayerData.CurrentParty.ToList();
        if (currentPartyList.Count(partyMonster => partyMonster != null) < 3)
        {
            Debug.Log("パーティが3体揃っていません");
            return false;
        }

        var battleSessionData = BattleSessionData.Instance;

        List<AllyMonsterData> allyMonsters = new List<AllyMonsterData>();

        // パーティ内モンスターを AllyMonsterData に変換
        foreach (var monster in currentPartyList)
        {
            if (monster == null) continue;

            AllyMonsterData allyData;

            if (monster is AllyMonsterData allyCastData) // AllyMonsterData型にキャストする
            {
                allyData = allyCastData;
            }
            else
            {
                // キャストできなければ、MonsterDataをAllyMonsterDataに変換
                allyData = MonsterDataConverter.ToAllyMonster(monster);
            }

            allyMonsters.Add(allyData);
        }

        // BattleSessionData に味方データ登録
        battleSessionData.SetPlayerAlliesList(allyMonsters);


        List<Card> deck = new List<Card>();

        // 味方ごとのカード情報からデッキを構築
        foreach (var ally in allyMonsters)
        {
            if (ally.cards == null) continue;

            foreach (var cardData in ally.cards)
            {
                if (cardData != null)
                    deck.Add(new Card(cardData)); // カードをデッキとして一時的なリストに追加
            }
        }

        if (deck.Count == 0)
        {
            Debug.LogError("カードが1枚もありません");
            return false;
        }

        // BattleSessionData にデッキを登録
        battleSessionData.SetPlayerDeckList(deck);

        return true;
    }

}