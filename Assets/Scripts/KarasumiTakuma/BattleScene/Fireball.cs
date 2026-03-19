using UnityEngine;
using System;

/// <summary>
/// 戦闘中に飛来する火の玉オブジェクトを制御するクラス。
/// 指定位置へ向かって移動し、
/// クリックされるとランダムな効果（ダメージ・回復）を発動して消滅する。
/// </summary>
public class Fireball : MonoBehaviour
{
    /// <summary> 火の玉の移動速度（1秒あたりの移動距離）</summary>
    [SerializeField, Header("火の玉の移動速度(1秒あたり)")] private float moveSpeed;

    [Header("火の玉の揺れ設定")]

    /// <summary>左右方向の揺れ幅</summary>
    [SerializeField] private float amplitudeX = 0.5f;

    /// <summary>左右方向の揺れ速度</summary>
    [SerializeField] private float frequencyX = 3f;

    /// <summary>上下方向の揺れ幅</summary>
    [SerializeField] private float amplitudeY = 0.5f;

    /// <summary>上下方向の揺れ速度</summary>
    [SerializeField] private float frequencyY = 3f;


    [Header("SE設定")]

    /// <summary>攻撃時に再生される効果音</summary>
    [SerializeField] private AudioClip attackSoundEffect;

    /// <summary>回復時に再生される効果音。</summary>
    [SerializeField] private AudioClip healSoundEffect;

    /// <summary>火の玉の到達目標位置</summary>
    private Vector3 targetPosition;

    /// <summary>火の玉が発射された初期位置</summary>
    private Vector3 startPosition;

    /// <summary>発射後の経過時間</summary>
    private float elapsedTime;

    /// <summary>現在火の玉が移動中かどうか</summary>
    private bool isMoving = false;

    /// <summary>火の玉の効果タイプを外部へ通知するイベント</summary>
    public event Action<FireballEffectType> OnEffectTriggered;


    /// <summary>
    /// 火の玉を指定した位置へ向けて発射する。
    /// </summary>
    /// <param name="target">火の玉が向かう目的地のワールド座標</param>
    public void Launch(Vector3 target)
    {
        targetPosition = target;
        startPosition = transform.position;
        isMoving = true;
        elapsedTime = 0f;
    }

    /// <summary>
    /// 毎フレーム呼び出され、火の玉の移動処理を行う。
    /// </summary>
    private void Update()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;  // 前フレームからの経過時間を加算

        // 直線移動の計算
        Vector3 direction = (targetPosition - startPosition).normalized;
        float baseDistance = moveSpeed * elapsedTime;
        Vector3 basePosition = startPosition + direction * baseDistance;

        // 揺れ演出（サイン波）
        float offsetX = Mathf.Sin(elapsedTime * frequencyX) * amplitudeX;
        float offsetY = Mathf.Sin(elapsedTime * frequencyY) * amplitudeY;

        // 直線的に移動した時の位置と上下左右に揺れ動いたときの位置を合成し、火の玉の新たな位置とする
        transform.position = basePosition + new Vector3(offsetX, offsetY, 0);

        // 目的地に到達したら消滅させる
        // 上から下へ移動する前提のため、Y座標で到達判定を行う
        if (transform.position.y <= targetPosition.y)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 火の玉がクリックされたときに呼ばれる。
    /// 効果を発動し、オブジェクトを破壊する。
    /// </summary>
    private void OnMouseDown()
    {
        TriggerRandomEffect();
        Destroy(gameObject);
    }

    /// <summary>
    /// ランダムに火の玉の効果を決定し、
    /// 効果の種類をイベントとして通知する。
    /// </summary>
    private void TriggerRandomEffect()
    {
        FireballEffectType effectType = (FireballEffectType)UnityEngine.Random.Range(0, 3);

        switch (effectType)
        {
            case FireballEffectType.DamageToAllEnemy:
                AudioManager.Instance.PlaySE(attackSoundEffect);
                break;

            case FireballEffectType.DamageToAlly:
                AudioManager.Instance.PlaySE(attackSoundEffect);
                break;

            case FireballEffectType.HealToAlly:
                AudioManager.Instance.PlaySE(healSoundEffect);
                break;
        }

        OnEffectTriggered?.Invoke(effectType);
    }
}


/// <summary>
/// 火の玉が発動する効果の種類を表す列挙型。
/// </summary>
public enum FireballEffectType
{
    DamageToAllEnemy, // 敵全体へのダメージ
    DamageToAlly,    // 味方へのダメージ
    HealToAlly       // 味方への回復
}