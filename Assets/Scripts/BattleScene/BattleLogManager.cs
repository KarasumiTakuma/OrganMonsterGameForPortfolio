// バトル中の戦闘ログをキューで保持し、管理するクラス(シングルトンインスタンスを生成)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// バトル中の戦闘ログを管理するクラス。
/// ログメッセージをキューで保持し、
/// TextMeshPro の UI に一定数まで表示する。
/// シングルトンとして実装され、バトル中のどこからでもログ追加が可能。
/// </summary>
public class BattleLogManager : MonoBehaviour
{
    /// <summary>BattleLogManager のシングルトンインスタンス</summary>
    public static BattleLogManager Instance { get; private set; }

    /// <summary>戦闘ログを表示する TextMeshPro のテキストコンポーネント。</summary>
    [SerializeField] private TMP_Text logText;

    /// <summary> 
    /// 戦闘ログ表示用 ScrollView の ScrollRect コンポーネント。
    /// ログ追加時に自動スクロールさせるために使用。 
    /// </summary>
    [SerializeField] private ScrollRect scrollRect;

    /// <summary>ログキューに保持できる最大ログ数</summary>    
    [SerializeField] private int maxLogCount = 20;

    /// <summary>戦闘ログを保持するキュー。FIFO（先入れ先出し）で管理され、表示件数制限に使用される。</summary>
    private Queue<string> logQueue = new Queue<string>();

    /// <summary>
    /// シングルトンの初期化処理。
    /// 既にインスタンスが存在する場合は自身を破棄する。
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 初期状態ではログ表示を空に
        logText.text = "";
    }

    /// <summary>
    /// 戦闘ログを1件追加する。
    /// ログ種別に応じて色付けを行い、
    /// キューに追加後、UI表示を更新する。
    /// </summary>
    /// <param name="message">表示するログメッセージ本文</param>
    /// <param name="type">ログの種別（攻撃・回復・システムなど）</param>
    public void AddLog(string message, BattleLogType type = BattleLogType.System)
    {
        // ログ種別に応じて色を適用
        string coloredMessage = ApplyColor(message, type);

        // ログをキューに追加
        logQueue.Enqueue(coloredMessage);

        // 最大ログ数を超えた場合、一番古いログを削除
        if (logQueue.Count > maxLogCount)
        {
            logQueue.Dequeue();
        }

        // キュー内のログを改行区切りで表示
        logText.text = string.Join("\n\n", logQueue);

        // ログ追加後に自動で下までスクロール
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0f);
    }

    /// <summary>
    /// ログ種別に応じてメッセージに色タグを付与する。
    /// TextMeshPro のリッチテキスト機能を利用する。
    /// </summary>
    /// <param name="message">元のログメッセージ</param>
    /// <param name="type">ログの種別</param>
    /// <returns>色タグが付与されたログメッセージ</returns>
    private string ApplyColor(string message, BattleLogType type = BattleLogType.System)
    {
        return type switch
        {
            BattleLogType.Attack => $"<color=red>{message}</color>",
            BattleLogType.DamageOverTime => $"<color=purple>{message}</color>",
            BattleLogType.Heal => $"<color=green>{message}</color>",
            BattleLogType.Attention => $"<color=yellow>{message}</color>",
            BattleLogType.System => $"<color=white>{message}</color>",
            _ => message
        };
    }

}

/// <summary>
/// 戦闘ログの種別を表す列挙型。
/// ログの色分けや意味付けに使用される。
/// </summary>
public enum BattleLogType
{
    Attack,           // 通常攻撃やスキル攻撃に関するログ
    DamageOverTime,  // 毒や火傷などの継続ダメージログ
    Heal,            // 回復効果に関するログ
    Attention,      // 注意喚起や重要イベントのログ
    System          // システムメッセージや汎用ログ
}
