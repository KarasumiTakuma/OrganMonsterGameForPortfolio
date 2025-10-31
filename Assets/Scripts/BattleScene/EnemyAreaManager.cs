using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;       // 敵プレハブ
    [SerializeField] private Transform[] spawnPoints;      // 敵出現位置（EnemyAreaの子）
    [SerializeField] private List<EnemyMonsterData> enemyDataList;  // 各スポーンポイントに出現する対応する敵データ

    // 生成済みのEnemyスクリプトのリスト。SpawnEnemies()で生成したEnemyたちのオブジェクトを保持
    private List<Enemy> spawnedEnemies = new List<Enemy>();


    /// <summary>
    /// スポーンポイントに敵を生成
    /// </summary>
    public void SpawnEnemies()
    {
        // 既存の敵がいたら削除
        ClearEnemies();

        int dataIndex = 0; // enemyDataList 参照用カウンタ

        // 各スポーンポイントに敵を生成
        foreach (var point in spawnPoints)
        {

            // enemyObjはそのスポーン位置(point)を親とする
            GameObject enemyObj = Instantiate(enemyPrefab, point);

            // enemyオブジェとそのスポーンポイント(point)のRectTransformを取得
            RectTransform enemyRect = enemyObj.GetComponent<RectTransform>();
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

            // enemyObjのEnemyPrefabからEnemyスクリプト(敵データ)を取得
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            // 敵データ
            if (enemy != null) // enemyObjのenemyPrefabにEnemyスクリプトがアタッチされてるか?
            {
                // データが存在すれば反映
                if (dataIndex < enemyDataList.Count && enemyDataList[dataIndex] != null)
                {
                    // あらかじめ設定された敵データをenemyDataListから取得
                    EnemyMonsterData enemyData = enemyDataList[dataIndex];

                    // この敵(enemy)の各種データ(HPや画像など)をセットする(スポーン時はcurrentHPは最大体力)
                    enemy.InitializeSet(enemyData);
                }

                spawnedEnemies.Add(enemy);
            }
            dataIndex++;
        }
    }


    /// <summary>
    /// 指定の敵にダメージ
    /// </summary>
    public void DamageEnemy(int index, int damage)
    {
        if (index < 0 || index >= spawnedEnemies.Count) return;
        spawnedEnemies[index].TakeDamage(damage);
    }


    /// <summary>
    /// 全敵にダメージ
    /// </summary>
    public void DamageAllEnemies(int damage)
    {
        foreach (var enemy in spawnedEnemies)
        {
            enemy.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 指定の敵の現在HPを取得
    /// </summary>
    public int GetEnemyCurrentHP(int index)
    {
        if (index < 0 || index >= spawnedEnemies.Count) return 0;
        return spawnedEnemies[index].GetCurrentHP();
    }

    /// <summary>
    /// 敵の生存確認
    /// </summary>
    public bool IsEnemyAlive(int index)
    {
        if (index < 0 || index >= spawnedEnemies.Count) return false;
        return spawnedEnemies[index].IsAlive();
    }


    public int GetEnemyCount() => spawnedEnemies.Count;


    /// <summary>
    /// 既存の敵を削除
    /// </summary>
    public void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        spawnedEnemies.Clear();
    }
}
