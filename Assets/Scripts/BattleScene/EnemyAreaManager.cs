using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaManager : MonsterAreaManager
{

    public const int NoSelection = -1;   // 敵が選択されていない状態を表す定数

    // 敵が選択(クリック)された時に、その敵モンスターがspawnedMonsters(生成した敵モンスターリスト)のいずれであるかを示すインデックス
    // どの敵を選択している状態かを示すインデックス情報。初期値は敵が選択されていない状態(-1)に。
    private int selectedEnemyIndex = NoSelection;

    [SerializeField] private AudioClip AttackSoundEffect;  // 攻撃の効果音
    private List<int> enemyPowersList;
    private List<EnemyMonsterData> enemyDataList = new List<EnemyMonsterData>();  // 各スポーンポイントに出現する敵に対する敵データ
    private List<DamageOverTimeEffect> damageOverTimeEffects = new List<DamageOverTimeEffect>();  // 進行中の継続ダメージ効果をまとめたリスト

    public void SetEnemyData(List<MonsterData> enemies)
    {
        enemyDataList.Clear();

        foreach (var data in enemies)
        {
            if (data is EnemyMonsterData enemyDat)
                enemyDataList.Add(enemyDat);
        }
    }


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
        // インデックスが生成した敵モンスターリスト範囲外にアクセスしていないかをチェック
        if (index < 0 || index >= spawnedMonsters.Count)
            return;

        // 同じ敵を再度選択すると、選択解除
        if (selectedEnemyIndex == index)
        {
            var enemy = spawnedMonsters[index] as Enemy;
            enemy?.SetTargetMarkVisible(false);

            Log($"{spawnedMonsters[selectedEnemyIndex].GetMonsterName()} の選択を解除", BattleLogType.System);  // 元々選択されていた敵の選択を解除したメッセージをログに表示。
            selectedEnemyIndex = NoSelection;
            return;
        }

        // 前の敵の選択を解除
        if (selectedEnemyIndex != NoSelection)
        {
            var prevEnemy = spawnedMonsters[selectedEnemyIndex] as Enemy;
            prevEnemy?.SetTargetMarkVisible(false);
        }

        // 新しく選択した敵に対するターゲット処理
        selectedEnemyIndex = index;// クリックした敵の選択インデックス情報を保持。

        var newEnemy = spawnedMonsters[selectedEnemyIndex] as Enemy;
        newEnemy?.SetTargetMarkVisible(true);

        Log($"{spawnedMonsters[selectedEnemyIndex].GetMonsterName()}を選択", BattleLogType.System); // 選択した旨をメッセージとしてログに追加
    }

    // 敵モンスター単体に対する攻撃処理用メソッド
    public void TakeDamageToSelectedEnemy(int targetIndex, int damage)
    {
        if (spawnedMonsters.Count == 0) return;

        // targetIndex = selectedEnemyIndex;

        // プレイヤーが敵を選択していない、またはプレイヤーが選択している敵がすでに死亡していたら
        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count || spawnedMonsters[targetIndex].GetIsDead())
        {
            // 敵を選択していない状態にして、ターゲットをランダムに決める
            selectedEnemyIndex = NoSelection;
            targetIndex = GetRandomAliveEnemyIndex();
        }

        if (targetIndex == NoSelection) return;  // ランダム取得に失敗

        // 選択された敵1体に対するダメージ処理
        ApplyDamage(targetIndex, damage);
        // ダメージが与えられた旨を、その敵の名前とともにログに追加
        Log($"{spawnedMonsters[targetIndex].GetMonsterName()}に{damage}ダメージ!", BattleLogType.Attack);
        LogIfDead();
    }

    // indexで指定した敵モンスターがdamege量の攻撃を受けた際にMonsterAreaManagerクラス(親)のApplyDamageメソッドを呼び出して
    // その敵モンスターへのダメージ処理を行うメソッド
    protected override void ApplyDamage(int targetIndex, int damage)
    {
        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count) return;  // 生成した敵モンスターリストの範囲外にアクセスした場合は何も返さない

        Enemy enemy = spawnedMonsters[targetIndex] as Enemy;
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
            if (monster is Enemy enemy && !enemy.GetIsDead()) // 生成したモンスタが敵モンスターで、そのモンスターが死んでいない場合
            {
                enemy.TakeDamagePublic(damage);  // TakeDamagePublicメソッドでダメージを与える
            }
        }
    }

    // 敵全体にダメージを与える処理を、外部から呼び出すためのラッパーメソッド
    public void TakeDamageToAll(int damage)
    {
        this.ApplyDamageToAll(damage);
        // 敵全体にダメージが入ったことをメッセージとしてログに追加
        Log($"敵全体に{damage}ダメージ!", BattleLogType.Attack);
        LogIfDead();
    }

    public void ApplyDamageOverTimeToSelectedEnemy(int damageAmount, int durationTurns)
    {
        if (spawnedMonsters.Count == 0) return;

        int targetIndex = selectedEnemyIndex;

        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count || spawnedMonsters[targetIndex].GetIsDead())
        {
            // 敵を選択していない状態にして、ターゲットをランダムに決める
            selectedEnemyIndex = NoSelection;
            targetIndex = GetRandomAliveEnemyIndex();
        }

        if (targetIndex == NoSelection) return;

        // 継続ターン数durationTurnsを、[durationTurns, durationTurns+2]の範囲でランダム化
        int randomizedTurns = UnityEngine.Random.Range(durationTurns, durationTurns + 3);

        damageOverTimeEffects.Add(new DamageOverTimeEffect(targetIndex, damageAmount, randomizedTurns));  // 新たな継続ダメージ効果として保持

        Log($"{spawnedMonsters[targetIndex].GetMonsterName()}に継続ダメージ付与！：{randomizedTurns}ターン", BattleLogType.DamageOverTime);
    }

    // 残っている継続ダメージ効果を適用するメソッド(ダメージ量はターンごとに変動)
    public void ProcessDamageOverTime()
    {
        if (damageOverTimeEffects.Count == 0) return;  // 適用する継続ダメージ効果が残っていなければ何もしない

        List<DamageOverTimeEffect> expiredEffects = new List<DamageOverTimeEffect>(); // 期限切れの効果を保持(効果削除用)

        // 現在適用中の継続ダメージの効果を発動する
        foreach (var dotEffect in damageOverTimeEffects)
        {
            if (dotEffect.targetIndex < 0 || dotEffect.targetIndex >= spawnedMonsters.Count)
            {
                expiredEffects.Add(dotEffect);
                continue;
            }

            // 継続ダメージ量を[dotEffect.damagePerTurn * 0.8, dotEffect.damagePerTurn*1.5]の範囲でランダム化
            int randomizedDamage = Mathf.RoundToInt(dotEffect.damagePerTurn * Random.Range(0.8f, 1.5f));

            // 効果の対象の敵が死んでいなければ、ダメージを与える
            var monster = spawnedMonsters[dotEffect.targetIndex];

            if (monster.GetIsDead())
            {
                expiredEffects.Add(dotEffect);
                continue;
            }

            ApplyDamage(dotEffect.targetIndex, randomizedDamage);
            AudioManager.Instance.PlaySE(AttackSoundEffect);
            Log($"{monster.GetMonsterName()}に継続ダメージ {randomizedDamage}!", BattleLogType.DamageOverTime);

            dotEffect.remainingTurns--;   // 継続ダメージを適用する残りターン数を減らす

            // 継続ダメージの残りターン数が0になると、効果が切れる
            if (dotEffect.remainingTurns <= 0)
            {
                expiredEffects.Add(dotEffect);
                Log($"{monster.GetMonsterName()}の継続ダメージの効果が切れた！", BattleLogType.Attention);
            }
        }

        // 期限が切れた効果は削除する
        foreach (var expEffect in expiredEffects)
        {
            damageOverTimeEffects.Remove(expEffect);
        }

        LogIfDead();
    }


    /// 指定の敵の現在HPを取得
    public int GetCurrentHP(int index)
    {
        if (index < 0 || index >= spawnedMonsters.Count) return 0;
        return spawnedMonsters[index].GetCurrentHP();
    }

    // 敵モンスターが全員死亡している状態であるかを取得するメソッド
    // trueなら 敵モンスターは全滅している状態
    public bool GetIsAllMonstersDead()
    {
        bool isAllMonstersDead = true;  // 全てのモンスターが死亡しているかどうかのフラグ

        // 生成した各敵モンスターについて、死亡しているかどうかの確認
        foreach (var enemyMonster in spawnedMonsters)
        {
            if (!enemyMonster.GetIsDead()) //死亡していなければ
            {
                isAllMonstersDead = false;
                break;
            }
        }
        return isAllMonstersDead;
    }

    // 生存している敵の中から、ランダムに(spawnedMonstersにおける)インデックスを返すメソッド
    private int GetRandomAliveEnemyIndex()
    {
        int monsterIndex = 0;
        var enemyAliveList = new List<int>();

        foreach (var monster in spawnedMonsters)
        {
            if (!monster.GetIsDead())
            {
                enemyAliveList.Add(monsterIndex);
            }
            monsterIndex++;
        }

        if (enemyAliveList.Count == 0) return NoSelection;  // 全敵が死んでいた場合は、敵が選択されていない状態のインデックス

        return enemyAliveList[Random.Range(0, enemyAliveList.Count)];
    }



    // 引数で指定されたワールド/ローカル座標に最も近い生存している敵のインデックスを返すメソッド。
    // 生存している敵がいなければ NoSelection を返す
    public int GetNearestEnemyIndex(Vector2 position)
    {
        if (spawnedMonsters.Count == 0) return NoSelection;

        int nearestEnemyIndex = NoSelection;
        float minDistance = float.MaxValue;

        for (int i = 0; i < spawnedMonsters.Count; i++)
        {
            var monster = spawnedMonsters[i];
            if (monster == null || monster.GetIsDead()) continue;

            // モンスターと引数で指定された座標との距離を測る
            Vector2 monsterPosition = monster.transform.position;
            float distance = Vector2.Distance(position, monsterPosition);


            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemyIndex = i;
            }
        }

        return nearestEnemyIndex;
    }

    // カードをドラッグしているときに近い敵をハイライト
    public void HighlightNearestEnemy(Vector2 screenPosition)
    {
        if (spawnedMonsters.Count == 0) return;

        int nearestIndex = GetNearestEnemyIndex(screenPosition);

        // まず全てのハイライトを消す
        foreach (var monster in spawnedMonsters)
        {
            monster.StopHighlight();
        }

        // 最も近い敵にハイライト
        if (nearestIndex != NoSelection && spawnedMonsters[nearestIndex] is Enemy nearestEnemy)
        {
            nearestEnemy.StartHighlight();
        }
    }

    public void HighlightAllEnemies()
    {
        foreach(var monster in spawnedMonsters)
        {
            if (!monster.GetIsDead())
            monster.StartHighlight();
        }
    }

    // ドラッグ終了時に全てのハイライトを消す
    public void ClearAllHighlights()
    {
        foreach (var monster in spawnedMonsters)
        {
            monster.StopHighlight();
        }
    }

    // 敵が倒れたら、それに応じたログを出すメソッド
    private void LogIfDead()
    {
        // 各敵に対して、死亡ログの表示が必要かどうかを判断する
        foreach (var enemyMonster in spawnedMonsters)
        {
            // 該当の敵が死亡しており、その旨を伝えるログが既に表示されている場合は、
            // IsShouldDeathLogged()がfalseになるので、ログの重複表示がされない
            if (enemyMonster is Enemy enemy && enemy.IsShouldDeathLogged())
            {
                Log($"{enemy.GetMonsterName()}は倒れた！", BattleLogType.Attention);
            }
        }
    }


    public void PrepareEnemyAttackAmounts()
    {
        enemyPowersList = new List<int>();
        foreach (var enemyMonster in spawnedMonsters)
        {
            // 既に該当のモンスターが倒れていたら、そのモンスターの攻撃処理は行わない
            if (enemyMonster.GetIsDead())
            {
                continue;
            }
            // Enemy型の敵データにセットされている攻撃力attackPowerを入手し、
            // [attackPowe-10, attackPoewr+30]の範囲でランダムに「味方HPに与えるダメージ量(attackPowerAmount)」を決める。
            int baseAttackPower = enemyMonster.GetAttackPower();
            int attackPowerAmount = UnityEngine.Random.Range(baseAttackPower - 10, baseAttackPower + 41);
            enemyPowersList.Add(attackPowerAmount);
        }
    }

    // 現在の敵の数を取得。MonsterAreaManagerクラス(親)のGetMonsterCount()メソッドを呼び出す
    public int GetEnemyCount() => base.GetMonsterCount();

    public IReadOnlyList<int> GetEnemyPowersList() => enemyPowersList;  // 各敵の攻撃量(attackPowerAmount)を保持したリストを外部クラスから参照するためのメソッド

    public int GetSelectedEnemyIndex() => this.selectedEnemyIndex;

    public IReadOnlyList<string> GetAliveEnemyNamesList()
    {
        var aliveEnemyNamesList = new List<string>();

        foreach (var monster in spawnedMonsters)
        {
            if (monster is Enemy enemy && !enemy.GetIsDead())
            {
                aliveEnemyNamesList.Add(enemy.GetMonsterName());
            }
        }
        return aliveEnemyNamesList;
    }


    // 継続ダメージ効果用のクラスデータ構造(内部クラス)
    private class DamageOverTimeEffect
    {
        public int targetIndex;      // 継続ダメージを与える対象の敵のインデックス
        public int damagePerTurn;    // 1ターン当たりのダメージ量
        public int remainingTurns;  // 残り継続ターン数

        // コンストラクタ
        public DamageOverTimeEffect(int targetIndex, int damagePerTurn, int remainingTurns)
        {
            this.targetIndex = targetIndex;
            this.damagePerTurn = damagePerTurn;
            this.remainingTurns = remainingTurns;
        }
    }
}
