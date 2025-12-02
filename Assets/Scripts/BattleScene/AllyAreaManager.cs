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

    private int sharedMaxHP;      // 3体の最大HP合算
    private int sharedCurrentHP;  // 現在の共有HP（ダメージや回復後とかに使う値）

    private List<AllyMonsterData> allyDataList = new List<AllyMonsterData>();  // 3体の味方データ

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

        SpawnAllies();
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
    public void TakeDamageToSharedHP(int damage)
    {
        this.ApplyDamageToAll(damage);
    }



    /// <summary>
    /// 共有HPを回復
    /// </summary>
    public void HealSharedHP(int amount)
    {
        int previousHP = sharedCurrentHP;
        sharedCurrentHP = Mathf.Min(sharedCurrentHP + amount, sharedMaxHP);

        if (sharedHpGauge != null)
        {
            // 回復分をゲージに反映
            int healedAmount = sharedCurrentHP - previousHP;
            if (healedAmount > 0)
                sharedHpGauge.BeHealed(healedAmount);// ゲージを更新(ゲージの回復処理)
        }

        // シーン内に存在するScreenHealEffectコンポーネントを持つオブジェクトを探して、そのコンポーネントを取得し、
        ScreenHealEffect healEffect = Object.FindAnyObjectByType<ScreenHealEffect>();
        healEffect?.PlayHealEffect();  // コンポーネントが正しく取得できた場合は回復エフェクトのアニメーションを実行
    }

    public bool GetIsAliveMonster()
    {
        return sharedCurrentHP > 0 ? true : false; // 共有HPなので、falseなら全滅扱い
    }

    public int GetSharedCurrentHP() => sharedCurrentHP;
    public int GetSharedMaxHP() => sharedMaxHP;

    public List<Monster> GetAllies() => spawnedMonsters;
    public int GetAllyCount() => spawnedMonsters.Count;

    /// <summary>
    /// 既存の味方を削除
    /// </summary>
    public void ClearAllies()
    {
        foreach (var ally in spawnedMonsters)
        {
            if (ally != null) Destroy(ally.gameObject);
        }
        spawnedMonsters.Clear();
    }
}
