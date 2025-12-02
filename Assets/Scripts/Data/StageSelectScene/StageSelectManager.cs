using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージ設定")]
    public List<StageInfo> allStages = new List<StageInfo>();

    [Header("UI")]
    public List<Transform> buttonParents; // ボタンを配置する複数の親
    public GameObject stageButtonPrefab;

    private void Start()
    {
        CreateStageButtons();
    }

    private void CreateStageButtons()
    {

        // ステージに応じてボタンを配置
        for (int i = 0; i < allStages.Count; i++)
        {
            // 配置先の親（buttonParents の数が不足しない前提）
            Transform parent = buttonParents[Mathf.Min(i, buttonParents.Count - 1)];

            StageInfo stage = allStages[i];

            GameObject buttonObj = Instantiate(stageButtonPrefab, parent);
            buttonObj.name = $"StageButton_{stage.stageID}";

            // ボタン設定
            Button button = buttonObj.GetComponent<Button>();
            StageSelectButton stageSelectButton = buttonObj.GetComponent<StageSelectButton>();
            if (stageSelectButton != null)
            {
                stageSelectButton.SetStage(stage);
            }

            // テキスト設定
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = stage.stageName;
            }
        }
    }
}
