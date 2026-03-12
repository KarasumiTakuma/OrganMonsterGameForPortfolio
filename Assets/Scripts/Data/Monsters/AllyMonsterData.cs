using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 味方モンスター専用のデータクラス。
/// MonsterData を継承し、味方キャラクターにのみ必要な
/// UI表示用・スキル用の追加情報を保持する。
/// </summary>
/// 
/// <remarks>
/// ※ 現在のゲーム実装では、本クラスで定義している
/// allyDetailDescription / skillName / skillPower は
/// 実際のバトルやUI処理では使用されていない。
/// 将来的な拡張（味方スキル実装、詳細UI表示など）を想定して定義している。
/// </remarks>
[CreateAssetMenu(fileName = "AllyMonsterData", menuName = "Data/AllyMonsterData")]
public class AllyMonsterData : MonsterData
{
    /// <summary>
    ///  味方モンスターの詳細説明文。クリック時や詳細画面での表示を想定したUI用テキスト。
    /// ※ 現在のゲームでは未使用。
    /// 将来的に味方モンスターの詳細UIを実装する際に利用する想定。
    /// </summary>
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string allyDetailDescription; // クリック時に味方情報を表示する用

    /// <summary>
    /// 味方モンスターが持つスキルの名称。
    /// ※ 現在のゲームでは未使用。
    /// スキルシステム導入時に、UI表示やログ出力での使用を想定。
    /// </summary>
    [Header("スキル情報")]
    [SerializeField] private string skillName;  // 味方のスキル名

    /// <summary>
    /// スキルの効果量を表す数値。
    /// ダメージ量・回復量など、スキルの基本的な強さを示す。
    /// 
    /// ※ 現在のゲームでは未使用。
    /// 将来的にスキル処理ロジックを実装する際に使用する想定。
    /// </summary>
    [SerializeField] private int skillPower;

    /// <summary>味方モンスターの詳細説明文を取得する</summary>
    /// <returns> 味方モンスターの詳細説明文</returns>
    public string GetAllyDetailDescription() => allyDetailDescription;

    /// <summary>味方モンスターのスキル名を取得</summary>
    /// <returns>スキル名を表す文字列</returns>
    public string GetSkillName() => skillName;

    /// <summary>味方モンスターのスキル効果量を取得する</summary>
    /// <returns>スキルの効果量を表す整数値</returns>
    public int GetSkillPower() => skillPower;
}
