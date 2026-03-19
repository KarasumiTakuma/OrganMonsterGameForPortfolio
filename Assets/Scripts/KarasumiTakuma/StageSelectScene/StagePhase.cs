using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ステージ内の単一フェーズ(戦闘段階)を定義するデータクラス。
/// 出現する敵モンスター構成およびクリア報酬を保持する。
/// ScriptableObject として作成。
/// </summary>
[CreateAssetMenu(fileName = "NewStagePhase", menuName = "Data/StagePhase")]
public class StagePhase : ScriptableObject
{
    /// <summary> このフェーズで出現する敵モンスターの定義リスト</summary>
    [SerializeField] private List<MonsterData> enemiesList;

    [Header("Rewards")]
    /// <summary> フェーズクリア時にプレイヤーへ付与される報酬ポイント</summary>
    [SerializeField] private int clearRewardPoints;

    [Header("Fireball Effects")]

    /// <summary>Fireballの効果によって敵全体に与えられるダメージ量</summary>
    [SerializeField] private int fireballDamageToEnemyAmount;

    /// <summary>Fireballの効果によって味方に与えられるダメージ量</summary>
    [SerializeField] private int fireballDamageToAllyAmount;

    /// <summary>Fireballの効果によって味方が回復する量</summary>
    [SerializeField] private int fireballHealToAllyAmount;

    /// <summary> このフェーズに設定された敵モンスターリストを取得</summary>
    /// <returns>敵モンスターデータのリスト</returns>
    public List<MonsterData> GetEnemiesList() => enemiesList;

    /// <summary> フェーズクリア報酬ポイントを取得する</summary>
    /// <returns>クリア報酬ポイント</returns>
    public int GetClearRewardPoints() => clearRewardPoints;

    /// <summary>Fireballの効果による敵ダメージ量を取得</summary>
    public int GetFireballDamageToEnemyAmount() => fireballDamageToEnemyAmount;

    /// <summary>Fireballの効果による味方ダメージ量を取得</summary>
    public int GetFireballDamageToAllyAmount() => fireballDamageToAllyAmount;

    /// <summary>Fireballの効果による回復量を取得</summary>
    public int GetFireballHealToAllyAmount() => fireballHealToAllyAmount;
}