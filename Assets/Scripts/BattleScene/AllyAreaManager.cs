using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 味方モンスターをスポーン・管理するクラス。
/// 複数の味方HPを合算し、共有HPとして扱う。
/// </summary>
public class AllyAreaManager : MonsterAreaManager
{

    [Header("UI")]
    /// <summary> 共有HPを表示するHPゲージ </summary>
    [SerializeField] private HPGaugeController sharedHpGauge;

    /// <summary> 回復時に再生する効果音 </summary>
    [SerializeField] private AudioClip healSoundEffect;

    /// <summary> 味方全体の最大HP合計 </summary>
    private int sharedMaxHP;

    /// <summary> 現在の共有HP </summary>
    private int sharedCurrentHP;

    /// <summary> スポーン用の味方データリスト </summary>
    private List<AllyMonsterData> allyDataList = new List<AllyMonsterData>();

    /// <summary> 継続回復効果の管理リスト </summary>
    private List<HealOverTimeEffect> healOverTimeEffects = new List<HealOverTimeEffect>();

    /// <summary>
    /// 味方データを設定する
    /// </summary>
    /// <param name="allies">設定する味方モンスターデータ</param>
    public void SetAllyData(List<MonsterData> allies)
    {
        allyDataList.Clear();

        foreach (var data in allies)
        {
            AllyMonsterData allyDat;

            if (data is AllyMonsterData existingAlly)
            {
                allyDat = existingAlly;
            }
            else
            {
                // MonsterData を味方用データへ変換
                allyDat = MonsterDataConverter.CreateAllyMonsterFrom(data);
            }

            allyDataList.Add(allyDat);
        }
    }

    /// <summary>
    /// 味方モンスターをスポーンする
    /// </summary>
    public void SpawnAllies()
    {
        // 基底クラス(MonsterData)用のデータ形式へ変換
        List<MonsterData> baseDataList = allyDataList.ConvertAll(data => (MonsterData)data);
        base.SpawnMonsters(baseDataList);

        // スポーン直後に共有HPを初期化
        InitializeSharedHP();
    }

    /// <summary>
    /// 共有HPを初期化する
    /// </summary>
    private void InitializeSharedHP()
    {
        sharedMaxHP = 0;

        // 各味方の最大HPを合算
        foreach (var ally in spawnedMonsters)
        {
            sharedMaxHP += ally.GetMaxHP();
        }

        sharedCurrentHP = sharedMaxHP;

        // HPゲージの初期化
        if (sharedHpGauge != null)
        {
            sharedHpGauge.InitializeHP(sharedMaxHP);
        }
    }

    /// <summary>
    /// 共有HPへダメージを適用する
    /// </summary>
    /// <param name="damage">与えるダメージ量</param>
    protected override void ApplyDamageToAllMonsters(int damage)
    {
        // HPを減算（0未満にならないよう制限）
        sharedCurrentHP = Mathf.Max(sharedCurrentHP - damage, 0);

        // 味方モンスターへダメージエフェクトを適用
        foreach (var monster in spawnedMonsters)
        {
            if (monster != null && monster is Ally ally)
            {
                ally.PlayDamageEffect();
            }
        }

        // HPゲージ更新
        if (sharedHpGauge != null)
            sharedHpGauge.TakeDamage(damage);
    }


    /// <summary>
    /// 外部から共有HPへダメージを与えるラッパーメソッド
    /// </summary>
    /// <param name="damage">与えるダメージ量</param>
    public void TakeDamageToSharedHP(int damage)
    {
        this.ApplyDamageToAllMonsters(damage);
        Log($"プレイヤーに{damage}ダメージ!", BattleLogType.Attack);
    }


    /// <summary>
    /// 共有HPを回復する
    /// </summary>
    /// <param name="amount">回復量</param>
    public void HealSharedHP(int amount)
    {
        int previousHP = sharedCurrentHP;

        // 最大HPを超えないよう制限
        sharedCurrentHP = Mathf.Min(sharedCurrentHP + amount, sharedMaxHP);

        int healedAmount = sharedCurrentHP - previousHP;

        if (sharedHpGauge != null && healedAmount > 0)
        {
            sharedHpGauge.Heal(healedAmount);
        }

        Log($"プレイヤーのHPが{healedAmount}回復!", BattleLogType.Heal);

        // 画面回復エフェクトの実行
        ScreenHealEffect healEffect = Object.FindAnyObjectByType<ScreenHealEffect>();
        healEffect?.PlayHealEffect();
    }

    /// <summary>
    /// 継続回復効果を付与する
    /// </summary>
    /// <param name="healAmount">1ターン当たりの回復量</param>
    /// <param name="durationTurns">継続ターン数</param>
    public void ApplyHealOverTime(int healAmount, int durationTurns)
    {
        // 継続ターン数durationTurnsを、[durationTurns, durationTurns+2]の範囲でランダム化
        int randomizedTurns = UnityEngine.Random.Range(durationTurns, durationTurns + 3);
        healOverTimeEffects.Add(new HealOverTimeEffect(healAmount, randomizedTurns));
        Log($"プレイヤーに継続回復付与!：{randomizedTurns}ターン", BattleLogType.Heal);
    }

    /// <summary>
    /// 残っている継続回復効果を処理する(回復量はターンごとに変動)
    /// </summary>
    public void ProcessHealOverTime()
    {
        if (healOverTimeEffects.Count == 0) return;

        // 期限切れの効果を保持(効果削除用)
        List<HealOverTimeEffect> expiredEffects = new List<HealOverTimeEffect>();

        // 現在適用中の継続回復効果を処理
        foreach (var hotEffect in healOverTimeEffects)
        {
            // 継続回復量を[hotEffect.healPerTurn * 0.8,hotEffect.healPerTurn * 1.5]の範囲でランダム化
            int randomizedHealAmount = Mathf.RoundToInt(hotEffect.healPerTurn * Random.Range(0.8f, 1.5f));

            HealSharedHP(randomizedHealAmount);
            AudioManager.Instance.PlaySE(healSoundEffect);

            hotEffect.remainingTurns--;

            // 効果が切れたかの判定
            if (hotEffect.remainingTurns <= 0)
            {
                expiredEffects.Add(hotEffect);
                Log($"プレイヤーへの継続回復の効果が切れた！", BattleLogType.Attention);
            }
        }

        // 期限が切れた効果は削除する
        foreach (var expEffect in expiredEffects)
        {
            healOverTimeEffects.Remove(expEffect);
        }
    }

    /// <summary>
    /// 全味方をハイライト表示する
    /// </summary>
    public void HighlightAllAllies()
    {
        foreach (var ally in spawnedMonsters)
        {
            ally.StartHighlight();
        }
    }

    /// <summary>
    /// 全ハイライトを解除する
    /// </summary>
    public void ClearAllHighlights()
    {
        foreach (var ally in spawnedMonsters)
        {
            ally.StopHighlight();
        }
    }

    /// <summary>
    /// プレイヤーが死亡しているか判定
    /// </summary>
    /// <returns>死亡していたら、true</returns>
    public bool GetIsPlayerDead()
    {
        return sharedCurrentHP <= 0;  // 共有HPが0以下なら、死亡している
    }

    /// <summary>現在の共有HP取得</summary>
    public int GetSharedCurrentHP() => sharedCurrentHP;

    /// <summary>最大共有HP取得</summary>
    public int GetSharedMaxHP() => sharedMaxHP;

    /// <summary>味方リスト取得（読み取り専用）</summary>
    public IReadOnlyList<Monster> GetAllies() => spawnedMonsters;

    /// <summary>味方数取得</summary>
    public int GetAllyCount() => spawnedMonsters.Count;

    /// <summary>
    /// 継続回復効果データ構造
    /// </summary>
    private class HealOverTimeEffect
    {
        public int healPerTurn;     // 1ターン当たりの回復量
        public int remainingTurns;  // 残り継続ターン数

        public HealOverTimeEffect(int healPerTurn, int remainingTurns)
        {
            this.healPerTurn = healPerTurn;
            this.remainingTurns = remainingTurns;
        }
    }
}
