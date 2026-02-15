using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

//  ステージ選択画面のボタンにアタッチするスクリプト(StageSelectButtonプレファブにアタッチしている)
// ボタンを押したときに BattleSessionData に選択ステージと味方モンスター情報(パーティとカードの情報)を準備して、バトルシーンへ遷移する

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;  // このスクリプトを付けたオブジェクト自体のボタンコンポーネントをアタッチ
    private StageInfo stageData;  // このボタンに対応するステージ情報

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("StageSelectButton: Button が設定されていません");
            return;
        }

        // ボタンクリック時のイベント登録。OnButtonClickを登録しておく
        button.onClick.AddListener(OnButtonClick);
    }

    // このボタンにステージ情報をセットするメソッド
    public void SetStage(StageInfo stageInfo) { stageData = stageInfo; }

    // ボタンが押されたときに処理されるメソッド。クリック時に発動するイベントとして登録されている。
    private void OnButtonClick()
    {
        var battleSessionData = BattleSessionData.Instance;
        battleSessionData.ClearData();

        // stageData がセットされているか確認
        if (stageData == null)
        {
            Debug.LogError("StageSelectButton: stageData がセットされていません");
            return;
        }

        // 選択されたステージの情報および味方モンスター情報を BattleSessionData にセット
        battleSessionData.SetCurrentStage(stageData);
        bool isReadyBattlePreparation = BattlePreparation.TryPrepareBattle();
        if (!isReadyBattlePreparation)
        {
            Debug.Log("味方モンスターが編成されていません");
            return;
        }


        // BattleSceneへ移動
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    // ステージボタンが解放されているかどうかを設定し、
    // UIの色を変えて、ロック状態であることが分かるようにする。
    public void SetUnlocked(bool isUnlocked)
    {
        button.interactable = isUnlocked;  // ボタンを押せるかどうかを設定

        // isUnlockedの状態(解放状態かどうか)に応じて、ボタンの色を変える。
        // trueならそのまま。falseなら暗くする
        var colors = button.colors;
        colors.normalColor = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        button.colors = colors;
    }

}
