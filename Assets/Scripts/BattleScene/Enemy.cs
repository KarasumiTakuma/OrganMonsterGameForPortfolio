using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : Monster
{
    [SerializeField, Header("HPゲージのコントローラ")] private HpGaugeController hpGauge;

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
        {
            hpGauge.BeInjured(amount);
        }
    }

    public void TakeDamagePublic(int amount)
    {
        this.TakeDamage(amount);
    }

    protected override void OnDeath()
    {

        if (hpGauge != null)
        {
            // この敵のHPバーを非表示にする前に、Hpバーのアニメーションのコルーチンが終わるまで待つ
            StartCoroutine(DeactivateAfterDelay());
        }
        else
        {
            // HPバーがない場合は、即座に敵のオブジェクトを非表示
            this.gameObject.SetActive(false);
        }

    }    


    // HPバーが減少するアニメーションが終了してからHPバーと敵のオブジェクトを非表示にするCoroutine
    private IEnumerator DeactivateAfterDelay()
    {
        // HPバー側のCoroutineのアニメーションが完了するまで少し待つ
        yield return new WaitForSeconds(0.5f);
        // HPバーと敵のオブジェクトを非表示
        hpGauge.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }


}
