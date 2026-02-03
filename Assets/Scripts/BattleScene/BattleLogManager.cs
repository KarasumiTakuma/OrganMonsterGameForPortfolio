using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance { get; private set; }

    [SerializeField] private TMP_Text logText;
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
    }
}
