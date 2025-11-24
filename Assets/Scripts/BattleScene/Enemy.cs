using UnityEngine;
using UnityEngine.UI;

public class Enemy : Monster
{
    [SerializeField] private HpGaugeController hpGauge;

    // Enemyデータの初期化(敵スポーン時の初期化)
    public void InitializeSet(EnemyMonsterData enemyMonsterData)
    {
        // 親クラス(Monster)のモンスターデータ初期化を使用
        InitializeBase(
            enemyMonsterData.GetID(),
            enemyMonsterData.GetName(),
            enemyMonsterData.GetHP(),
            enemyMonsterData.GetAttackPower(),
            enemyMonsterData.GetIcon()
        );
        // この敵のHpGaugeController側にも最大HPを設定する
        if (hpGauge != null)
        hpGauge.SetMaxHP(enemyMonsterData.GetHP());
    }

    // ダメージ処理（HPゲージ更新込み）
    protected override void TakeDamage(int amount)
    {
        base.TakeDamage(amount); // HP減算は親クラスのTakeDamageメソッドで処理

        if (hpGauge != null)
            hpGauge.BeInjured(amount);
    }

    public void TakeDamagePublic(int amount)
    {
        this.TakeDamage(amount);
    }

    // 必要に応じて IsAlive() も親クラスのまま protected で使える
}
