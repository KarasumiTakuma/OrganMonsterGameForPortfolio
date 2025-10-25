using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;       // 敵プレハブ
    [SerializeField] private Transform[] spawnPoints;      // 敵出現位置（EnemyAreaの子）
    [SerializeField] private int enemyMaxHP = 30;          // 敵の最大HP
    [SerializeField] private Canvas canvas;               // UI CanvasをInspectorでセット

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<HpGaugeController> hpControllers = new List<HpGaugeController>();

    /// <summary>
    /// 敵を全てスポーン
    /// </summary>
    public void SpawnEnemies()
    {
        // 既存の敵がいたら削除
        foreach (var enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        spawnedEnemies.Clear();
        hpControllers.Clear();

        // 各スポーンポイントに敵を生成
        foreach (var point in spawnPoints)
        {
            // enemyオブジェクトはそのスポーン位置(point)を親とする
            GameObject enemy = Instantiate(enemyPrefab, point);

            // enemyオブジェとそのスポーンポイント(point)のRectTransformを取得
            RectTransform enemyRect = enemy.GetComponent<RectTransform>();
            RectTransform pointRect = point.GetComponent<RectTransform>();

            if (enemyRect != null && pointRect != null) //RectTransformが空値でないかを確認
            {   
                //Inspector上で各オブジェクトのサイズ設定を誤っても正しいサイズにするため
                
                // enemyオブジェクトのサイズをspawnPointに合わせる
                enemyRect.sizeDelta = pointRect.sizeDelta;

                // 各敵オブジェクトの位置をそれぞれの親であるpointのpivot位置(中心)にする
                enemyRect.anchoredPosition = Vector2.zero;

                // enemyのサイズは1に固定
                enemyRect.localScale = Vector3.one;
            }

            // HpGaugeControllerを取得して最大HPを設定
            HpGaugeController hpCtrl = enemy.GetComponentInChildren<HpGaugeController>();
            if (hpCtrl != null)
            {
                hpCtrl.SetMaxHP(enemyMaxHP);
                hpControllers.Add(hpCtrl);
            }

            spawnedEnemies.Add(enemy);
        }
    }


    /// <summary>
    /// 指定の敵にダメージを与える
    /// </summary>
    public void DamageEnemy(int index, int damage)
    {
        if (index < 0 || index >= hpControllers.Count) return;

        hpControllers[index].BeInjured(damage);
    }

    /// <summary>
    /// 敵全体にダメージ（全体攻撃用）
    /// </summary>
    public void DamageAllEnemies(int damage)
    {
        foreach (var hp in hpControllers)
        {
            hp.BeInjured(damage);
        }
    }

    /// <summary>
    /// 指定の敵の現在HPを取得
    /// </summary>
    public int GetEnemyCurrentHP(int index)
    {
        if (index < 0 || index >= hpControllers.Count) return 0;
        return hpControllers[index].GetCurrentHP();
    }

    /// <summary>
    /// 現在スポーンしている敵の数を取得
    /// </summary>
    public int GetEnemyCount()
    {
        return spawnedEnemies.Count;
    }

    /// <summary>
    /// 指定の敵が生きているかどうか
    /// </summary>
    public bool IsEnemyAlive(int index)
    {
        if (index < 0 || index >= hpControllers.Count) return false;
        return hpControllers[index].GetCurrentHP() > 0;
    }


    /// <summary>
    /// 既存の敵を削除
    /// </summary>
    public void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        spawnedEnemies.Clear();
        hpControllers.Clear();
    }
}
