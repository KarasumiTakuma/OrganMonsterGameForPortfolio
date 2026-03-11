using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// バトル中に表示される各種通知（ゲーム開始、ターン開始、ゲームオーバー等）を管理するクラス。
/// 通知用UIオブジェクトの表示・非表示と、
/// 一定時間表示されるメッセージ演出をコルーチンで制御する。
/// </summary>
public class BattleNoticeManager : MonoBehaviour
{
    /// <summary> 通知表示用の親オブジェクト。テキスト（noticeText）を子に持ち、表示／非表示を切り替えるために使用する</summary>
    [SerializeField] private GameObject noticeObject;

    /// <summary>通知内容を表示する TextMeshPro テキスト。通知の種類に応じて文字、色を変更する</summary>
    [SerializeField] private TMP_Text noticeText;

    /// <summary>現在実行中の通知表示コルーチンへの参照。新しい通知を表示する際、既存の通知を中断するために使用</summary>
    private Coroutine currentRoutine;

    /// <summary>
    /// シーン開始時は通知UIを非表示状態にしておく。
    /// </summary>
    private void Awake()
    {
        noticeObject.SetActive(false);
    }

    /// <summary>
    /// 指定された通知タイプを、一定時間表示する。
    /// 既に通知が表示中の場合はそれを中断し、新しい通知を優先表示する。
    /// </summary>
    /// <param name="battleNoticeType">表示する通知の種類</param>
    /// <param name="duration">通知を表示する時間（秒）</param>
    public IEnumerator Show(BattleNoticeType battleNoticeType, float duration = 1.0f)
    {
        // 既に通知が表示中の場合は中断する
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            noticeObject.SetActive(false); 
        }
        
        // 新しい通知表示コルーチンを開始
        yield return currentRoutine = StartCoroutine(ShowRoutine(battleNoticeType, duration));
    }

    /// <summary>
    /// 通知タイプに応じた文字、色を設定し、
    /// 指定時間だけ通知を表示し続ける内部コルーチン。
    /// </summary>
    /// <param name="type">表示する通知の種類</param>
    /// <param name="duration">通知を表示する時間（秒）</param>
    private IEnumerator ShowRoutine(BattleNoticeType type, float duration)
    {
        // 通知タイプごとに表示内容を切り替える
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

        noticeObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        noticeObject.SetActive(false);
        currentRoutine = null;
    }

    /// <summary>
    /// 勝利時および報酬獲得時専用の通知表示処理。
    /// 「勝利！」→「研究ポイント獲得」の順に表示する。
    /// </summary>
    /// <param name="points">獲得した研究ポイント数</param>
    /// <param name="duration">各メッセージの表示時間（秒）</param>
    public IEnumerator ShowVictoryAndReward(int points, float duration = 2.0f)
    {
        // 表示中の通知があれば中断
        if(currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            noticeObject.SetActive(false); 
        }

        yield return currentRoutine = StartCoroutine(ShowVictoryAndRewardRoutine(points, duration));
    }
    
    /// <summary>
    /// 勝利メッセージと報酬ポイント表示を段階的に行う内部コルーチン。
    /// </summary>
    /// <param name="points">獲得した研究ポイント数</param>
    /// <param name="duration">各表示の継続時間（秒）</param>
    private IEnumerator ShowVictoryAndRewardRoutine(int points, float duration)
    {
        // 勝利メッセージ表示
        noticeText.text = "勝利!";
        noticeText.color = Color.yellow;
        noticeObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        // 報酬ポイント表示
        noticeText.text = $"研究ポイント{points}pt獲得!";
        noticeText.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        noticeObject.SetActive(false);
        currentRoutine = null;
    }
}

/// <summary>
/// バトル中に表示される通知の種類を表す列挙型。
/// 通知内容の分岐や表示制御に使用される。
/// </summary>
public enum BattleNoticeType
{
    BattleStart,  // バトル開始時
    PlayerTurn,   // プレイヤーターン開始
    EnemyTurn,    // 敵ターン開始
    NextPhase,    // 次フェーズ遷移
    StageClear,   // ステージクリア
    GameOver      // ゲームオーバー
}
