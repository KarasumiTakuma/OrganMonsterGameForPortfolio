using System.Collections.Generic;
using UnityEngine;

// 1つのステージに関する情報をまとめたデータクラス。ScriptableObjectで作成。
[CreateAssetMenu(fileName = "StageInfo", menuName = "Data/StageInfo")]
public class StageInfo : ScriptableObject
{
    [SerializeField] private int stageID; // ステージを識別するためのID
    [SerializeField] private string stageName;  // ステージ名

    [Header("フェーズ構成")]
    [SerializeField] private List<StagePhase> stagePhases;  // このステージの各フェーズ 

    // ゲッターメソッド
    public List<StagePhase> GetStagePhases() => stagePhases;
    public string GetStageName() => stageName;
    public int GetStageID() => stageID;
}