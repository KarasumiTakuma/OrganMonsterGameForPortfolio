using System.Collections;
using UnityEngine;
using TMPro;

// バトル中の様々な通知(ゲーム開始、ゲームオーバ、ターン開始 等)を表示するクラス
public class BattleNoticeManager : MonoBehaviour
{
    [SerializeField] private GameObject noticeObject; // テキスト付きの親オブジェクト(対象のストを))通知テキストを子に持つ)
    [SerializeField] private TMP_Text noticeText;      // 表示するテキスト(通知内容を表示するテキスト)

    private Coroutine currentRoutine;   // 実行中のコルーチン(ShowRoutine)への参照を保持する変数

    private void Awake()
    {
        noticeObject.SetActive(false); // 生成時は通知オブジェクトを非表示
    }

    // 通知の表示を行う処理を外部から呼び出すメソッド
    // battleNoticeTypeは、表示する通知の種類。durationは表示したい時間
    public void Show(BattleNoticeType battleNoticeType, float duration = 2.0f)
    {
        // 表示中の通知があるなら、コルーチンを止めて、
        // 新たな通知を表示する
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            noticeObject.SetActive(false); 
        }

        currentRoutine = StartCoroutine(ShowRoutine(battleNoticeType, duration));
    }

    // 通知の種類に応じたメッセージをduration秒間表示し続けるコルーチン
    private IEnumerator ShowRoutine(BattleNoticeType type, float duration)
    {

        // typeに応じた通知の表示
        switch (type)
        {
            case BattleNoticeType.BattleStart:
                noticeText.text = "ゲーム開始！";
                noticeText.color = Color.white;
                break;
            case BattleNoticeType.PlayerTurn:
                noticeText.text = "プレイヤーのターン";
                noticeText.color = Color.blue;
                break;
            case BattleNoticeType.EnemyTurn:
                noticeText.text = "敵のターン";
                noticeText.color = Color.red;
                break;
            case BattleNoticeType.NextPhase:
                noticeText.text = "次のフェーズへ!";
                noticeText.color = Color.white;
                break;
            case BattleNoticeType.StageClear:
                noticeText.text = "ステージクリア！";
                noticeText.color = Color.yellow;
                break;
            case BattleNoticeType.GameOver:
                noticeText.text = "GameOver";
                noticeText.color = Color.red;
                break;
        }

        noticeObject.SetActive(true);  // 通知オブジェクトを表示する

        yield return new WaitForSeconds(duration);

        noticeObject.SetActive(false);  // duration秒間待ってから非表示にする
        currentRoutine = null;  // 表示処理は、このコルーチンへの参照をはずす
    }

    //　「報酬ポイント」の通知表示専用の処理を外部から呼び出すメソッド
    public void ShowVictoryAndReward(int points, float duration = 1.5f)
    {
        if(currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            noticeObject.SetActive(false); 
        }

        currentRoutine = StartCoroutine(ShowVictoryAndRewardRoutine(points, duration));
    }
    
    // 獲得した研究ポイントのメッセージをduration秒間表示し続けるコルーチン
    private IEnumerator ShowVictoryAndRewardRoutine(int points, float duration)
    {
        noticeText.text = "勝利!";
        noticeText.color = Color.yellow;
        noticeObject.SetActive(true);  // 通知オブジェクトを表示する

        yield return new WaitForSeconds(duration);

        noticeText.text = $"研究ポイント{points}pt獲得!";
        noticeText.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        noticeObject.SetActive(false);  // duration秒間待ってから非表示にする
        currentRoutine = null;  // 表示処理は、このコルーチンへの参照をはずす
    }
}

// バトル中の通知を、enum(列挙)型で6つのタイプに分ける
public enum BattleNoticeType
{
    BattleStart,
    PlayerTurn,
    EnemyTurn,
    NextPhase,
    StageClear,
    GameOver
}

