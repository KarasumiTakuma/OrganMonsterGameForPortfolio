using UnityEngine;
using System.Collections.Generic;

// 1ステージにおけるフェーズ(段階)単位のデータをScriptableObjectで分離
[CreateAssetMenu(fileName = "NewStagePhase", menuName = "Data/StagePhase")]
public class StagePhase : ScriptableObject
{
    [SerializeField] private List<MonsterData> enemiesList;  // このフェーズに配置する敵モンスターのリスト

    [Header("Rewards")]
    [SerializeField] private int clearRewardPoints;  // このフェーズクリア後に報酬としてもらえる研究ポイント

    // ゲッター
    public List<MonsterData> GetEnemiesList() => enemiesList;
    public int GetClearRewardPoints() => clearRewardPoints;
}