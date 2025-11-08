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

    // カードプレイを通知するイベント
    // Card型の引数を1つ受け取りvoidを返すメソッド(イベント群)を格納するもの
    // メソッドを変数のように扱える型がdelegate
    public delegate void CardPlayedHandler(Card card);

    // CardPlayedHandler型の変数OnCardPlayedを「イベント」として宣言
    // 他のクラスからこのイベント変数にメソッドを登録できる。
    public event CardPlayedHandler OnCardPlayed;

    public void Setup(Card card)
    {
        cardData = card;
        canvas = GetComponentInParent<Canvas>();
    }

    // ドラッグ開始
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        transform.SetParent(canvas.transform); // UIが最前面になるように
    }

    // ドラッグ中
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // ドラッグ終了
    public void OnEndDrag(PointerEventData eventData)
    {

        // 仮に画面中央より上でドロップしたらカードをプレイする
        if (eventData.position.y > Screen.height / 2f)
        {
            // イベント変数OnCardPlayedに登録されているメソッドを順番に呼び出す
            // null条件演算子「?」でOnCardPlayedにメソッドが登録されているか(nullでないか)を確認
            // Invokeはイベントに登録されたメソッドをまとめて呼び出すメソッド。
            // 引数としてcardDataを渡すことで、登録されているメソッドすべてCard型の引数cardDataを受け取るこのになる。
            OnCardPlayed?.Invoke(cardData);  
            Destroy(gameObject); // カードをUIから削除   
        }
        else
        {
            // 元の位置に戻す
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
}