using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトルセッション中に必要な一時データを保持するクラス。
/// ステージ選択シーンからバトルシーンへデータを受け渡すために使用される。
/// シングルトンとして実装され、シーン遷移後も破棄されない。
/// </summary>
public class BattleSessionData : MonoBehaviour
{
    /// <summary>BattleSessionData のシングルトンインスタンス</summary>
    public static BattleSessionData Instance { get; private set; }

    /// <summary>現在選択されているステージ情報</summary>
    private StageInfo currentStage;

    /// <summary> 現在進行中のフェーズインデックス。StageInfo が保持するフェーズリストの添字として使用される</summary>
    private int currentPhaseIndex;

    /// <summary>プレイヤーがバトルに持ち込む味方モンスターのリスト</summary>
    private List<AllyMonsterData> playerAllies;

    /// <summary> プレイヤーが使用するカードデッキ。味方モンスターが所持するカードを統合したもの</summary>
    private List<Card> playerDeckList;

    /// <summary>
    /// シングルトン初期化。
    /// 既存インスタンスが存在する場合は自身を破棄する。
    /// </summary>
    private void Awake()
    {
        // 既にシングルトンが存在している場合は破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 自身を唯一のインスタンスとして登録
        Instance = this;

        // シーン遷移時にも破棄されないようにする
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// バトルセッションに関する保持データをすべて初期化する。
    /// ステージ選択のやり直しやタイトルへ戻る際に使用される。
    /// </summary>
    public void ClearData()
    {
        currentStage = null;
        currentPhaseIndex = 0;
        playerAllies = null;
        playerDeckList = null;
    }

    /// <summary> 現在選択されているステージ情報を取得</summary>
    public StageInfo GetCurrentStage() => currentStage;

    /// <summary> プレイヤーの味方モンスターリストを取得</summary>
    public List<AllyMonsterData> GetPlayerAlliesList() => playerAllies;

    /// <summary> 現在のフェーズインデックスを取得</summary>
    public int GetCurrentPhaseIndex() => currentPhaseIndex;

    /// <summary>プレイヤーのカードデッキを取得する</summary>
    public List<Card> GetPlayerDeckList() => playerDeckList;

    /// <summary>現在のステージ情報を設定する。</summary>
    /// <param name="stageInfo">選択されたステージ情報</param>
    public void SetCurrentStage(StageInfo stageInfo) { currentStage = stageInfo; }

    /// <summary> 現在のフェーズインデックスを設定する</summary>
    /// <param name="phaseIndex">設定するフェーズ番号</param>
    public void SetPhaseIndex(int phaseIndex) { currentPhaseIndex = phaseIndex; }

    /// <summary>プレイヤーの味方モンスターリストを設定する</summary>
    /// <param name="allyMonsterData">味方モンスターのリスト</param>
    public void SetPlayerAlliesList(List<AllyMonsterData> allyMonsterData) { playerAllies = allyMonsterData; }

    /// <summary> プレイヤーのカードデッキを設定する</summary>
    /// <param name="deck">使用するカードデッキ</param>
    public void SetPlayerDeckList(List<Card> deck) { playerDeckList = deck; }

    /// <summary> 現在のフェーズを1つ進める</summary>
    public void AdvancePhase() { currentPhaseIndex++; }

    /// <summary>
    /// 次のフェーズが存在するかを判定する。
    /// </summary>
    /// <returns>
    /// 次フェーズが存在する場合は true。
    /// ステージ未設定、または最終フェーズの場合は false。
    /// </returns>
    public bool IsThereNextPhase()
    {
        if (currentStage == null) return false;

        return currentPhaseIndex < currentStage.GetStagePhases().Count - 1;
    }
}
