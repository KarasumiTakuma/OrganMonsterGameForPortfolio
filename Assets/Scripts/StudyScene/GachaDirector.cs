using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Threading;

public class GachaDirector : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gachaResultPanel; // 演出全体の親パネル
    [SerializeField] private GameObject singleResultDisplay; // 1枚ずつ表示する用のUIグループ
    [SerializeField] private GameObject summaryDisplay; // 最終的な一覧表示用のUIグループ

    [Header("Single Result Components")]
    [SerializeField] private Image singleResultIcon; // アイテムをひとつ(ずつ)表示するアイコン
    [SerializeField] private TextMeshProUGUI singleResultName; //排出したアイテムの名前
    [SerializeField] private Button nextButton; // [次へ]ボタンをInspectorで設定
    [SerializeField] private TextMeshProUGUI countText;
    private int count;

    [Header("Summary Components")]
    [SerializeField] private Transform summaryGridContent; // 一覧表示の親オブジェクト (Contentなど)
    [SerializeField] private GameObject inventorySlotPrefab; // インベントリスロットのプレハブ
    [SerializeField] private Button backButton; // 研究所に戻るボタンなど

    // 演出が再生中かどうかを管理するフラグ
    public bool IsPlaying { get; private set; }
    private bool nextButtonPressed = false; // ★ボタンが押されたかを記録する旗

    private void Start()
    {
        // 戻るボタンが押されたら、パネルを非表示にする
        backButton?.onClick.AddListener(() => gachaResultPanel.SetActive(false));
        // 次へボタンを押されたら、次の表示
        nextButton?.onClick.AddListener(OnNextButtonPressed);
        // 最初は非表示にしておく
        gachaResultPanel.SetActive(false);
        countText.text = "0";
        count = 1;
    }

    // 次へボタンが押されたら、旗を立てる
    public void OnNextButtonPressed()
    {
        nextButtonPressed = true;
    }

    /// <summary>
    /// ガチャの演出を開始する
    /// </summary>
    public void PlayRevealAnimation(List<OrganData> results)
    {
        if (IsPlaying) return;
        StartCoroutine(RevealSequence(results));
    }

    /// <summary>
    /// 時間をかけて演出を再生するためのコルーチン
    /// </summary>
    private IEnumerator RevealSequence(List<OrganData> results)
    {
        IsPlaying = true;

        /// --- 1. ---
        /// エフェクトが完成するとまず、
        /// gachaResultPanel.SetActive(true);
        /// effectPanel.SetActive(true);      // 演出パネルを表示
        /// singleResultDisplay.SetActive(true); 結果は非表示
        /// summaryDisplay.SetActive(false);
        /// nextButton.gameObject.SetActive(true);

        // --- 1. 演出の準備 ---
        gachaResultPanel.SetActive(true);
        singleResultDisplay.SetActive(true);
        summaryDisplay.SetActive(false);
        //nextButton.gameObject.SetActive(true); // 「次へ」ボタンを表示

        // --- 2. ボタンが押されるたびに、次の結果を表示 ---
        foreach (var organ in results)
        {
            singleResultIcon.sprite = organ.GetIcon();
            singleResultName.text = organ.GetName();
            countText.text = count.ToString() + "連目";
            count++;

            // ここで「カードがめくれる」などのアニメーションやサウンドを再生
            // if (organ.rarity == 1)
            // {
            /// エフェクトメソッド呼び出す
            // } レアリティで演出変える
            
            ///    // 演出が終わるまで待つ
            ///yield return new WaitForSeconds(1.5f);

            // --- 3. 結果の表示 ---
            /// effectPanel.SetActive(false);       // 演出パネルを非表示にして
            /// resultDisplayPanel.SetActive(true); // 結果パネルを表示する

            // 旗を下げて、ボタンが押されるのを待つ
            nextButtonPressed = false;
            yield return new WaitUntil(() => nextButtonPressed);
        }

        // --- 3. 最終的な一覧表示 ---
        singleResultDisplay.SetActive(false);
        //nextButton.gameObject.SetActive(false); // 「次へ」ボタンを非表示
        summaryDisplay.SetActive(true);
        PopulateSummaryGrid(results);
        count = 1;

        IsPlaying = false;
    }

    /// <summary>
    /// 最終的な結果をグリッドに表示する
    /// </summary>
    private void PopulateSummaryGrid(List<OrganData> results)
    {
        // 既存のスロットをクリア
        foreach (Transform child in summaryGridContent)
        {
            Destroy(child.gameObject);
        }

        // 結果をスロットとして生成
        foreach (var organ in results)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, summaryGridContent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            // 10連の場合、同じアイテムが複数出ることがあるので、ここでは数は1で表示
            slotUI.Setup(organ, 1); 
        }
    }
}