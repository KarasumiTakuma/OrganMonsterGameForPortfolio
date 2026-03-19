using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ステージ選択画面の管理クラス。
/// 登録されたステージ情報をもとに、選択用ボタンを自動生成・初期化する。
/// </summary>
public class StageSelectManager : MonoBehaviour
{
    [Header("ステージ設定")]
    /// <summary>表示対象となる全ステージのデータリスト。ステージ順序・解放判定の基準にも使用される</summary>
    [SerializeField] private List<StageInfo> allStages = new List<StageInfo>();

    [Header("UI")]
    /// <summary> ボタンを配置する親Transformのリスト。ステージ数がこの数を超えた場合、最後のTransformが使われる</summary>
    [SerializeField] private List<Transform> buttonPositions;

    /// <summary>ステージボタンのプレハブ</summary>
    [SerializeField] private GameObject stageButtonPrefab;

    private void Start()
    {
        // シーンロード時に(アクティブなら)各ステージ選択ボタンを生成
        CreateStageButtons();
    }

    /// <summary>
    /// ステージ情報に対応するボタンを生成・初期化する。
    /// ボタン生成 → 表示設定 → 解放状態設定 の順で処理を行う。
    /// </summary>
    private void CreateStageButtons()
    {
        for (int index = 0; index < allStages.Count; index++)
        {
            // ボタン配置先の親Transform。buttonPositions の範囲外アクセスを防ぐため Mathf.Min を使用。
            Transform parentPosition = buttonPositions[Mathf.Min(index, buttonPositions.Count - 1)];
            
            StageInfo stageInfo = allStages[index];

            // 該当のボタン配置ポジションを親として、stageButtonPrefabを配置する
            GameObject stageButtonObject = Instantiate(stageButtonPrefab, parentPosition);
            stageButtonObject.name = $"StageButton{stageInfo.GetStageID()}";

            StageSelectButton stageSelectButton = stageButtonObject.GetComponent<StageSelectButton>();
            if (stageSelectButton != null)
            {
                // ボタンにステージデータを登録
                stageSelectButton.SetStage(stageInfo);
            }

            TMP_Text buttonText = stageButtonObject.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                // ボタンにステージ名を表示
                buttonText.text = stageInfo.GetStageName();
            }

            // ステージ解放状態（true: 選択可能 / false: ロック状態）
            bool isUnlocked = false;

            
            if (index == 0)
            {
                isUnlocked = true;  // 最初のステージは常に解放状態に
            }
            else
            {
                // 前ステージがクリア済みなら解放
                StageInfo prevStage = allStages[index - 1];  
                isUnlocked = GameManager.Instance.PlayerData.IsStageCleared(prevStage.GetStageID());
            }

            // ボタンの有効 / 無効状態を反映
            stageSelectButton.SetUnlocked(isUnlocked);
        }
    }
}
