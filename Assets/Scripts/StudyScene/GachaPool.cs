using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewGachaPool", menuName = "Data/Gacha Pool")]
public class GachaPool : ScriptableObject
{
    public int rarity; // 1~5
    public int probability; // このプールが選ばれる確率 (例: 70%)

    // このプールから排出されるアイテムのリスト
    public List<OrganData> items; 
}