using UnityEngine;
using System.Collections.Generic;

// Unityのメニューから作成可能にする
[CreateAssetMenu(fileName = "AllyMonsterData", menuName = "Data/AllyMonsterData")]
public class AllyMonsterData : MonsterDataBattleScene
{
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string description; // クリック時に味方情報を表示する用

    // descriptionを外部から取得するためのゲッター
    public string GetDescription() => description;

    [Header("ゲーム用追加情報")]
    [SerializeField] private string skillName;  // 味方のスキル名
    [SerializeField] private int skillPower;    // スキルの効果量

    public string GetSkillName() => skillName;
    public int GetSkillPower() => skillPower;
}
