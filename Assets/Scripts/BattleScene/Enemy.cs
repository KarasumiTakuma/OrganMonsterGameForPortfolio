using UnityEngine;
using UnityEngine.UI;
using System.Collections;


/// <summary>
/// 敵モンスターを表すクラス。
/// Monster クラスを継承し、
/// HPゲージ管理・ターゲット表示・死亡時処理など、
/// 敵専用の挙動を担当する。
/// </summary>
public class Enemy : Monster
{

    /// <summary>
    /// 敵のHPゲージを制御するコンポーネント。ダメージ時のゲージ更新や非表示処理に使用される。
    /// </summary>
    [SerializeField, Header("HPゲージのコントローラ")] private HpGaugeController hpGauge;

    /// <summary>
    /// 現在ターゲットされていることを示すUIマーク。カード選択時などに表示／非表示を切り替える。
    /// </summary>
    [SerializeField, Header("ターゲットマーク")] private Image targetMark;

    /// <summary>
    /// この敵が死亡した際に、死亡ログを出力すべきかどうかを示すフラグ。
    /// 重複ログ防止のため、最初の死亡判定後に false へ切り替えられる。
    /// </summary>
    private bool isShouldLogDeath = true;

    /// <summary>
    /// 敵モンスターの初期化処理。
    /// EnemyMonsterData に定義されたステータスをもとに、
    /// Monster 基底クラスおよびHPゲージの初期状態を設定する。
    /// </summary>
    /// <param name="enemyMonsterData">初期化に使用する敵モンスターデータ</param>
    public void InitializeFromData(EnemyMonsterData enemyMonsterData)
    {
        // 親の Monster クラスの共通初期化処理
        InitializeBase(
            enemyMonsterData.GetID(),
            enemyMonsterData.GetName(),
            enemyMonsterData.GetMaxHP(),
            enemyMonsterData.GetAttackPower(),
            enemyMonsterData.GetIcon()
        );
        
        // HPゲージ側にも最大HPを設定
        if (hpGauge != null)
            hpGauge.SetMaxHP(enemyMonsterData.GetMaxHP());

        // 初期状態ではターゲットマークは非表示
        if (targetMark != null)
            targetMark.enabled = false;
    }

    /// <summary>
    /// 敵にダメージを適用する内部処理。
    /// HPの減少、ダメージ演出、死亡判定、HPゲージ更新を行う。
    /// </summary>
    /// <param name="amount">与えるダメージ量</param>
    private void ApplyDamage(int amount)
    {
        // HPを減算（0未満にはならない）
        currentHP = Mathf.Max(currentHP - amount, 0);

        // ダメージエフェクト再生
        PlayDamageEffect();

        // 死亡判定
        if (currentHP <= 0)
        {
            isDead = true;
            OnDeath();
        }

        // HPゲージ更新
        if (hpGauge != null)
        {
            hpGauge.BeInjured(amount);
        }
    }

    /// <summary>
    /// 外部クラスから呼び出すためのダメージ受付メソッド。
    /// 内部的には ApplyDamage を呼び出す。
    /// </summary>
    /// <param name="amount">与えるダメージ量</param>
    public void TakeDamage(int amount)
    {
        ApplyDamage(amount);
    }


    /// <summary>
    /// この敵モンスターが死亡した際の処理。
    /// ハイライト解除、ターゲットマーク非表示、
    /// HPゲージおよび敵オブジェクトの非表示処理を行う。
    /// </summary>
    private void OnDeath()
    {
        // ハイライトを確実に停止
        StopHighlight();

        // ターゲットマークを非表示
        if (targetMark != null)
            targetMark.enabled = false;

        // HPゲージが存在する場合は、演出終了後に非表示
        if (hpGauge != null)
        {
            StartCoroutine(DeactivateAfterDelay());
        }
        else
        {
            // HPゲージがない場合は即座に非表示
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// この敵の死亡ログを出力すべきかどうかを判定する。
    /// 死亡直後の1回のみ true を返し、それ以降は false を返す。
    /// </summary>
    /// <returns>
    /// 死亡ログを出力すべき場合は true。
    /// 既にログ出力済み、または未死亡の場合は false。
    /// </returns>
    public bool IsShouldLogDeath()
    {
        if (GetIsDead() && isShouldLogDeath)
        {
            isShouldLogDeath = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// HPゲージの減少アニメーション完了後に、
    /// HPゲージおよび敵オブジェクトを非表示にするコルーチン。
    /// </summary>
    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // HPゲージと敵のオブジェクトを非表示
        hpGauge.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// ターゲットマークの表示状態を設定する。
    /// </summary>
    /// <param name="isVisible">true の場合表示、false の場合非表示</param>
    public void SetTargetMarkVisible(bool isVisible)
    {
        if (targetMark != null)
            targetMark.enabled = isVisible;
    }

}
