using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaManager : MonsterAreaManager
{
    [Header("Enemy Settings")]
    [SerializeField] private List<EnemyMonsterData> enemyDataList;  // 各スポーンポイントに出現する敵に対する敵データ

    /// <summary>
    /// スポーンポイントに敵を生成
    /// </summary>
    public void SpawnEnemies()
    {
        // MonsterAreaManager(親クラス)のSpawnMonsters()を利用して各スポーンポイントに敵を生成
        // enemyDataListにある敵データEnemyMonsterData型のものなので、ConvertAllでリストの全要素を
        // MonsterDataBattleScene型に変換してから親クラスのメソッドSpawnMonsters()を呼び出す
        List<MonsterDataBattleScene> baseDataList = enemyDataList.ConvertAll(data => (MonsterDataBattleScene)data);
        base.SpawnMonsters(baseDataList);
    }


    // indexで指定した敵モンスターがdamege量の攻撃を受けた際にMonsterAreaManagerクラス(親)のApplyDamageメソッドを呼び出して
    // その敵モンスターへのダメージ処理を行うメソッド
    // ↑コメント変更予定

    protected override void ApplyDamage(int index, int damage)
    {
        if (index < 0 || index >= spawnedMonsters.Count) return;  // 生成した敵モンスターリストの範囲外にアクセスした場合は何も返さない

        Enemy enemy = spawnedMonsters[index] as Enemy;
        if (enemy != null)
        {
            enemy.TakeDamagePublic(damage);  // TakeDamageメソッドを呼び出してダメージ処理
        }
    }

    public void DamageEnemy(int index, int damage)
    {
        foreach (var monster in spawnedMonsters) // 生成した各モンスターに対して
        {
            if (monster is Enemy enemy) // 生成したモンスタが敵モンスターである場合
            {
                enemy.TakeDamagePublic(damage);  // TakeDamageメソッドでダメージを与える
            }
        }
    }

    // 生成した各敵モンスターの各々に対してMonsterAreaManagerクラス(親)のTakeDamageメソッドを呼び出して、
    // 全体攻撃によるダメージ処理をそれぞれの敵モンスターに適用するメソッド
    public void DamageAllEnemies(int damage)
    {
        base.ApplyDamageToAll(damage);
    }

    /// <summary>
    /// 指定の敵の現在HPを取得
    /// </summary>
    public int GetEnemyCurrentHP(int index)
    {
        return base.GetCurrentHP(index);
    }

    // isAlive処理を追加予定

    // 現在の敵の数を取得。MonsterAreaManagerクラス(親)のGetMonsterCount()メソッドを呼び出す
    public int GetEnemyCount() => base.GetMonsterCount();
}
