using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 味方モンスター3体をスポーン・管理するクラス
/// EnemyAreaManager と同様の構造
/// さらに3体のHPを合算して共有HPとして管理
/// </summary>
public class AllyAreaManager : MonoBehaviour
{
    [Header("Ally Settings")]
    [SerializeField] private GameObject allyPrefab;           // 味方プレハブ
    [SerializeField] private Transform[] spawnPoints;         // 味方出現位置（Canvas内）
    [SerializeField] private List<AllyMonsterData> allyDataList; // 3体の味方データ

    [Header("UI")]
    [SerializeField] private HpGaugeController sharedHpGauge; // 共有HPゲージ

    private List<Ally> spawnedAllies = new List<Ally>();

    private int sharedMaxHP;      // 3体の最大HP合算
    private int sharedCurrentHP;  // 現在の共有HP

    /// <summary>
    /// 味方モンスターをスポーン
    /// </summary>
    public void SpawnAllies()
    {
        ClearAllies();

        int dataIndex = 0;

        foreach (var point in spawnPoints)
        {
            GameObject allyObj = Instantiate(allyPrefab, point);

            RectTransform allyRect = allyObj.GetComponent<RectTransform>();
            RectTransform pointRect = point.GetComponent<RectTransform>();

            if (allyRect != null && pointRect != null)
            {
                allyRect.sizeDelta = pointRect.sizeDelta;
                allyRect.anchoredPosition = Vector2.zero;
                allyRect.localScale = Vector3.one;
            }

            Ally ally = allyObj.GetComponent<Ally>();

            if (ally != null && dataIndex < allyDataList.Count && allyDataList[dataIndex] != null)
            {
                AllyMonsterData allyData = allyDataList[dataIndex];
                ally.InitializeSet(allyData);

                spawnedAllies.Add(ally);
            }

            dataIndex++;
        }

        InitializeSharedHP();
    }

    /// <summary>
    /// 共有HPを初期化
    /// </summary>
    private void InitializeSharedHP()
    {
        sharedMaxHP = 0;
        foreach (var ally in spawnedAllies)
        {
            sharedMaxHP += ally.GetMaxHP(); // 各味方の最大HPを合算
        }
        sharedCurrentHP = sharedMaxHP;

        if (sharedHpGauge != null)
        {
            sharedHpGauge.SetMaxHP(sharedMaxHP);
        }
    }

    /// <summary>
    /// 共有HPにダメージ
    /// </summary>
    public void TakeDamage(int amount)
    {
        sharedCurrentHP = Mathf.Max(sharedCurrentHP - amount, 0);

        if (sharedHpGauge != null)
        {
            sharedHpGauge.BeInjured(amount);
        }
    }

    /// <summary>
    /// 共有HPを回復
    /// </summary>
    public void Heal(int amount)
    {
        sharedCurrentHP = Mathf.Min(sharedCurrentHP + amount, sharedMaxHP);

        if (sharedHpGauge != null)
        {
            // 回復分をゲージに反映
            // BeInjured はダメージ用なので、回復用に別メソッドを作るのもあり
            sharedHpGauge.SetMaxHP(sharedMaxHP); // ゲージを更新（簡易対応）
        }
    }

    public int GetSharedCurrentHP() => sharedCurrentHP;
    public int GetSharedMaxHP() => sharedMaxHP;

    public List<Ally> GetAllies() => spawnedAllies;
    public int GetAllyCount() => spawnedAllies.Count;

    /// <summary>
    /// 既存の味方を削除
    /// </summary>
    public void ClearAllies()
    {
        foreach (var ally in spawnedAllies)
        {
            if (ally != null) Destroy(ally.gameObject);
        }
        spawnedAllies.Clear();
    }
}
