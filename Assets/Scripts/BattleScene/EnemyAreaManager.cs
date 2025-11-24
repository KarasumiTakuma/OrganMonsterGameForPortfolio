using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaManager : MonsterAreaManager
{
    [Header("Enemy Settings")]
    [SerializeField] private List<EnemyMonsterData> enemyDataList;  // 各スポーンポイントに出現する敵に対する敵データ

    public const int NoSelection = -1;   // 敵が選択されていない状態を表す定数

    // 敵が選択(クリック)された時に、その敵モンスターがspawnedMonsters(生成した敵モンスターリスト)のいずれであるかを示すインデックス
    // どの敵を選択している状態かを示すインデックス情報を保持する変数
    private int selectedEnemyIndex = NoSelection;

    private List<int> enemyPowersList;

    /// <summary>
    /// スポーンポイントに敵を生成
    /// </summary>
    public void SpawnEnemies()
    {
        // MonsterAreaManager(親クラス)のSpawnMonsters()を利用して各スポーンポイントに敵を生成
        // enemyDataListにある敵データEnemyMonsterData型のものなので、ConvertAllでリストの全要素を
        // MonsterData型に変換してから親クラスのメソッドSpawnMonsters()を呼び出す
        List<MonsterData> baseDataList = enemyDataList.ConvertAll(data => (MonsterData)data);
        base.SpawnMonsters(baseDataList);
    }

    // 敵がクリックされたときに発生する処理メソッド。
    // 敵の選択状態を更新する
    public void UpdateSelectedEnemy(int index)
    {
        if (index >= 0 && index < spawnedMonsters.Count) // インデックスが生成した敵モンスターリスト範囲外にアクセスしていないかをチェック
        {
            selectedEnemyIndex = index; // クリックした敵の選択インデックス情報を保持。
            Debug.Log(spawnedMonsters[selectedEnemyIndex].GetMonsterName() + " を選択");
        }
        else
        {
            Debug.Log(spawnedMonsters[selectedEnemyIndex].GetMonsterName() + "の選択を解除");  // 元々選択されていた敵の選択解除を報告してから
            selectedEnemyIndex = index; // 敵の選択インデックス情報を更新
        }
    }

    // 敵モンスター単体に対する攻撃処理用メソッド
    public void TakeDamageToSelectedEnemy(int damage)
    {
        if (spawnedMonsters.Count == 0) return;
        
        int targetIndex = selectedEnemyIndex;

        // プレイヤーが敵を選択していなければ
        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count)
        {   
            targetIndex = UnityEngine.Random.Range(0, spawnedMonsters.Count);  // ランダムで1体を選択
        }

        // 選択された敵1体に対するダメージ処理
        ApplyDamage(targetIndex, damage);
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

    // 生成した各敵モンスターの各々に対してMonsterAreaManagerクラス(親)のTakeDamageメソッドを呼び出して、
    // 全体攻撃によるダメージ処理をそれぞれの敵モンスターに適用するメソッド
    protected override void ApplyDamageToAll(int damage)
    {
        foreach (var monster in spawnedMonsters) // 生成した各モンスターに対して
        {
            if (monster is Enemy enemy) // 生成したモンスタが敵モンスターである場合
            {
                enemy.TakeDamagePublic(damage);  // TakeDamagePublicメソッドでダメージを与える
            }
        }
    }

    public void TakeDamageToAll(int damage)
    {
        this.ApplyDamageToAll(damage);
    }

    /// <summary>
    /// 指定の敵の現在HPを取得
    /// </summary>
    public int GetCurrentHP(int index)
    {
        if (index < 0 || index >= spawnedMonsters.Count) return 0;
        return spawnedMonsters[index].GetCurrentHP();
    }

    public int GetIsAliveMonsterCount()
    {
        int count = 0;
        foreach (var enemyMonster in spawnedMonsters)
        {
            if (enemyMonster.GetCurrentHP() > 0)
            {
                count++;
            }
        }
        return count;
    }

    public void EnemyRundomPowers()
    {
        enemyPowersList = new List<int>();
        foreach(var enemyMonster in spawnedMonsters)
        {
            // Enemy型の敵データにセットされている攻撃力attackPowerを入手し、
            // [attackPower-3, attackPoewr+3]の範囲でランダムに「味方HPに与えるダメージ量(attackPowerAmount)」を決める。
            int baseAttackPower = enemyMonster.GetAttackPower();  
            int attackPowerAmount = UnityEngine.Random.Range(baseAttackPower-3, baseAttackPower+4);
            enemyPowersList.Add(attackPowerAmount);
        }
    }

    // 現在の敵の数を取得。MonsterAreaManagerクラス(親)のGetMonsterCount()メソッドを呼び出す
    public int GetEnemyCount() => base.GetMonsterCount();

    public List<int> GetEnemyPowersList() => enemyPowersList;

    public int GetSelectedEnemyIndex() => this.selectedEnemyIndex;
}