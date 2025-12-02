using System.Collections.Generic;
using UnityEngine;

public class BattleSessionData : MonoBehaviour
{
    public static BattleSessionData Instance { get; private set; }

    public StageInfo currentStage;
    public List<AllyMonsterData> playerAllies; // 選択中の味方パーティ3体
    public List<Card> playerCards; // 味方パーティの手札情報

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearData()
    {
        currentStage = null;
        playerAllies = null;
        playerCards = null;
    }
}
