using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaSystem : MonoBehaviour
{
    [Header("UI Components")]
    public Button normalGachaButton;    // 通常ガチャ（1回）
    public Button normalGachaButton10;  // 通常ガチャ（10回）
    public Button premiumGachaButton;   // プレミアムガチャ（1回）
    [SerializeField] private TextMeshProUGUI myPointsText;

    [Header("Normal Gacha Settings")]
    [Tooltip("通常ガチャのプールを確率の高い順に設定")]
    public List<GachaPool> normalGachaPools;
    public int normalGachaCost = 100;

    [Header("Premium Gacha Settings")]
    [Tooltip("プレミアムガチャのプールを確率の高い順に設定")]
    public List<GachaPool> premiumGachaPools;
    public int premiumGachaCost = 500;

    [Header("Dependencies")]
    public GachaDirector gachaDirector;

    void Start()
    {
        // 各ボタンに、対応するガチャを実行する処理をラムダ式で登録
        normalGachaButton?.onClick.AddListener(() => ExecuteGacha(normalGachaPools, normalGachaCost, 1));
        normalGachaButton10?.onClick.AddListener(() => ExecuteGacha(normalGachaPools, normalGachaCost * 10, 10));
        premiumGachaButton?.onClick.AddListener(() => ExecuteGacha(premiumGachaPools, premiumGachaCost, 1));
        myPointsText.text = GameManager.Instance.PlayerData.GetPoints().ToString() + "pt";
    }

    /// <summary>
    /// ガチャ実行のメインロジック
    /// </summary>
    /// <param name="poolsToUse">使用するガチャプールのリスト</param>
    /// <param name="totalCost">消費する合計ポイント</param>
    /// <param name="pullCount">ガチャを引く回数</param>
    public void ExecuteGacha(List<GachaPool> poolsToUse, int totalCost, int pullCount)
    {
        // 1. ポイントが足りるか確認
        if (!GameManager.Instance.PlayerData.UsePoints(totalCost))
        {
            Debug.Log("研究ポイントが足りません！");
            return;
        }
        myPointsText.text = GameManager.Instance.PlayerData.GetPoints().ToString() + "pt";

        Debug.Log($"--- {pullCount}連ガチャ開始！ ---");

        // 2. 指定された回数だけ抽選を繰り返す
        List<OrganData> results = new List<OrganData>();
        for (int i = 0; i < pullCount; i++)
        {
            GachaPool selectedPool = SelectGachaPool(poolsToUse);
            if (selectedPool == null) continue;

            OrganData resultOrgan = DrawOrganFromPool(selectedPool);
            if (resultOrgan == null) continue;

            results.Add(resultOrgan); // 結果をリストに保存
            GameManager.Instance.PlayerData.AddOrgan(resultOrgan, 1);
            Debug.Log($"結果 {i + 1}: {resultOrgan.GetName()}");
        }

        // 3. 演出ディレクターに結果リストを渡して演出を開始させる
        gachaDirector?.PlayRevealAnimation(results);
    }

    /// <summary>
    /// どのレアリティプールに当選したかを抽選する
    /// </summary>
    private GachaPool SelectGachaPool(List<GachaPool> pools)
    {
        int totalProbability = 0;
        foreach (var pool in pools) { totalProbability += pool.probability; }

        int randomValue = Random.Range(1, totalProbability + 1);
        
        int cumulative = 0;
        foreach (var pool in pools)
        {
            cumulative += pool.probability;
            if (randomValue <= cumulative)
            {
                return pool;
            }
        }
        return null;
    }

    /// <summary>
    /// 当選したプールの中から臓器を1つ抽選する
    /// </summary>
    private OrganData DrawOrganFromPool(GachaPool pool)
    {
        if (pool.items.Count == 0) return null;
        int randomIndex = Random.Range(0, pool.items.Count);
        return pool.items[randomIndex];
    }
}