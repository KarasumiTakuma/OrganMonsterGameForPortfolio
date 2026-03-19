using UnityEngine;
using System;

/// <summary>
/// Fireball（火の玉）を一定間隔で生成するかどうかを判定し、
/// ランダムな位置にスポーンさせる管理クラス。
/// ・スポーン開始 / 停止の制御
/// ・出現確率による生成判定
/// ・Fireball が確定させた効果を外部へ中継
/// を行う。
/// Fireball 自身の挙動（移動・効果決定）には関与せず、
///「生成と管理」に責務を限定している。
/// </summary>
public class FireballManager : MonoBehaviour
{
    /// <summary>生成する Fireball のプレハブ。Fireballコンポーネントがアタッチされていることが前提</summary>
    [SerializeField] private GameObject fireballPrefab;

    /// <summary>
    /// Fireball を生成する基準位置。
    /// 実際の生成位置は、この座標を中心にランダムなオフセットを加えた位置になる。
    /// </summary>
    [SerializeField] private Transform spawnBasePoint;

    /// <summary>
    /// Fireball の生成判定を行う間隔（秒）。
    /// InvokeRepeatingによって、この間隔でTrySpawnFireballが呼ばれる。
    /// </summary>
    [SerializeField] private float spawnInterval = 1f;

    /// <summary>Fireball が実際に出現する確率（0.0 ～ 1.0）。 例：0.7 の場合、約70%の確率で出現する</summary>
    [SerializeField] private float spawnChance = 0.7f;

    /// <summary> 現在 Fireball のスポーン処理が実行中かどうか。多重にInvokeRepeatingを開始しないためのフラグ</summary>
    private bool isSpawning = false;

    /// <summary>
    /// Fireballの効果タイプが確定した際に通知されるイベント。
    /// Fireball側のOnEffectTriggeredを受け取り外部へ中継する。
    /// </summary>
    public event Action<FireballEffectType> OnFireballEffectTriggered;  // Fireballの効果が確定した際にそれを通知するイベント


    /// <summary>
    /// Fireball のスポーン処理を開始する。
    /// 既に実行中の場合は何もしない。
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;

        // 一定間隔で Fireball の生成判定を行う
        InvokeRepeating(nameof(TrySpawnFireball), 1f, spawnInterval);
    }

    /// <summary>
    /// Fireball のスポーン処理を停止する。
    /// InvokeRepeating による次回以降の呼び出しをキャンセルする。
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning) return;

        isSpawning = false;

        // TrySpawnFireball の定期呼び出しを停止
        CancelInvoke(nameof(TrySpawnFireball));
    }

    /// <summary>
    /// Fireball を生成するかどうかを判定し、
    /// 条件を満たした場合に Fireball をスポーンさせる。
    /// ・spawnChance による確率判定
    /// ・ランダムな生成位置の決定
    /// ・Fireball の初期化とイベントの購読
    /// を行う
    /// </summary>
    public void TrySpawnFireball()
    {
        // 出現確率に満たなければ何もしない
        if (UnityEngine.Random.value > spawnChance) return;

        // 基準位置からランダムなオフセットを加えた生成位置を決定
        Vector3 spawnPosition = spawnBasePoint.position;

        float offsetX = UnityEngine.Random.Range(-8.0f, 5.0f);
        float offsetY = UnityEngine.Random.Range(-0.5f, 0.5f);

        spawnPosition += new Vector3(offsetX, offsetY, 0);

        // Fireball プレハブを生成
        var fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);

        // Fireball コンポーネントを取得
        var fireballComponent = fireball.GetComponent<Fireball>();
        if (fireballComponent == null) // コンポーネントを正しく取得できたかをチェック
        {
            Debug.LogError("Fireball コンポーネントが付いていません");
            return;
        }

        // Fireball の移動先を設定（下方向へ飛ばす）
        Vector3 targetPosition = new Vector3(spawnPosition.x, spawnPosition.y - 10f, spawnPosition.z);
        fireballComponent.Launch(targetPosition);

        // Fireballの効果タイプの通知イベントを中継する
        fireballComponent.OnEffectTriggered += HandleFireballEffect;
    }

    /// <summary>
    /// Fireballから通知された効果タイプを外部へ中継する。
    /// </summary>
    private void HandleFireballEffect(FireballEffectType effectType)
    {
        OnFireballEffectTriggered?.Invoke(effectType);
    }
}
