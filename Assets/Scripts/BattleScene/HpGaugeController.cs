using System.Collections;
using UnityEngine;

public class HpGaugeController : MonoBehaviour
{
    // 体力ゲージ(表のゲージ)
    [SerializeField] private GameObject gauge;
    // 猶予ゲージ(裏のゲージ)
    [SerializeField] private GameObject graceGauge;

    private int maxHP;      // 最大HP
    private int currentHP;  // 現在のHP
    private float perHP;    // HP1あたりの幅

    // 体力ゲージが減った後裏ゲージが減るまでの待機時間
    private float waitingTimeAfterFrontGauge = 0.5f;

    // RectTransformをキャッシュするための変数
    private RectTransform gaugeRect;
    private RectTransform graceGaugeRect;


    void Awake()
    {
        // RectTransformを取得して変数に保存
        gaugeRect = gauge.GetComponent<RectTransform>();
        graceGaugeRect = graceGauge.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 最大HPを設定し、ゲージを初期化する
    /// </summary>
    public void SetMaxHP(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;

        // HP1あたりの幅を計算
        perHP = gaugeRect.sizeDelta.x / maxHP;

        // 初期ゲージを満タンにする
        Vector2 fullSize = gaugeRect.sizeDelta;
        fullSize.x = perHP * currentHP;
        gaugeRect.sizeDelta = fullSize;
        graceGaugeRect.sizeDelta = fullSize;
    }

    /// <summary>
    /// 現在HPを返す
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHP;
    }


    // ダメージを受けた際に呼ばれるメソッド
    public void BeInjured(int attack)
    {
        // 攻撃分のダメージを現在のHPから減算(この時HPがマイナスにならないようにする)
        currentHP = Mathf.Max(currentHP - attack, 0);

        // 体力1あたりの幅とダメージを考慮したcurrentHPの積が攻撃を受けた後に残るHPゲージの幅
        float remainingHPGaugeWidth = perHP * currentHP;

        // コルーチンでゲージを徐々に減らす
        StartCoroutine(DamageAnimation(remainingHPGaugeWidth));  // ダメージ後の挙動を制御

    }

    // 体力ゲージを減らすコルーチン(アニメーション)
    IEnumerator DamageAnimation(float remainingHPGaugeWidth)
    {

        // 現在の表ゲージのサイズ(幅と高さ)をVector2で取得
        Vector2 currentSize = gaugeRect.sizeDelta;
        // 目標のゲージ(ダメージ後のゲージ)のサイズを設定(初期値は現在の表ゲージサイズ)
        Vector2 targetSize = currentSize;
        targetSize.x = remainingHPGaugeWidth;  // ダメージ後の残ったHPゲージ幅を目標ゲージの幅とする

        // ゲージを0.3秒かけてなめらかに減らす
        float elapsed = 0f;      // 経過時間
        float duration = 0.3f;   // 全体時間(アニメーション時間)
        while (elapsed < duration)
        {
            // 現在のゲージ幅から目標のゲージ幅に向かって徐々に減らす処理

            // 現在ゲージサイズから目標ゲージサイズまで、(elapsed / duration)の割合でゲージを減らしていく
            currentSize.x = Mathf.Lerp(currentSize.x, targetSize.x, elapsed / duration);
            gaugeRect.sizeDelta = currentSize;  // 表ゲージのサイズを更新
            elapsed += Time.deltaTime;          // 前フレームからの経過時間を加算(60FPSなら0.0166s)
            yield return null;                  // 1フレーム待つ
        }


        // 現在の表ゲージ幅を最終的な幅(目標ゲージ幅)に設定
        gaugeRect.sizeDelta = targetSize;

        // 指定秒数だけ待機してから裏ゲージを追いつかせる
        yield return new WaitForSeconds(waitingTimeAfterFrontGauge);
        graceGaugeRect.sizeDelta = targetSize; // 裏ゲージ幅をダメージ後のゲージ幅とする
    }
}