using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1つのステージに関する情報を保持するデータクラス。
/// ステージ名・識別ID・フェーズ構成など、ステージ全体の定義を管理する。
/// ScriptableObject として作成。
/// </summary>
[CreateAssetMenu(fileName = "StageInfo", menuName = "Data/StageInfo")]
public class StageInfo : ScriptableObject
{
    /// <summary>ステージを一意に識別するためのID</summary>
    [SerializeField] private int stageID;

    /// <summary> ステージの表示名</summary>
    [SerializeField] private string stageName;

    [Header("フェーズ構成")]
    /// <summary> このステージに含まれるフェーズ一覧 </summary>
    [SerializeField] private List<StagePhase> stagePhases;

    /// <summary> ステージに含まれるフェーズ一覧を取得する。</summary>
    /// <returns>ステージのフェーズリスト</returns>
    public List<StagePhase> GetStagePhases() => stagePhases;

    /// <summary> ステージの表示名を取得する </summary>
    /// <returns>ステージ名</returns>
    public string GetStageName() => stageName;

    /// <summary>ステージの識別IDを取得する。</summary>
    /// <returns>ステージID</returns>
    public int GetStageID() => stageID;
}