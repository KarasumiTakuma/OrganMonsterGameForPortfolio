using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵モンスター配置エリアの管理クラス。
/// 敵の生成、ターゲット選択、ダメージ処理、継続ダメージ管理などを担当。
/// </summary>
public class EnemyAreaManager : MonsterAreaManager
{
    /// <summary> 敵が選択されていない状態を表すインデックス </summary>
    public const int NoSelection = -1;

    /// <summary> 現在選択中の敵インデックス。spawnPoints / spawnedMonsters に対応。</summary>
    private int clickedEnemyIndex = NoSelection;

    /// <summary> 攻撃の効果音 </summary>
    [SerializeField] private AudioClip attackSoundEffect;

    /// <summary> 各敵の攻撃量キャッシュ用リスト </summary>
    private List<int> enemyAttackDamagesList;

    /// <summary> スポーン用の敵データリスト </summary>
    private List<EnemyMonsterData> enemyDataList = new List<EnemyMonsterData>();

    /// <summary> 継続ダメージ効果管理リスト </summary>
    private List<DamageOverTimeEffect> damageOverTimeEffects = new List<DamageOverTimeEffect>();

    /// <summary> /ドラッグ自動ターゲットモードかどうか </summary>
    private bool IsDragAutoMode =>
        SettingsManager.Instance.CurrentTargetingMode == TargetingMode.DragAutoTarget;
    
    /// <summary> 敵クリック選択モードか </summary>
    private bool IsClickedTargetMode =>
        SettingsManager.Instance.CurrentTargetingMode == TargetingMode.ClickedTarget;

    /// <summary>
    /// 敵データを設定。
    /// </summary>
    /// <param name="enemies"> 登録するモンスターデータ一覧 </param>
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
    /// 敵をスポーンポイントへ生成。
    /// </summary>
    public void SpawnEnemies()
    {
        // 親クラス用に型変換
        List<MonsterData> baseDataList = enemyDataList.ConvertAll(data => (MonsterData)data);
        base.SpawnMonsters(baseDataList);
    }

    /// <summary>
    /// 指定の敵へダメージ適用。
    /// </summary>
    /// <param name="targetIndex"> 対象の敵インデックス </param>
    /// <param name="damage"> ダメージ量 </param>
    public void TakeDamageToTargetEnemy(int targetIndex, int damage)
    {
        if (spawnedMonsters.Count == 0) return;

        // モードに応じてターゲットを確定
        if (IsClickedTargetMode)
        {
            targetIndex = clickedEnemyIndex;
        }

        // 無効なターゲット(プレイヤーが敵を選択していない、または選択した敵が既に死亡)を補正
        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count || spawnedMonsters[targetIndex].GetIsDead())
        {
            clickedEnemyIndex = NoSelection;
            targetIndex = GetRandomAliveEnemyIndex();
        }

        if (targetIndex == NoSelection) return;  // ランダム取得に失敗

        ApplyDamageToTargetEnemy(targetIndex, damage);

        Log($"{spawnedMonsters[targetIndex].GetMonsterName()}に{damage}ダメージ!", BattleLogType.Attack);

        LogIfDead();
    }

    /// <summary>
    /// 単体敵ダメージ処理。
    /// </summary>
    private void ApplyDamageToTargetEnemy(int targetIndex, int damage)
    {
        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count) return;

        Enemy enemy = spawnedMonsters[targetIndex] as Enemy;
        if (enemy != null)
        {
            enemy.TakeDamagePublic(damage);
        }
    }

    /// <summary>
    /// 敵全体へのダメージ処理。
    /// </summary>
    protected override void ApplyDamageToAllMonsters(int damage)
    {
        foreach (var monster in spawnedMonsters)
        {
            if (monster is Enemy enemy && !enemy.GetIsDead())
            {
                enemy.TakeDamagePublic(damage);
            }
        }
    }

    /// <summary>
    /// 外部用の敵全体へのダメージを呼び出すラッパーメソッド
    /// </summary>
    /// <param name="damage"> ダメージ量 </param>
    public void TakeDamageToAll(int damage)
    {
        this.ApplyDamageToAllMonsters(damage);
        Log($"敵全体に{damage}ダメージ!", BattleLogType.Attack);
        LogIfDead();
    }

    /// <summary>
    /// 指定した敵への継続ダメージ付与。
    /// </summary>
    /// <param name="targetIndex"> 対象敵 </param>
    /// <param name="damageAmount"> ターン毎ダメージ </param>
    /// <param name="durationTurns"> 継続ターン数 </param>
    public void ApplyDamageOverTimeToTargetEnemy(int targetIndex, int damageAmount, int durationTurns)
    {
        if (spawnedMonsters.Count == 0) return;

        if (IsClickedTargetMode)
        {
            targetIndex = clickedEnemyIndex;
        }


        if (targetIndex < 0 || targetIndex >= spawnedMonsters.Count || spawnedMonsters[targetIndex].GetIsDead())
        {
            clickedEnemyIndex = NoSelection;
            targetIndex = GetRandomAliveEnemyIndex();
        }

        if (targetIndex == NoSelection) return;

        // 継続ターン数durationTurnsを、[durationTurns, durationTurns+2]の範囲でランダム化
        int randomizedTurns = UnityEngine.Random.Range(durationTurns, durationTurns + 3);

        damageOverTimeEffects.Add(new DamageOverTimeEffect(targetIndex, damageAmount, randomizedTurns));  // 新たな継続ダメージ効果として保持

        Log($"{spawnedMonsters[targetIndex].GetMonsterName()}に継続ダメージ付与！：{randomizedTurns}ターン", BattleLogType.DamageOverTime);
    }


    /// <summary>
    /// 継続継続ダメージ効果を処理。(ダメージ量はターンごとに変動)
    /// </summary>
    
    public void ProcessDamageOverTime()
    {
        if (damageOverTimeEffects.Count == 0) return;

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

            ApplyDamageToTargetEnemy(dotEffect.targetIndex, randomizedDamage);
            AudioManager.Instance.PlaySE(attackSoundEffect);
            Log($"{monster.GetMonsterName()}に継続ダメージ {randomizedDamage}!", BattleLogType.DamageOverTime);

            dotEffect.remainingTurns--;

            // 継続ダメージの残りターン数が0になると、効果が切れる
            if (dotEffect.remainingTurns <= 0)
            {
                expiredEffects.Add(dotEffect);
                Log($"{monster.GetMonsterName()}の継続ダメージの効果が切れた！", BattleLogType.Attention);
            }
        }

        // 期限が切れた効果は削除
        foreach (var expEffect in expiredEffects)
        {
            damageOverTimeEffects.Remove(expEffect);
        }

        LogIfDead();
    }

    /// <summary> 継続ダメージの全解除処理 </summary>
    public void ClearAllDamageOverTime()
    {
        damageOverTimeEffects.Clear();
    }


    /// <summary>
    /// 指定の敵HP取得。
    /// </summary>
    /// <param name="index"> 対象の敵 </param>
    /// <returns>現在HP</returns>
    public int GetCurrentHP(int index)
    {
        if (index < 0 || index >= spawnedMonsters.Count) return 0;
        return spawnedMonsters[index].GetCurrentHP();
    }

    /// <summary>
    /// 敵全滅判定用のラッパーメソッド
    /// </summary>
    /// <returns>全滅していれば true</returns>
    public bool GetIsAllMonstersDead()
    {
        bool isAllMonstersDead = true;  // 全てのモンスターが死亡しているかどうかのフラグ

        foreach (var enemyMonster in spawnedMonsters)
        {
            if (!enemyMonster.GetIsDead())
            {
                isAllMonstersDead = false;
                break;
            }
        }
        return isAllMonstersDead;
    }

    
    /// <summary>
    /// 生存している敵の中から、ランダムにインデックスを取得
    /// </summary>
    /// <returns>敵インデックス / 取得失敗時 NoSelection</returns>
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

        if (enemyAliveList.Count == 0) return NoSelection;

        return enemyAliveList[Random.Range(0, enemyAliveList.Count)];
    }

    /// <summary>
    /// クリックした敵を選択する処理。
    /// </summary>
    /// <param name="targetIndex"> クリックされた敵のインデックス </param>
    public void ClickedEnemy(int targetIndex)
    {
        // ターゲット選択モードの時のみ有効に
        if (!IsClickedTargetMode) return;
        UpdateClickedEnemy(targetIndex);
    }

    /// <summary>
    /// クリック選択状態の更新。
    /// </summary>
    private void UpdateClickedEnemy(int index)
    {
        if (index < 0 || index >= spawnedMonsters.Count)
            return;

        // 同じ敵を再度選択すると、選択解除
        if (clickedEnemyIndex == index)
        {
            var enemy = spawnedMonsters[index] as Enemy;
            enemy?.SetTargetMarkVisible(false);

            Log($"{spawnedMonsters[clickedEnemyIndex].GetMonsterName()} の選択を解除", BattleLogType.System);
            clickedEnemyIndex = NoSelection;
            return;
        }

        // 前の敵の選択を解除
        if (clickedEnemyIndex != NoSelection)
        {
            var prevEnemy = spawnedMonsters[clickedEnemyIndex] as Enemy;
            prevEnemy?.SetTargetMarkVisible(false);
        }

        // 新しく選択した敵に対するターゲット処理
        clickedEnemyIndex = index;

        var newEnemy = spawnedMonsters[clickedEnemyIndex] as Enemy;
        newEnemy?.SetTargetMarkVisible(true);

        Log($"{spawnedMonsters[clickedEnemyIndex].GetMonsterName()}を選択", BattleLogType.System);
    }


    /// <summary>
    /// 選択されたターゲット取得。
    /// </summary>
    /// <param name="position">　
    /// 判定用の座標。
    /// IsDragAutoModeなら、
    /// この座標に近い位置の敵のインデックスが返される
    /// </param>
    /// <returns>敵インデックス</returns>
    public int GetSelectedEnemyIndex(Vector2 position)
    {
        int selectedEnemyIndex = NoSelection;

        if (IsClickedTargetMode)
            selectedEnemyIndex = this.clickedEnemyIndex;

        if (IsDragAutoMode)
            selectedEnemyIndex = GetNearestEnemyIndex(position);

        return selectedEnemyIndex;
    }


    /// <summary>
    /// 最も近い敵のインデックス取得。
    /// </summary>
    /// <param name="position">
    /// 基準座標。
    /// ここに最も近い敵のインデックスが返される。
    /// </param>
    /// <returns>敵インデックス</returns>
    private int GetNearestEnemyIndex(Vector2 position)
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

    /// <summary>
    /// ドラッグ中のスクリーン座標に最も近い敵をハイライトする。
    /// DragAutoTarget モード時のみ有効。
    /// </summary>
    /// <param name="screenPosition">
    /// 現在のドラッグ位置（スクリーン座標）。
    /// 最寄り敵を判定するために使用。
    /// </param>
    public void HighlightNearestEnemy(Vector2 screenPosition)
    {
        // 自動ターゲットモードでない場合は処理しない
        if (!IsDragAutoMode) return;

        if (spawnedMonsters.Count == 0) return;

        // 指定座標に最も近い敵のインデックスを取得
        int nearestIndex = GetNearestEnemyIndex(screenPosition);

        // 既存のハイライトをすべて解除
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

    /// <summary>
    /// 生存している敵全体をハイライトする。
    /// 範囲攻撃カード使用時などを想定。
    /// DragAutoTarget モード時のみ有効にする。
    /// </summary>
    public void HighlightAllEnemies()
    {
        if (!IsDragAutoMode) return;
        foreach (var monster in spawnedMonsters)
        {
            if (!monster.GetIsDead())
                monster.StartHighlight();
        }
    }

    /// <summary>
    /// 全ての敵ハイライトを解除する。
    /// ドラッグ終了時に呼び出すことを想定。
    /// </summary>
    public void ClearAllHighlights()
    {
        foreach (var monster in spawnedMonsters)
        {
            monster.StopHighlight();
        }
    }

    /// <summary>
    /// 敵死亡時のログ処理。
    /// </summary>
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


    /// <summary>
    /// 各敵の攻撃量の準備。
    /// </summary>
    /// <returns>なし</returns>
    public void PrepareEnemyAttackDamages()
    {
        enemyAttackDamagesList = new List<int>();
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
            enemyAttackDamagesList.Add(attackPowerAmount);
        }
    }

    /// <summary>
    /// 敵の撃量リスト取得用のゲッター
    /// </summary>
    /// <returns>敵の攻撃量一覧</returns>
    public IReadOnlyList<int> GetEnemyAttackDamages() => enemyAttackDamagesList;

    /// <summary>
    /// 生存している敵の名前リストを取得するゲッター
    /// </summary>
    /// <returns>敵の名前リスト</returns>
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


    /// <summary>
    /// 継続ダメージ効果のデータ構造。
    /// </summary>
    private class DamageOverTimeEffect
    {
        public int targetIndex;      // 対象の敵のインデックス
        public int damagePerTurn;    // ターンあたりのダメージ量
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
