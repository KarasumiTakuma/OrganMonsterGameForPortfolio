using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

//  ステージ選択画面のボタンにアタッチするスクリプト(StageSelectButtonプレファブにアタッチしている)
// ボタンを押したときに BattleSessionData に選択ステージと味方モンスター情報をセットして、バトルシーンへ遷移する

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;  // このスクリプトを付けたオブジェクト自体のボタンコンポーネントをアタッチ
    private StageInfo stageData;  // このボタンに対応するステージ情報
    private void Awake()
    {
        // ボタンクリック時のイベント登録。OnButtonClickを登録しておく
        button.onClick.AddListener(OnButtonClick);
    }

    // このボタンにステージ情報をセットするメソッド
    public void SetStage(StageInfo stage) { stageData = stage; }

    // ボタンが押されたときに処理されるメソッド。クリック時に発動するイベントとして登録されている。
    private void OnButtonClick()
    {
        var session = BattleSessionData.Instance;
        // stageData がセットされているか確認
        if (stageData == null)
        {
            Debug.LogError("StageSelectButton: stageData がセットされていません");
            return;
        }

        // 選択されたステージの情報を BattleSessionData にセット
        session.SetCurrentStage(stageData);

        // 現在のプレイヤーパーティリストを取得
        List<MonsterData> party = GameManager.Instance.PlayerData.CurrentParty.ToList();
        if (party == null || party.Count == 0) //パーティがセットされていなければ何もしない
        {
            Debug.LogError("StageSelectButton: CurrentParty が null または空です");
            return;
        }
        // BattleSessionData の味方リストを初期化
        session.SetPlayerAlliesList(new List<AllyMonsterData>());
        // プレイヤーパーティのMonsterData型オブジェクト1体1体に対して
        foreach (var monster in party)
        {
            AllyMonsterData ally;
            if (monster is AllyMonsterData existingAlly) // AllyMonsterData型にキャストする
            {
                ally = existingAlly;
            }
            else //キャストできなければ
            {
                // MonsterDataをAllyMonsterDataに変換
                ally = MonsterDataConverter.ToAllyMonster(monster);
            }
            // AllyMonsterDataであることを確認後、
            // BattleSessionData の味方リストに味方データを追加する
            session.GetPlayerAlliesList().Add(ally);
        }


        // 味方データからカードデッキを生成
        List<Card> deck = new List<Card>();
        foreach (var ally in session.GetPlayerAlliesList()) // 味方モンスターそれぞれに対して
        {
            if (ally.cards == null) continue;  // カード情報がセットされていなければ、次のモンスターへ処理を移す

            foreach (var cardData in ally.cards) // 味方モンスター1体が持つカードリスト内の複数カード(10枚分)を取得していく
            {
                if (cardData != null) // カードデータが空でなければ
                    deck.Add(new Card(cardData)); // そのカードをデッキとして一時的なリストに追加
            }
        }
        // プレイヤーが使うカードの山札情報としてBattleSessionDataにセットしておく
        session.SetPlayerCardList(deck);

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
