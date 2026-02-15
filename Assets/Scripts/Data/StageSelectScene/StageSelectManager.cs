using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//StageSelectSceneで、全ステージのボタンを自動生成するクラス

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージ設定")]
    [SerializeField] private List<StageInfo> allStages = new List<StageInfo>();  // 表示する全ステージの情報リスト

    [Header("UI")]
    [SerializeField] private List<Transform> buttonPosition;  // 各ステージボタンを配置するポジションを親としてリストに入れる
    [SerializeField] private GameObject stageButtonPrefab;  // 各ステージボタンのプレハブ

    private void Start()
    {
        // シーンロード時に(アクティブなら)各ステージ選択ボタンを生成
        CreateStageButtons();
    }

    // 各ステージに対応したボタンを生成して配置するメソッド
    private void CreateStageButtons()
    {

        // ステージに応じてボタンを配置
        for (int index = 0; index < allStages.Count; index++)
        {
            // 配置先の親を決める。buttonPositionの数より多いステージがあっても、最後の親ポジションに配置する
            Transform parentPosition = buttonPosition[Mathf.Min(index, buttonPosition.Count - 1)];

            // allStageのインデックスに応じたステージ情報を取り出す
            StageInfo stageInfo = allStages[index];

            // 該当のボタン配置ポジションを親として、stageButtonPrefabを配置する
            GameObject buttonObj = Instantiate(stageButtonPrefab, parentPosition);
            buttonObj.name = $"StageButton{stageInfo.GetStageID()}";

            // ボタンコンポーネントをボタンオブジェクトから取り出す
            Button button = buttonObj.GetComponent<Button>();

            // StageSelectButton にステージ情報を渡す
            StageSelectButton stageSelectButton = buttonObj.GetComponent<StageSelectButton>();
            if (stageSelectButton != null)
            {
                stageSelectButton.SetStage(stageInfo);
            }

            // ボタンにステージ名を表示
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = stageInfo.GetStageName();
            }

            bool isUnlocked = false;  // ステージが解放されている状態を示す。trueなら解放

            if (index == 0)
            {
                isUnlocked = true;  // 最初のステージは常に解放状態に
            }
            else
            {
                // 前のステージがクリア済みなら解放
                // 前のステージのステージIDが
                // PlayerDataが保持する「クリア済みステージのIDリスト」に含まれていれば、
                // isUnlockedがtrueになる。
                StageInfo prevStage = allStages[index - 1];  // 一つ前のステージ情報を取り出し、
                // そのステージがクリア済みなら、ステージが解放されている状態(isUnlocked = true)にする
                isUnlocked = GameManager.Instance.PlayerData.IsStageCleared(prevStage.GetStageID());
            }

            // ボタンが押下できるかを設定する
            // isUnlockedがtrueなら押せる
            stageSelectButton.SetUnlocked(isUnlocked);
        }
    }
}
