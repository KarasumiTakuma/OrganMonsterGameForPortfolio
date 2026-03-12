using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Threading;
using System.Linq;

public class GachaDirector : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private Animator effectAnimator;
    // [SerializeField] private Animator normalAnimator; // ゴゴゴのPNG画像を持つGameObjectのAnimatorを設定
    // [SerializeField] private Animator rareAnimator;
    // [SerializeField] private Animator superRareAnimator;

    [Header("UI Panels")]
    [SerializeField] private GameObject gachaResultPanel; // 演出全体の親パネル
    [SerializeField] private GameObject singleResultDisplay; // 1枚ずつ表示する用のUIグループ
    [SerializeField] private GameObject summaryDisplay; // 最終的な一覧表示用のUIグループ
    [SerializeField] private GameObject effectPanel; // 演出用のパネル

    [Header("Single Result Components")]
    [SerializeField] private Image singleResultIcon; // アイテムをひとつ(ずつ)表示するアイコン
    [SerializeField] private TextMeshProUGUI singleResultName; //排出したアイテムの名前
    [SerializeField] private Button nextButton; // [次へ]ボタンをInspectorで設定
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button skipButton; // 演出スキップ用
    private int count;

    [Header("Summary Components")]
    [SerializeField] private Transform summaryGridContent; // 一覧表示の親オブジェクト (Contentなど)
    [SerializeField] private GameObject inventorySlotPrefab; // インベントリスロットのプレハブ
    [SerializeField] private Button backButton; // 研究所に戻るボタンなど

    [Header("Rarity Display")]
    [Tooltip("星を表示するためのImageコンポーネント（5つ）")]
    [SerializeField] private List<Image> rarityStars;
    [SerializeField] private Sprite starSprite;

    // 演出が再生中かどうかを管理するフラグ
    public bool IsPlaying { get; private set; }
    private bool nextButtonPressed = false; // ボタンが押されたかを記録する旗
    private bool skipCurrentAnimation = false; // 演出スキップフラグ

    [Header("効果音")]
    [SerializeField] private AudioClip GOGOGO; // ノーマル演出効果音
    [SerializeField] private AudioClip DODODODO;  // レア演出効果音
    [SerializeField] private AudioClip BAKYUN;  // スーパーレア演出効果音
    // バキュン音を鳴らすかどうかのフラグ
    private bool BakyunPlay = false;

    private void Start()
    {
        // 戻るボタンが押されたら、パネルを非表示にする
        backButton?.onClick.AddListener(() => gachaResultPanel.SetActive(false));
        // 次へボタンを押されたら、次の表示
        nextButton?.onClick.AddListener(OnNextButtonPressed);
        // スキップボタンの予約
        skipButton?.onClick.AddListener(OnSkipButtonPressed);

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
    // スキップする
    public void OnSkipButtonPressed()
    {
        skipCurrentAnimation = true;
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
        count = 1; // カウントをリセット
        skipCurrentAnimation = false;

        // --- 1. 演出の準備 (親パネルと演出パネルを表示) ---
        gachaResultPanel.SetActive(true);
        summaryDisplay.SetActive(false);
        singleResultDisplay.SetActive(false); // 結果はまだ隠す
        effectPanel.SetActive(true);          // 演出パネルを表示
        skipButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);

        // resultsリストから、最も高いrarityの数値を取得する (LINQを使用)
        int maxRarity = 0;
        if (results.Count > 0)
        {
            maxRarity = results.Max(organ => organ.GetRarity());
        }

        // --- 2. 最初の「ゴゴゴ」演出 ---
        // if文で再生するアニメーションを変更
        // まずAnimatorのGameObjectをアクティブにする
        effectAnimator.gameObject.SetActive(true);
        if (maxRarity >= 5)
        {
            AudioManager.Instance.PlayLoopSE1(DODODODO);
            // バキュンを鳴らすフラグ
            BakyunPlay = true;
            effectAnimator.Play("SuperRareEffect");
        }
        else if (maxRarity >= 4)
        {
            AudioManager.Instance.PlayLoopSE1(DODODODO);
            effectAnimator.Play("RareEffect");
        }
        else
        {
            // 途中で停止可能なAudio Sourceで流す
            AudioManager.Instance.PlayLoopSE1(GOGOGO);
            effectAnimator.Play("NormalEffect");
        }

        // 演出が終わる(2秒)か、スキップが押されるまで待つ
        float timer = 0f;
        while(timer < 2.0f && !skipCurrentAnimation)
        {
            timer += Time.deltaTime;
            // バキュンを鳴らすフラグがtrueの場合、鳴らす
            if (timer >= 1.0f && BakyunPlay)
            {
                AudioManager.Instance.PlayLoopSE2(BAKYUN);
                BakyunPlay = false;
            }

            yield return null;
        }
        
        // 演出が終わったらAnimatorを非表示にする
        effectAnimator.gameObject.SetActive(false);
        // 効果音をストップする
        AudioManager.Instance.StopAllLoopSE();

        // --- 3. 演出パネルを非表示にし、結果表示パネルに切り替え ---
        effectPanel.SetActive(false);
        singleResultDisplay.SetActive(true);
        skipButton.gameObject.SetActive(false); // スキップはもう不要
        nextButton.gameObject.SetActive(true);  // 「次へ」ボタンを表示

        // --- 4. ボタンが押されるたびに、次の結果を表示 ---
        foreach (var organ in results)
        {

            singleResultIcon.sprite = organ.GetIcon();
            singleResultName.text = organ.GetName();
            countText.text = count.ToString() + " / " + results.Count; // "1 / 10 連目" のように表示
            count++;

            UpdateRarityStars(organ.GetRarity());

            // 旗を下げて、「次へ」ボタンが押されるのを待つ
            nextButtonPressed = false;
            yield return new WaitUntil(() => nextButtonPressed);
        }

        // --- 5. 最終的な一覧表示 ---
        singleResultDisplay.SetActive(false);
        nextButton.gameObject.SetActive(false);
        summaryDisplay.SetActive(true);
        PopulateSummaryGrid(results);

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

    /// <summary>
    /// レアリティに応じて星の表示（スプライト）を更新する
    /// </summary>
    /// <param name="rarity">レアリティ（例: 1～5）</param>
    private void UpdateRarityStars(int rarity)
    {
        // 5つの星イメージを順番に処理
        for (int i = 0; i < rarityStars.Count; i++)
        {
            if (i < rarity)
            {
                // レアリティの数だけ「満ちた星」のスプライトに差し替える
                rarityStars[i].sprite = starSprite;
                rarityStars[i].enabled = true;
            }
            else
            {  
                // 星自体を非表示にする
                rarityStars[i].enabled = false;
            }
        }
    }
}