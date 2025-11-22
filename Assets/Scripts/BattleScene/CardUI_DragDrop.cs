using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// カードのドラッグ＆ドロップ挙動を制御するクラス。
public class CardUI_DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private Card cardData;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas canvas;  // ドラッグ時にUIが隠れないようにする

    // CardPlayedHandlerカードプレイを通知するイベント
    // デリゲートとして扱う。
    // 第一引数としてCard型を受け取理、
    // 第二引数としてAction<bool>(bool型を引数とする返り値voidのメソッド)を受けとり、
    // そして、voidを返すメソッド(イベント群)を格納するデリゲート。
    // メソッドを変数のように扱える型がdelegate
    public delegate void CardPlayedHandler(Card card, System.Action<bool> isSuccessCallback);

    // CardPlayedHandler型の変数OnCardPlayedを「イベント」として宣言
    // 他のクラスからこのイベント変数にメソッドを登録できる。
    public event CardPlayedHandler OnCardPlayed;

    public void Setup(Card card)
    {
        cardData = card;
        canvas = GetComponentInParent<Canvas>();
    }

    // OnCardPlayedに登録されているイベントを全て消去するためのメソッド
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
    }

    // ドラッグ終了後(ドロップした時)の処理
    public void OnEndDrag(PointerEventData eventData)
    {

        // 仮に画面中央より上でドロップしたとき
        if (eventData.position.y > Screen.height / 2f)
        {

            // OnCardPlayed に登録されているメソッドを呼び出す
            // 通常、BattleManager の PlayCard メソッドなどが登録されている
            // null 条件演算子「?」で、イベントが登録されていれば呼び出す
            // Invokeはイベントに登録された全てのメソッドを順番に呼ぶ
            // 第一引数として cardData を渡す
            // 第二引数は「プレイが成功したかどうか」を通知するコールバック
            // ラムダ式でisPlaySuccessがtrueならカードを削除、失敗なら元の位置に戻す処理を実行
            OnCardPlayed?.Invoke(cardData, isPlaySuccess =>
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