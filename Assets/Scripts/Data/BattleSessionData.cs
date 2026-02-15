using System.Collections.Generic;
using UnityEngine;


// バトル関連の一時データを保持するクラス。シングルトンインスタンス
// シーンをまたいでも破棄されない
// StageSelectButtonで設定され、BattleSceneに受け渡される想定
public class BattleSessionData : MonoBehaviour
{
    public static BattleSessionData Instance { get; private set; }

    private StageInfo currentStage;  // 現在選択されているステージ情報
    private List<AllyMonsterData> playerAllies;  // 選択中のプレイヤーパーティの味方モンスターリスト
    private List<Card> playerCards; // 味方モンスターが持つカード情報をまとめたリスト

    private void Awake()
    {   
        // 既にこのシングルトンが生成されていたら、このスクリプトを付けた新たなゲームオブジェクトは破壊される
        // シングルトンの二重生成を防止
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // すでに存在する場合は自分を破棄
            return;
        }

        // 存在していなければ、自分を唯一シングルトンとして生成する
        Instance = this;
        DontDestroyOnLoad(gameObject);  // シーン遷移で破棄されないようにする
    }

    // バトル前情報をリセットする
    // ステージ選択し直す・タイトルへ戻るなどで使用
    public void ClearData()
    {
        currentStage = null;
        playerAllies = null;
        playerCards = null;
    }

    // ゲットメソッド
    public StageInfo GetCurrentStage() => currentStage;
    public List<AllyMonsterData> GetPlayerAlliesList() => playerAllies;
    public List<Card> GetPlayerCardsList() => playerCards;

    // セットメソッド
    public void SetCurrentStage(StageInfo stageInfo) {currentStage = stageInfo;}
    public void SetPlayerAlliesList(List<AllyMonsterData> allyMonsterData) {playerAllies = allyMonsterData;}
    public void SetPlayerCardList(List<Card> cards) {playerCards = cards;}
}
