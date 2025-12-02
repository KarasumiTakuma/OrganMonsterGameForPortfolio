using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private StageInfo stageData;
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    public void SetStage(StageInfo stage)
    {
        stageData = stage;
    }

    private void OnButtonClick()
    {
        var session = BattleSessionData.Instance;
        if (stageData == null)
        {
            Debug.LogError("StageSelectButton: stageData がセットされていません");
            return;
        }

        session.currentStage = stageData;

        List<MonsterData> party = GameManager.Instance.PlayerData.CurrentParty.ToList();
        if (party == null || party.Count == 0)
        {
            Debug.LogError("StageSelectButton: CurrentParty が null または空です");
            return;
        }

        // AllyMonsterData にキャストできるものはそのまま、できないものは変換
        session.playerAllies = new List<AllyMonsterData>();
        foreach (var monster in party)
        {
            AllyMonsterData ally;
            if (monster is AllyMonsterData existingAlly)
            {
                ally = existingAlly;
            }
            else
            {
                // MonsterData を AllyMonsterData に変換
                ally = MonsterDataConverter.ToAllyMonster(monster);
                Debug.Log($"StageSelectButton: {monster.GetName()} を AllyMonsterData に変換しました");
            }
            session.playerAllies.Add(ally);
        }


        // デッキ生成
        List<Card> deck = new List<Card>();
        foreach (var ally in session.playerAllies)
        {
            if (ally.cards == null) continue;
            foreach (var cardData in ally.cards)
            {
                if (cardData != null)
                    deck.Add(new Card(cardData));
            }
        }
        session.playerCards = deck;

        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

}
