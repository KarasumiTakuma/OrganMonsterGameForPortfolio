using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// 手札エリア(UI)を管理するクラス。
/// 手札カードの生成・破棄・表示制御・操作可否の切り替えを担当する。
/// バトルロジックは持たず、UI(View)専用の責務に限定されている。
/// </summary>
public class HandAreaManager : MonoBehaviour
{
    [Header("References")]

    /// <summary>手札カード1枚分のUIプレハブ</summary>
    [SerializeField] private GameObject cardPrefab;

    /// <summary>手札カードを配置するUIスロットのTransform配列。スロット数＝最大手札枚数を想定</summary>
    [SerializeField] private Transform[] handSlotPoints;


    [Header("Visual Control")]
    /// <summary>手札UI全体の表示状態・操作可否を制御するCanvasGroup</summary>
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary> カードHover時に表示するカード説明パネル。</summary>
    [SerializeField] private CardDescriptionPanel cardDescriptionPanel;

    /// <summary>現在生成されている手札カードUIオブジェクトのリスト。UIオブジェクト管理専用</summary>
    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    /// <summary>現在の手札データ（Cardモデル）</summary>
    private List<Card> handCards;

    /// <summary>
    /// 手札データ（モデル）をセットする。
    /// UI構築前に必ず呼ばれる想定。
    /// </summary>
    /// <param name="handCardData">現在の手札カードデータリスト</param>
    public void SetHandCardData(List<Card> handCardData)
    {
        this.handCards = handCardData;
    }

    /// <summary>
    /// 手札UIを完全に再構築する。
    /// 既存UIを破棄し、新しい手札データをもとに再生成する。
    /// </summary>
    /// <param name="onCardPlayed">
    /// カード使用時に呼ばれるコールバック。
    /// (Card 使用カード, Vector2 ドロップ位置, Action<bool> カードが使用ができたかどうかの通知)
    /// </param>
    /// <param name="enemyAreaManager">敵エリア管理クラス</param>
    /// <param name="allyAreaManager">味方エリア管理クラス</param>
    public void RebuildHandUI(System.Action<Card, Vector2, System.Action<bool>> onCardPlayed, EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager)
    {
        ClearHandUI();
        BuildHandUI(onCardPlayed, enemyAreaManager, allyAreaManager);
    }

    /// <summary>
    /// 現在表示されている手札カードUIをすべて破棄する。
    /// </summary>
    private void ClearHandUI()
    {
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();
    }

    /// <summary>
    /// 手札データをもとに手札UIを生成する。
    /// 各スロットにカードUIを配置し、必要な初期設定を行う。
    /// </summary>
    /// <param name="onCardPlayed">カード使用時のコールバック</param>
    /// <param name="enemyAreaManager">敵エリア管理クラス</param>
    /// <param name="allyAreaManager">味方エリア管理クラス</param>
    private void BuildHandUI(System.Action<Card, Vector2, System.Action<bool>> onCardPlayed, 
        EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager)
    {
        int dataIndex = 0;

        foreach (var slotPoint in handSlotPoints)
        {
            if (dataIndex >= handCards.Count) break;

            GameObject cardObject = Instantiate(cardPrefab, slotPoint);
            SetupCardTransform(cardObject, slotPoint);

            Card cardData = handCards[dataIndex];

            SetupCardView(cardObject, cardData);
            SetupDragDrop(cardObject, cardData, onCardPlayed, enemyAreaManager, allyAreaManager);
            SetupHover(cardObject, cardData);

            spawnedCards.Add(cardObject);
            dataIndex++;
        }
    }

    /// <summary>
    /// カードUIのTransformをスロットに合わせて初期化する。
    /// サイズ・位置・スケールを統一する。
    /// </summary>
    /// <param name="cardObject">生成されたカードUIオブジェクト</param>
    /// <param name="slotPoint">配置先スロットTransform</param>
    private void SetupCardTransform(GameObject cardObject, Transform slotPoint)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        RectTransform slotRect = slotPoint.GetComponent<RectTransform>();

        if (cardRect == null || slotRect == null) return;

        cardRect.sizeDelta = slotRect.sizeDelta;
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localScale = Vector3.one;
    }

    /// <summary>
    /// CardViewにカードデータを設定し、表示を更新する。
    /// </summary>
    /// <param name="cardObject">カードUIオブジェクト</param>
    /// <param name="cardData">表示対象となるカードデータ</param>
    private void SetupCardView(GameObject cardObject, Card cardData)
    {
        var cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
        {
            cardView.SetCard(cardData);
        }
    }

    /// <summary>
    /// カードのドラッグ＆ドロップ操作を設定する。
    /// </summary>
    /// <param name="cardObject">カードUIオブジェクト</param>
    /// <param name="cardData">カードデータ</param>
    /// <param name="onCardPlayed">カード使用時コールバック</param>
    /// <param name="enemyAreaManager">敵エリア管理クラス</param>
    /// <param name="allyAreaManager">味方エリア管理クラス</param>
    private void SetupDragDrop(GameObject cardObject, Card cardData, 
        System.Action<Card, Vector2, System.Action<bool>> onCardPlayed, 
            EnemyAreaManager enemyAreaManager, AllyAreaManager allyAreaManager)
    {
        var dragDrop = cardObject.GetComponent<CardDragDropUI>();
        if (dragDrop == null || onCardPlayed == null) return;

        dragDrop.Setup(cardData, enemyAreaManager, allyAreaManager, cardDescriptionPanel);
        dragDrop.ClearOnCardPlayed();
        dragDrop.OnCardPlayed += (card, pos, callback) => onCardPlayed?.Invoke(card, pos, callback);
    }

    /// <summary>
    /// カードHover時の説明表示処理を設定する。
    /// </summary>
    /// <param name="cardObject">カードUIオブジェクト</param>
    /// <param name="cardData">カードデータ</param>
    private void SetupHover(GameObject cardObject, Card cardData)
    {
        var hover = cardObject.GetComponent<CardHoverHandler>();
        if (hover != null && cardDescriptionPanel != null)
        {
            hover.Setup(cardData, cardDescriptionPanel);
        }
    }

    /// <summary>
    /// 手札エリア全体の表示・非表示を切り替える。
    /// </summary>
    /// <param name="visible">表示する場合はtrue</param>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 手札UIの操作の可否を切り替える。
    /// 操作不可時は見た目も暗くする。
    /// </summary>
    /// <param name="interactable">操作可能にするかどうか</param>
    public void SetInteractable(bool interactable)
    {
        if (canvasGroup == null) return;

        // 手札が操作可能状態なら、手札の明るさを1(通常)に、
        // 不可能状態なら、手札を少し暗くして、
        // 操作不可能(interactable = false)と
        // クリック不可状態(blocksRaycasts = false)にする
        canvasGroup.alpha = interactable ? 1.0f : 0.4f;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    /// <summary>
    /// 現在生成されている手札カードUIオブジェクト一覧を取得する。
    /// </summary>
    /// <returns>手札カードUIオブジェクトのリスト</returns>
    public List<GameObject> GetSpawnedCards() => spawnedCards;
}
