using UnityEngine;
using System.Collections.Generic;

// Unityのメニューから作成可能にする
[CreateAssetMenu(fileName = "AllyMonsterData", menuName = "Data/AllyMonsterData")]
public class AllyMonsterData : MonsterData
{
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string allyDescription; // クリック時に味方情報を表示する用
    

    [Header("ゲーム用追加情報")]
    [SerializeField] private string skillName;  // 味方のスキル名
    [SerializeField] private int skillPower;    // スキルの効果量

    public string GetSkillName() => skillName;
    public int GetSkillPower() => skillPower;

    public string GetAllyDescription() => allyDescription;  // descriptionを外部から取得するためのゲッター
}
