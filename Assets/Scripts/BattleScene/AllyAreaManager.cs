using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 味方モンスター3体をスポーン・管理するクラス
/// EnemyAreaManager と同様の構造
/// さらに3体のHPを合算して共有HPとして管理
/// </summary>
public class AllyAreaManager : MonsterAreaManager
{

    [Header("UI")]
    [SerializeField] private HpGaugeController sharedHpGauge; // 共有HPゲージ
    [SerializeField] private AudioClip HealSoundEffect;  // 回復の効果音
    private int sharedMaxHP;      // 3体の最大HP合算
    private int sharedCurrentHP;  // 現在の共有HP（ダメージや回復後とかに使う値）
    private List<AllyMonsterData> allyDataList = new List<AllyMonsterData>();  // 3体の味方データ
    private List<HealOverTimeEffect> healOverTimeEffects = new List<HealOverTimeEffect>();  // 進行中の継続回復効果をまとめたリスト

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
                // 通常の MonsterData を AllyMonsterData に変換
                allyDat = MonsterDataConverter.ToAllyMonster(data);
            }

            allyDataList.Add(allyDat);
        }
    }

    /// <summary>
    /// 味方モンスターをスポーン
    /// </summary>
    public void SpawnAllies()
    {
        List<MonsterData> baseDataList = allyDataList.ConvertAll(data => (MonsterData)data);
        base.SpawnMonsters(baseDataList);
        InitializeSharedHP(); // スポーン直後に共有HPを初期化
    }

    /// 共有HPを初期化
    private void InitializeSharedHP()
    {
        sharedMaxHP = 0;
        foreach (var ally in spawnedMonsters)
        {
            sharedMaxHP += ally.GetHP(); // 各味方の最大HPを合算
        }
        sharedCurrentHP = sharedMaxHP;

        if (sharedHpGauge != null)
        {
            sharedHpGauge.SetMaxHP(sharedMaxHP);
        }
    }

    /// 共有HPにダメージ
    protected override void ApplyDamageToAll(int damage) // 親クラスのApplyDamageToAllをオーバーライド(引数はdamegeの1つのみ)
    {
        sharedCurrentHP = Mathf.Max(sharedCurrentHP - damage, 0);

        foreach (var monster in spawnedMonsters)// 各味方モンスターオブジェクトに対して
        {
            // Monster型オブジェクトmonsterをAlly型のallyとしてキャストできる場合のみ
            if (monster != null && monster is Ally ally)
            {
                // ダメージエフェクトを再生
                ally.AllyPlayDamageEffect();
            }
        }

        if (sharedHpGauge != null)
            sharedHpGauge.BeInjured(damage);
    }


    // 味方HPにダメージを与える処理を、外部から呼び出すためのラッパーメソッド
    public void TakeDamageToSharedHP(int damage)
    {
        this.ApplyDamageToAll(damage);
        Log($"プレイヤーに{damage}ダメージ!", BattleLogType.Attack); // // ダメージが与えられた旨をログに追加
    }


    /// 共有HPを回復するメソッド
    public void HealSharedHP(int amount)
    {
        int previousHP = sharedCurrentHP;
        sharedCurrentHP = Mathf.Min(sharedCurrentHP + amount, sharedMaxHP);

        int healedAmount = sharedCurrentHP - previousHP;

        if (sharedHpGauge != null)
        {
            // 回復分をゲージに反映
            if (healedAmount > 0)
                sharedHpGauge.BeHealed(healedAmount);// ゲージを更新(ゲージの回復処理)
        }

        // プレイヤーHPが回復した旨を戦闘ログにメッセージとして追加
        Log($"プレイヤーのHPが{healedAmount}回復!", BattleLogType.Heal);

        // シーン内に存在するScreenHealEffectコンポーネントを持つオブジェクトを探して、そのコンポーネントを取得し、
        ScreenHealEffect healEffect = Object.FindAnyObjectByType<ScreenHealEffect>();
        healEffect?.PlayHealEffect();  // コンポーネントが正しく取得できた場合は回復エフェクトのアニメーションを実行
    }

    public void ApplyHealOverTime(int healAmount, int durationTurns)
    {
        // 継続ターン数durationTurnsを、[durationTurns, durationTurns+2]の範囲でランダム化
        int randomizedTurns = UnityEngine.Random.Range(durationTurns, durationTurns + 3);
        healOverTimeEffects.Add(new HealOverTimeEffect(healAmount, randomizedTurns));
        Log($"プレイヤーに継続回復付与!：{randomizedTurns}ターン", BattleLogType.Heal);
    }

    // 残っている継続回復効果を適用するメソッド(回復量はターンごとに変動)
    public void ProcessHealOverTime()
    {
        if (healOverTimeEffects.Count == 0) return;  // 適用する継続回復効果が残っていなければ何もしない

        List<HealOverTimeEffect> expiredEffects = new List<HealOverTimeEffect>(); // 期限切れの効果を保持(効果削除用)

        // 現在適用中の継続ダメージの効果を発動する
        foreach (var hotEffect in healOverTimeEffects)
        {
            // 継続回復量を[hotEffect.healPerTurn * 0.8,hotEffect.healPerTurn * 1.5]の範囲でランダム化
            int randomizedHealAmount = Mathf.RoundToInt(hotEffect.healPerTurn * Random.Range(0.8f, 1.5f));
            
            // 共有HPを回復する
            HealSharedHP(randomizedHealAmount);
            AudioManager.Instance.PlaySE(HealSoundEffect);

            hotEffect.remainingTurns--;   // 継続回復を適用する残りターン数を減らす

            // 継続回復の残りターン数が0になると、効果が切れる
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

    // 味方(プレイヤー)が死んでいるかどうかの判定
    public bool GetIsDead()
    {
        return sharedCurrentHP <= 0 ? true : false;  // 共有HPが0以下なら、死亡している
    }

    public int GetSharedCurrentHP() => sharedCurrentHP;
    public int GetSharedMaxHP() => sharedMaxHP;

    public List<Monster> GetAllies() => spawnedMonsters;
    public int GetAllyCount() => spawnedMonsters.Count;

    // 既存の味方を削除
    public void ClearAllies()
    {
        foreach (var ally in spawnedMonsters)
        {
            if (ally != null) Destroy(ally.gameObject);
        }
        spawnedMonsters.Clear();
    }

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
