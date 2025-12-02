using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "StageInfo", menuName = "Data/StageInfo")]
public class StageInfo : ScriptableObject
{
    public int stageID; // ステージ番号
    public string stageName;
    public List<MonsterData> enemies; // ステージ固有の敵モンスター
}
