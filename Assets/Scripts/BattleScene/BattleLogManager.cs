// バトル中の戦闘ログをキューで保持し、管理するシングルトンインスタンスを生成するクラス

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance { get; private set; }

    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLogCount = 20;

    private Queue<string> logQueue = new Queue<string>();

    private void Awake()
    {
        Instance = this;
        logText.text = "";
    }

    public void AddLog(string message)
    {
        logQueue.Enqueue(message);

        if (logQueue.Count > maxLogCount)
        {
            logQueue.Dequeue();
        }

        logText.text = string.Join("\n\n", logQueue);

        // ログを追加した直後に、ログ表示ScrollViewのcontent(テキスト)が自動的に下までスクロールされるように
        // Contentのアンカーの位置を調節する
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0f);
    }
}
