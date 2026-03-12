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
    [SerializeField, Header("HPゲージのコントローラ")] private HPGaugeController hpGauge;

    /// <summary>
    /// 現在ターゲットされていることを示すUIマーク。カード選択時などに表示／非表示を切り替える。
    /// </summary>
    [SerializeField, Header("ターゲットマーク")] private Image targetMark;

    /// <summary>
    /// この敵が死亡した際に、死亡ログを出力すべきかどうかを示すフラグ。
    /// 重複ログ防止のため、最初の死亡判定後に false へ切り替えられる。
    /// </summary>
    private bool isShouldLogDeath = true;

    // ★自分の行動ロジック（AI）を保持する変数
    private EnemyLogicBase myLogic;

    // ★BattleManagerがAIを取得するためのプロパティ
    public EnemyLogicBase Logic => myLogic;

    // ★ダメージ軽減率 (0.0f = 無敵, 1.0f = 通常, 0.5f = 半減)
    private float damageMultiplier = 1.0f;

    // ★追加: バリアの残りターン数
    private int guardRemainingTurns = 0;


    /// <summary>
    /// 敵モンスターの初期化処理。
    /// EnemyMonsterData に定義されたステータスをもとに、
    /// Monster 基底クラスおよびHPゲージの初期状態を設定する。
    /// 行動ロジックも受け取る
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

        // ★追加：データから行動ロジック(AI)を受け取って保持する
        this.myLogic = enemyMonsterData.BehaviorLogic;

        // HPゲージ側にも最大HPを設定
        if (hpGauge != null)
            hpGauge.InitializeHP(enemyMonsterData.GetMaxHP());

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
            hpGauge.TakeDamage(amount);
        }
    }

    /// <summary>
    /// 外部クラスから呼び出すためのダメージ受付メソッド。
    /// 内部的には ApplyDamage を呼び出す。
    /// </summary>
    /// <param name="amount">与えるダメージ量</param>
    public void TakeDamage(int amount)
    {
        // 1. 計算
        int finalDamage = CalculateMitigatedDamage(amount);

        // 2. 攻撃用ログ
        Log($"{monsterName} に{finalDamage}ダメージ！", BattleLogType.Attack);

        // 3. HPを減らす
        ApplyDamage(finalDamage);
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

    /// <summary>
    /// HP回復処理
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;

        // 最大HPを超えないように回復
        int prevHP = currentHP;
        currentHP = Mathf.Min(currentHP + amount, maxHP);

        // 実際に回復した量
        int healAmount = currentHP - prevHP;

        // UI更新（HPGaugeControllerにHealメソッドがない場合は、仕様に合わせて調整）
        if (hpGauge != null)
        {
            // ここでは簡易表現
            hpGauge.InitializeHP(maxHP);
            hpGauge.TakeDamage(maxHP - currentHP);
        }

        Log($"{monsterName} のHPが{healAmount}回復！", BattleLogType.Heal);
        // 必要ならここで回復エフェクト再生
    }

    /// <summary>
    /// ダメージ軽減バリアを張る
    /// </summary>
    /// <param name="multiplier">0.5fならダメージ半減</param>
    /// <param name="duration">持続ターン数</param>
    public void SetGuard(float multiplier, int duration)
    {
        this.damageMultiplier = multiplier;
        this.guardRemainingTurns = duration; // ターン数を上書き設定

        // バリアエフェクト表示などをここに入れる
        Log($"{monsterName} は防御態勢をとった！(残り{duration}ターン)", BattleLogType.Attention);
    }

    /// <summary>
    /// ★ターン開始時の更新処理
    /// BattleManagerから、敵の行動前に呼び出してもらう
    /// </summary>
    public void OnTurnStart()
    {
        if (guardRemainingTurns > 0)
        {
            guardRemainingTurns--;

            // ターン経過でバリアが切れた場合
            if (guardRemainingTurns <= 0)
            {
                ResetGuard();
                Log($"{monsterName} の防御態勢が解除された。", BattleLogType.Attention);
            }
            else
            {
                Log($"{monsterName} の防御態勢継続 (残り{guardRemainingTurns}ターン)", BattleLogType.Attention);
            }
        }
    }

    /// <summary>
    /// ガードを解除する
    /// </summary>
    public void ResetGuard()
    {
        this.damageMultiplier = 1.0f;
        this.guardRemainingTurns = 0;
    }

    /// <summary>
    /// ガード計算（共通ロジックとして切り出し）
    /// </summary>
    private int CalculateMitigatedDamage(int amount)
    {
        int finalDamage = Mathf.RoundToInt(amount * damageMultiplier);

        // ガード演出用ログ（必要なら）
        if (damageMultiplier < 1.0f && amount > 0)
        {
            Debug.Log("ガード適用");
        }
        return finalDamage;
    }

    /// <summary>
    /// ★継続ダメージ専用メソッド
    /// 軽減はするが、ログの種類を変える
    /// </summary>
    public void TakeDoTDamage(int amount)
    {
        // 1. 計算（通常と同じ軽減ロジックを使う！）
        int finalDamage = CalculateMitigatedDamage(amount);

        // 2. 継続ダメージ用ログ（紫色などで表示される）
        Log($"{monsterName} に継続ダメージ {finalDamage}！", BattleLogType.DamageOverTime);

        // 3. HPを減らす）
        ApplyDamage(finalDamage);
    }

    // ログ出力用のヘルパーメソッド
    protected void Log(string message, BattleLogType type)
    {
        if (BattleLogManager.Instance != null)
            BattleLogManager.Instance.AddLog(message, type);
    }

}
