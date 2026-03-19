using System.Collections;
using UnityEngine;
using TMPro;

// ゲームプレイ中の様々な通知を表示するクラス
public class NoticeUI : MonoBehaviour
{
    public static NoticeUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject noticeObject;  // メッセージ通知用のオブジェクト
    [SerializeField] private TMP_Text noticeText;      // 表示するテキスト(noticeObjectの子)

    private Coroutine currentRoutine;  // 実行中のコルーチン(ShowRoutine)への参照を保持する変数

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        noticeObject.SetActive(false); // 生成時は通知オブジェクトを非表示
    }


    // 通知の表示を行う処理を外部から呼び出すメソッド
    // messageは表示する通知メッセージ。durationは表示したい時間
    public void Show(string message, float duration = 2.0f)
    {
        // 表示中の通知があるなら、コルーチンを止めて、
        // 新たな通知を表示する
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    // message(通知)をduration秒間表示し続けるコルーチン
    private IEnumerator ShowRoutine(string message, float duration)
    {
        noticeText.text = message;

        noticeObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        noticeObject.SetActive(false);  // duration秒間待ってから非表示にする
        currentRoutine = null;  // 表示処理は、このコルーチンへの参照をはずす
    }

}