using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// カードのドラッグ＆ドロップ挙動を制御するUIクラス。
/// カードの移動・ハイライト表示・ドロップ判定など、
/// 「UI操作のみ」を責務として持ち、
/// 実際のカード効果処理はイベント経由で外部(BattleManager等)に任せる。
/// </summary>
public class CardDragDropUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    /// <summary>敵エリア全体の管理クラス。敵のハイライト制御に使用する。</summary>
    private EnemyAreaManager enemyAreaManager;

    /// <summary>味方エリア全体の管理クラス。味方のハイライト制御に使用する。</summary>
    private AllyAreaManager allyAreaManager;

    /// <summary>このUIが表現しているカードのモデルデータ。</summary>
    private Card card;

    /// <summary>ドラッグ開始前の親Transform。ドラッグ失敗時に戻すために保持する。</summary>
    private Transform originalParent;

    /// <summary>ドラッグ開始前のワールド座標。ドラッグ失敗時に戻すために保持する。</summary>
    private Vector3 originalPosition;

    /// <summary>カード説明パネル。ドラッグ開始時に非表示にするために使用する。</summary>
    private CardDescriptionPanel cardDescriptionPanel;

    /// <summary>
    /// カードをドラッグ中に最前面へ表示するためのCanvas。
    /// ドラッグ時に他UIに隠れないようにする目的で使用される。
    /// </summary>
    private Canvas canvas;

    /// <summary>
    /// カードがプレイされたことを通知するためのデリゲート。
    /// このクラスからBattleManagerへ処理を渡すために使用される。
    /// </summary>
    /// <param name="card">プレイされたカード</param>
    /// <param name="dropPosition">カードがドロップされた画面座標</param>
    /// <param name="isSuccessCallback">
    /// カードプレイの成否を通知するコールバック。
    /// true の場合はUI側でカードを破棄し、
    /// false の場合は元の位置に戻す。
    /// </param>
    public delegate void CardPlayedHandler(Card card, Vector2 dropPosition, System.Action<bool> isSuccessCallback);

    /// <summary>
    /// カードプレイ時に発火するイベント。
    /// BattleManager などが購読し、実際のカード効果の処理を行う。
    /// </summary>
    public event CardPlayedHandler OnCardPlayed;

    /// <summary>
    /// このUIを初期化するためのセットアップメソッド。
    /// 必要な参照をすべて外部から受け取る。
    /// </summary>
    /// <param name="card">このUIが表すカード</param>
    /// <param name="enemyAreaManager">敵エリア管理クラス</param>
    /// <param name="allyAreaManager">味方エリア管理クラス</param>
    /// <param name="descriptionPanel">カード説明パネル</param>
    public void Setup(Card card, EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager, CardDescriptionPanel descriptionPanel)
    {
        this.card = card;
        this.enemyAreaManager = enemyAreaManager;
        this.allyAreaManager = allyAreaManager;
        this.cardDescriptionPanel = descriptionPanel;
        this.canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// CardPlayed イベントに登録された全てのリスナーを解除する。
    /// 再利用やシーン遷移時の安全対策として使用される。
    /// </summary>
    public void ClearOnCardPlayed()
    {
        OnCardPlayed = null;
    }

    /// <summary>
    /// ドラッグ開始時に呼ばれる処理。
    /// 説明パネルを非表示にし、
    /// 元の親・位置を記録した上で、UIを最前面へ移動させる。
    /// </summary>
    /// <param name="eventData">ポインターイベント情報</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(cardDescriptionPanel != null)
        {
            cardDescriptionPanel.HideDescriptionPanel();
        }
        originalParent = transform.parent;
        originalPosition = transform.position;
        // ドラッグ中にUIが隠れないようCanvas直下へ移動
        transform.SetParent(canvas.transform);
    }

    /// <summary>
    /// ドラッグ中に毎フレーム呼ばれる処理。
    /// カード位置をマウスに追従させ、
    /// カード、マウスの位置に応じた対象ハイライトを制御する。
    /// </summary>
    /// <param name="eventData">ポインターイベント情報</param>
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


    /// <summary>
    /// ドラッグ終了時（ドロップ時）に呼ばれる処理。
    /// ドロップ位置に応じてカードプレイ判定を行い、
    /// 成否に応じてカードを破棄または元の位置へ戻す。
    /// </summary>
    /// <param name="eventData">ポインターイベント情報</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dropPosition = eventData.position;

        enemyAreaManager.ClearAllHighlights();
        allyAreaManager.ClearAllHighlights();

        // プレイ可能エリアにドロップされた場合
        if (dropPosition.y > Screen.height * (5f / 12f))
        {
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