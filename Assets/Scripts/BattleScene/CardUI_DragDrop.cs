using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// カードのドラッグ＆ドロップ挙動を制御するクラス。「UI操作だけ」を責務とする
public class CardUI_DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private EnemyAreaManager enemyAreaManager;
    private AllyAreaManager allyAreaManager;
    private Card card;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas canvas;  // ドラッグ時にUIが隠れないようにする

    // CardPlayedHandlerカードプレイを通知するイベント
    // デリゲートとして扱う。
    // 第一引数としてCard型を受け取り、
    // 第二引数としてVector2型(カードをドロップする位置)を受け取り、
    // 第三引数としてAction<bool>(bool型を引数とする返り値voidのメソッド)を受けとり、
    // そして、voidを返すメソッド(イベント群)を格納するデリゲート。
    // メソッドを変数のように扱える型がdelegate
    public delegate void CardPlayedHandler(Card card, Vector2 dropPosition, System.Action<bool> isSuccessCallback);

    // CardPlayedHandler型の変数OnCardPlayedを「イベント」として宣言
    // 他のクラスからこのイベント変数にメソッドを登録できる。
    public event CardPlayedHandler OnCardPlayed;

    public void Setup(Card card, EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager)
    {
        this.card = card;
        this.enemyAreaManager = enemyAreaManager;
        this.allyAreaManager = allyAreaManager;
        this.canvas = GetComponentInParent<Canvas>();
    }

    public void ClearOnCardPlayed()
    {
        OnCardPlayed = null;
    }

    // ドラッグ開始直後の処理
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        transform.SetParent(canvas.transform); // UIが最前面になるように
    }

    // ドラッグ中の処理
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        switch (card.GetCardEffectType())
        {
            case CardEffectType.AttackToSelected:
            case CardEffectType.DamageOverTime:
                // ハイライトする位置の調節
                if (transform.position.y > Screen.height * (5f / 12f))
                {
                    enemyAreaManager.HighlightNearestEnemy(transform.position);
                }
                else
                {
                    enemyAreaManager.ClearAllHighlights();
                }
                break;

            case CardEffectType.AttackToAll:
                // ハイライトする位置の調節
                if (transform.position.y > Screen.height * (5f / 12f))
                {
                    enemyAreaManager.HighlightAllEnemies();
                }
                else
                {
                    enemyAreaManager.ClearAllHighlights();
                }

                break;

            case CardEffectType.Heal:
            case CardEffectType.HealOverTime:
                // ハイライトする位置の調節
                if (transform.position.y >= Screen.height * (5f / 12f)
                    && transform.position.y < Screen.height * (4f / 5f))
                {
                    allyAreaManager.HighlightAllAllies();
                }
                else
                {
                    allyAreaManager.ClearAllHighlights();
                }
                break;
        }

    }


    // ドラッグ終了後(ドロップした時)の処理
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dropPosition = eventData.position;

        enemyAreaManager.ClearAllHighlights();
        allyAreaManager.ClearAllHighlights();

        // 画面 * 5f / 12f より上でドロップしたとき
        if (dropPosition.y > Screen.height * (5f / 12f))
        {

            // OnCardPlayed に登録されているメソッドを呼び出す
            // 通常、BattleManager の PlayCard メソッドなどが登録されている
            // null 条件演算子「?」で、イベントが登録されていれば呼び出す
            // Invokeはイベントに登録された全てのメソッドを順番に呼ぶ
            // 第一引数として cardData を渡す
            // 第二引数は「プレイが成功したかどうか」を通知するコールバック
            // ラムダ式でisPlaySuccessがtrueならカードを削除、失敗なら元の位置に戻す処理を実行
            OnCardPlayed?.Invoke(card, dropPosition, isPlaySuccess =>
            {
                if (isPlaySuccess)
                {
                    Destroy(gameObject); // カードをプレイ成功していたら、そのカードを削除
                }
                else // 失敗していたら(マナが足りないなどでカードをプレイできなかったら)
                {
                    // 元の位置に戻す
                    transform.SetParent(originalParent);
                    transform.position = originalPosition;
                }
            });
        }
        else // 画面中央より下にドロップした場合
        {
            // 元の位置に戻す
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
}