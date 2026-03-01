using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// ステージ選択画面のボタン制御クラス。
/// 対応するステージ情報を保持し、クリック時にバトル準備を行って
/// BattleScene へ遷移する役割を持つ。
/// StageSelectButton プレファブにアタッチされる想定。
/// </summary>

public class StageSelectButton : MonoBehaviour
{
    /// <summary> このオブジェクトに紐づくButtonコンポーネント </summary>
    [SerializeField] private Button button;

    /// <summary>このボタンに対応するステージ情報</summary>
    private StageInfo stageData;

    /// <summary>初期化処理</summary>
    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("StageSelectButton: Button が設定されていません");
            return;
        }

        // ボタンクリック時に呼び出される処理を登録
        button.onClick.AddListener(OnButtonClick);
    }


    /// <summary>このボタンにステージ情報を設定する</summary>
    /// <param name="stageInfo"> このボタンに対応付けるステージデータ</param>
    public void SetStage(StageInfo stageInfo) { stageData = stageInfo; }

    /// <summary>
    /// ボタン押下時の処理。
    /// バトル用共有データの初期化 → ステージ設定 → バトル準備 →
    /// 条件を満たした場合のみ BattleScene へ遷移する。
    /// </summary>
    private void OnButtonClick()
    {
        var battleSessionData = BattleSessionData.Instance;
        battleSessionData.ClearData();  // 前回のバトル情報をクリア
        
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
            NoticeUI.Instance.Show("パーティが3体そろっていません");
            Debug.Log("パーティが3体揃っていません");
            return;
        }

        // BattleSceneへ移動
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    /// <summary>
    /// ステージボタンの解放状態を設定する。
    /// 押下の可否とボタンの色を同時に制御する。
    /// </summary>
    /// <param name="isUnlocked"> true なら選択可能、false ならロック状態として無効化する。</param>
    public void SetUnlocked(bool isUnlocked)
    {
        button.interactable = isUnlocked;

        // ステージの解放状態に応じた表示色変更
        var colors = button.colors;
        colors.normalColor = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        button.colors = colors;
    }

}
