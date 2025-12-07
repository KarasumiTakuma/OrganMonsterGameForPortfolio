using System.Collections.Generic;
using UnityEngine;

// 1つのステージに関する情報をまとめたデータクラス。ScriptableObjectで作成。

[CreateAssetMenu(fileName = "StageInfo", menuName = "Data/StageInfo")]
public class StageInfo : ScriptableObject
{
    [SerializeField] private int stageID; // ステージを識別するためのID
    [SerializeField] private string stageName;  // ステージ名
    [SerializeField] private List<MonsterData> enemies; // ステージ固有の敵モンスターのリスト

    // ゲッターメソッド
    public List<MonsterData> GetEnemiesList() => enemies;
    public string GetStageName() => stageName;
    public int GetStageID() => stageID;
    
}
