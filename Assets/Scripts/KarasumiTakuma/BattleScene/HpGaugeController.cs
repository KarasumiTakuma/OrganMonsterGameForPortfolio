using System.Collections;
using UnityEngine;
using TMPro;


/// <summary>
/// HPゲージUIを制御するクラス。
/// 表ゲージと裏ゲージによるダメージ／回復演出、およびHP数値表示を管理する。
/// </summary>
public class HPGaugeController : MonoBehaviour
{
    /// <summary>即時に変化する表側のHPゲージUI</summary>
    [SerializeField] private GameObject frontGauge;

    /// <summary>遅れて追従する裏側のHPゲージUI</summary>
    [SerializeField] private GameObject backGauge;

    /// <summary>「現在HP / 最大HP」を表示するテキスト</summary>
    [SerializeField] private TMP_Text HPText;

    /// <summary>最大HP</summary>
    private int maxHP;

    /// <summary>現在のHP</summary>
    private int currentHP;

    /// <summary>HP1あたりのゲージ幅</summary>
    private float widthPerHP;

    /// <summary>表ゲージ変化後、裏ゲージが追従するまでの待機時間</summary>
    private float frontToBackDelay = 0.5f;

    /// <summary>ゲージアニメーションにかける時間</summary>
    private const float GaugeAnimationDuration = 0.3f;

    /// <summary>表ゲージのRectTransform（キャッシュ）</summary>
    private RectTransform frontGaugeRect;

    /// <summary>裏ゲージのRectTransform（キャッシュ）</summary>
    private RectTransform backGaugeRect;


    /// <summary>
    /// 初期化処理。
    /// ゲージUIのRectTransformを取得しキャッシュする。
    /// </summary>
    void Awake()
    {
        // RectTransformを取得して変数に保存
        frontGaugeRect = frontGauge.GetComponent<RectTransform>();
        backGaugeRect = backGauge.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 最大HPを設定し、HPゲージと表示を初期化する。
    /// </summary>
    /// <param name="hp">設定する最大HP</param>
    public void InitializeHP(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;

        // HP1あたりのゲージ幅を算出
        widthPerHP = frontGaugeRect.sizeDelta.x / maxHP;

        // 表・裏ゲージを満タン状態にする
        Vector2 fullSize = frontGaugeRect.sizeDelta;
        fullSize.x = widthPerHP * currentHP;
        frontGaugeRect.sizeDelta = fullSize;
        backGaugeRect.sizeDelta = fullSize;

        UpdateHPText();
    }

    /// <summary>
    /// HP表示テキストを更新する。
    /// </summary>
    private void UpdateHPText()
    {
        if (HPText != null)
            HPText.text = $"{currentHP} / {maxHP}";
    }

    /// <summary>
    /// ダメージを受けた際の処理。
    /// HPを減算し、表→裏ゲージの減少アニメーションを再生する。
    /// </summary>
    /// <param name="attack">受けるダメージ量</param>
    public void TakeDamage(int attack)
    {
        currentHP = Mathf.Max(currentHP - attack, 0);

        // ダメージ後のHPに対応するゲージ幅
        float remainingHPGaugeWidth = widthPerHP * currentHP;

        StartCoroutine(DamageAnimation(remainingHPGaugeWidth));

        UpdateHPText();
    }

    /// <summary>
    /// ダメージ時のHPゲージ減少アニメーション。
    /// 表ゲージを先に減らし、一定時間後に裏ゲージを追従させる。
    /// </summary>
    /// <param name="remainingHPGaugeWidth">ダメージ後に残るHPゲージ幅</param>

    IEnumerator DamageAnimation(float remainingHPGaugeWidth)
    {

        Vector2 currentSize = frontGaugeRect.sizeDelta;

        // 目標のゲージ(ダメージ後のゲージ)のサイズを設定(初期値は現在の表ゲージサイズ。ダメージ後に残っているゲージの幅とはまだ不一致)
        Vector2 targetSize = currentSize;
        targetSize.x = remainingHPGaugeWidth;  // ダメージ後の残ったHPゲージの幅を目標ゲージの幅とする

        // ゲージを0.3秒かけてなめらかに減らす
        float elapsed = 0f;      // 経過時間
        while (elapsed < GaugeAnimationDuration)
        {
            // 現在のゲージ幅から目標のゲージ幅に向かって徐々に減らす処理
            // 現在ゲージサイズから目標ゲージサイズまで、(elapsed / duration)の割合でゲージを減らしていく
            currentSize.x = Mathf.Lerp(currentSize.x, targetSize.x, elapsed / GaugeAnimationDuration);
            frontGaugeRect.sizeDelta = currentSize;  // 表ゲージのサイズを更新

            // 前フレームからの経過時間を加算
            // (60FPSなら0.0166s)(FPSが異なっていても、 時間当たりに減るゲージの幅は等しくするため)。
            // (60FPSなら、1フレーム ≈ 0.0166秒、0.0166 × 約18フレームで0.3秒、18フレームでゲージが目的ゲージまで減る
            // (30FPSなら、1フレーム ≈ 0.033秒、0.033 × 約9フレームで0.3秒、9フレームでゲージが目的ゲージまで減る)
            elapsed += Time.deltaTime;
            yield return null;                  // 1フレーム待つ
        }


        // 表ゲージを最終サイズに固定
        frontGaugeRect.sizeDelta = targetSize;

        // 裏ゲージを遅れて追従させる
        yield return new WaitForSeconds(frontToBackDelay);
        backGaugeRect.sizeDelta = targetSize;
    }

    /// <summary>
    /// HPを回復する処理。
    /// 表ゲージは即時反映し、裏ゲージは遅れて追従させる。
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void Heal(int healAmount)
    {
        int oldHP = currentHP;
        currentHP = Mathf.Min(currentHP + healAmount, maxHP);

        float oldWidth = widthPerHP * oldHP;
        float newWidth = widthPerHP * currentHP;

        Vector2 gaugeSize = frontGaugeRect.sizeDelta;
        gaugeSize.x = newWidth;
        frontGaugeRect.sizeDelta = gaugeSize;

        StartCoroutine(HealAnimation(oldWidth, newWidth));

        UpdateHPText();
    }

    /// <summary>
    /// 回復時の裏ゲージ追従アニメーション。
    /// </summary>
    /// <param name="fromWidth">回復前のゲージ幅</param>
    /// <param name="toWidth">回復後のゲージ幅</param>
    IEnumerator HealAnimation(float fromWidth, float toWidth)
    {
        yield return new WaitForSeconds(frontToBackDelay);

        Vector2 currentSize = backGaugeRect.sizeDelta;
        Vector2 targetSize = currentSize;
        targetSize.x = toWidth;

        float elapsed = 0f;

        while (elapsed < GaugeAnimationDuration)
        {
            currentSize.x = Mathf.Lerp(currentSize.x, targetSize.x, elapsed / GaugeAnimationDuration);
            backGaugeRect.sizeDelta = currentSize;
            elapsed += Time.deltaTime;
            yield return null;
        }

        backGaugeRect.sizeDelta = targetSize;
    }

    /// <summary>
    /// 現在のHPを取得する。
    /// </summary>
    /// <returns>現在のHP</returns>
    public int GetCurrentHP() => currentHP;
}