// バトル中の戦闘ログをキューで保持し、管理するクラス(シングルトンインスタンスを生成)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance { get; private set; }

    [SerializeField] private TMP_Text logText;  // 戦闘ログのTMP(BattleLogText)をアタッチ
    [SerializeField] private ScrollRect scrollRect;  // 戦闘ログのScrollView(BattleLogScrollView)のScrollRectコンポーネント
    [SerializeField] private int maxLogCount = 20;  // キューに入れられるログの最大の数

    private Queue<string> logQueue = new Queue<string>();  // 戦闘ログをmaxLogCountの個数分保持するキュー。FIFOなので、古いものから順にDequeueされる。

    private void Awake()
    {
        Instance = this;  // シングルトン(このクラス唯一のオブジェクト)を生成
        logText.text = "";
    }

    // 外部からlogTextにログメッセージを追加するためのメソッド
    // BattleLogTypeを指定することで、メッセージの種別ごとに色が付く
    public void AddLog(string message, BattleLogType type)
    {
        // メッセージの種別(type)ごとに色を決める
        string coloredMessage = ApplyColor(message, type);

        logQueue.Enqueue(coloredMessage);  // キューに受け取ったメッセージを格納する

        // キューにあるログ数が最大カウントに達していたら、古いものを取り出す。
        if (logQueue.Count > maxLogCount)
        {
            logQueue.Dequeue();
        }

        logText.text = string.Join("\n\n", logQueue);  // キューにある複数ログに改行を2回設けて、logTextにtextとして格納(表示)

        // ログを追加した直後に、ログ表示ScrollViewのcontent(テキスト)が自動的に下までスクロールされるように
        // Contentのアンカーの位置を調節する
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0f);
    }

    // メッセージの種別(type)ごとに、メッセージに色を与えて、それを返すメソッド。デフォルトでのタイプはSystem
    private string ApplyColor(string message, BattleLogType type = BattleLogType.System)
    {
        // メッセージの種別(type)ごとに、messageに色を与えて(colorタグをつけて)それをリターンする
        return type switch
        {
            BattleLogType.Attack => $"<color=red>{message}</color>",
            BattleLogType.Heal => $"<color=green>{message}</color>",
            BattleLogType.Attention => $"<color=yellow>{message}</color>",
            BattleLogType.System => $"<color=white>{message}</color>",
            _ => message // デフォルトルート。messageをそのまま返す。「_」はディスカード(破棄する引数)
        };
    }


}

// 戦闘ログのメッセージを4つのタイプに分ける
// enumは列挙型
public enum BattleLogType
{
    Attack,
    Heal,
    Attention,
    System
}
