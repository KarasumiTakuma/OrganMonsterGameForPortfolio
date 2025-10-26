using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵1体の情報と挙動を管理するスクリプト
/// HP管理、攻撃力、画像などを保持
/// </summary>
public class Enemy : MonoBehaviour
{
    // 敵の名前
    public string enemyName;

    // 最大HP
    public int maxHP;

    // 現在のHP
    public int currentHP;

    // 敵の攻撃力（将来的に攻撃処理に利用）
    public int attackPower;

    // HPゲージを操作するコンポーネント
    [SerializeField] private HpGaugeController hpGauge;

    // 敵キャラクター画像
    [SerializeField] private Image enemyImage;

    /// <summary>
    /// 敵がダメージを受けた際に呼ばれるメソッド
    /// </summary>
    /// <param name="amount">受けるダメージ量</param>
    public void TakeDamage(int amount)
    {
        // 現在HPを減算、0未満にはならないようにする
        currentHP = Mathf.Max(currentHP - amount, 0);

        // HPゲージが設定されていれば、ダメージを反映
        if (hpGauge != null) hpGauge.BeInjured(amount);

        // ここで敵画像の点滅やエフェクトを追加可能
        // 例：enemyImage.color = Color.red;
    }

    /// <summary>
    /// 敵が生存しているかどうかを判定
    /// </summary>
    /// <returns>HPが0より大きければtrue、それ以外はfalse</returns>
    public bool IsAlive() => currentHP > 0;
}
