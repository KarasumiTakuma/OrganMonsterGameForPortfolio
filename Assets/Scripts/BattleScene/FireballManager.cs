using UnityEngine;

// Fireballをランダム時間で生成するかを決定し、そのFireballの出現位置をランダムで決定し、生成する制御クラス
public class FireballManager : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;  // 火の玉オブジェクト(プレファブ)
    [SerializeField] private Transform spawnArea; // 生成位置
    [SerializeField] private float spawnInterval = 1f; // メソッドの呼び出し間隔
    [SerializeField] private float chance = 0.7f; // 出現確率

    private void Start()
    {
        // InvokeRepeatingは指定したメソッドを一定時間ごとに自動で呼び続ける
        // InvokeRepeating( メソッド名 , 初回実行までの時間(t) , 実行間隔 )
        // メソッド名は呼び出したいメソッドの名前, 初回実行までの時間は、ゲーム開始t秒後にメソッドを初めて呼ぶ
        // 実行間隔は、その秒数ごとに呼び出す。spawnInterval = 5なら、初回呼び出し後に、5秒間隔でメソッドが呼び出される
        InvokeRepeating("TrySpawnFireball", 1f, spawnInterval);
    }

    public void TrySpawnFireball()
    {
        // Random.valueは、0〜1のランダム値
        // Random.value > chanceなら火の玉を出さずに終了
        if (Random.value > chance) return;
        
        // Random.value <= chanceなら、
        // 生成位置からズレた位置X方向 -2 〜 +2, Y方向 -0.5 〜 +0.5 
        // の範囲でランダムに火の玉オブジェクトのスポーン位置を決めて、
        Vector3 spawnPosition = spawnArea.position;
        spawnPosition += new Vector3(Random.Range(-2f, 2f), Random.Range(-0.5f, 0.5f), 0);

        // fireballPrefab(火の玉オブジェクト)をスポーン位置(spawnPosition)に生成
        // Quaternion.identityは回転が0°であることを示す
        var fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);

        // Fireball を飛ばす先をLaunchメソッドで設定
        Vector3 targetPosition = new Vector3(spawnPosition.x, spawnPosition.y - 10f, spawnPosition.z);
        fireball.GetComponent<Fireball>().Launch(targetPosition);
    }
}
