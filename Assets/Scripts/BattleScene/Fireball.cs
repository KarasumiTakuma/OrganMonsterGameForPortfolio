using UnityEngine;

// 戦闘中に飛んでくる火の玉(Fireball)を管理するスクリプト
public class Fireball : MonoBehaviour
{
    [SerializeField, Header("火の玉の移動速度(1秒あたり)")] private float speed;  // 火の玉が移動する速度(1秒あたり)

    [SerializeField, Header("敵に与えるダメージ量")] private int damageToEnemy;  // 敵に与えるダメージ量
    [SerializeField, Header("味方に与えるダメージ量")] private int damageToAlly;  // 味方に与えるダメージ量
    [SerializeField, Header("味方を回復する量")] private int healToAlly;  // 味方を回復する量


    [Header("火の玉の揺れ設定")]
    [SerializeField] private float amplitudeX = 0.5f; // 左右の揺れ幅
    [SerializeField] private float frequencyX = 3f;   // 左右の揺れの速さ
    [SerializeField] private float amplitudeY = 0.5f; // 上下の揺れ幅
    [SerializeField] private float frequencyY = 3f;   // 上下の揺れの速さ

    [Header("SE設定")]
    [SerializeField] private AudioClip AttackSoundEffect;  // 攻撃の効果音
    [SerializeField] private AudioClip HealSoundEffect; // 回復の効果音

    private Vector3 targetPosition;    // 火の玉の目的地座標
    private Vector3 startPosition;   // 火の玉の初期位置
    private float elapsedTime;  // 火の玉が移動してからの経過時間をフレームごとに加算するための変数
    private bool isMoving = false;  // 火の玉が移動中かどうかのフラグ

    // 火の玉を指定の位置まで飛ばすときに呼ぶメソッド
    public void Launch(Vector3 target)
    {
        targetPosition = target;  // 火の玉の目的位置targetをtargetPositionにセット
        startPosition = transform.position;  // 発射された瞬間の位置(スポーン位置)をセット
        isMoving = true;  // isMovingをtrueにして、Update()の処理を開始させる
        elapsedTime = 0f;
    }

    // 
    //毎フレーム呼ばれるメソッド
    void Update()
    {
        if (!isMoving) return;  // 移動中フラグがfalseなら、処理しない


        elapsedTime += Time.deltaTime;  // 前フレームからの経過時間を加算

        // 直線移動
        // スタート位置から目的位置へのベクトルを単位ベクトルに正規化。進行方向を示すベクトル
        Vector3 direction = (targetPosition - startPosition).normalized;
        // baseDistance = 直線で進む距離：進む大きさ を計算
        // speed = 5 なら、 1秒あたり5ずつ移動。
        // フレームレートが60fpsの場合、1フレームあたりの移動距離は 5 * (1/60) ≈ 0.0833
        float baseDistance = speed * elapsedTime;
        // スタート位置startPositionから進行方向directionに進む距離baseDistance分だけ移動させた時の位置を計算
        Vector3 basePosition = startPosition + direction * baseDistance;

        // Sin波で揺れを追加
        // 時間経過に応じて左右に揺れ動くときの、火の玉の位置をサイン波で表現
        // frequencyXは、揺れの速度(：周波数)、amplitudeXは揺れ幅(：振幅)
        float offsetX = Mathf.Sin(elapsedTime * frequencyX) * amplitudeX;
        // 同様に、時間経過に応じて上下に揺れ動くときの、火の玉の位置をサイン波で表現
        float offsetY = Mathf.Sin(elapsedTime * frequencyY) * amplitudeY;

        // 直線的に移動した時の位置と上下左右に揺れ動いたときの位置を合成し、火の玉の新たな位置とする
        transform.position = basePosition + new Vector3(offsetX, offsetY, 0);

        // オブジェクトの位置(transform.position)が目的地(targetPosition)よりも低い位置に来たら
        if (transform.position.y <= targetPosition.y)
        {
            Destroy(gameObject); // オブジェクトを破壊(火の玉を消す) 
        }
    }

    // クリック時に呼び出される 
    void OnMouseDown()
    {
        TriggerEffect(); // 火の玉をクリックしたら、ランダムで効果が発動(TriggerEffect()を呼び出す) 
        Destroy(gameObject); // 1回クリックしたら、オブジェクトを破壊(火の玉を消す) 
    }

    // 火の玉の効果の実行（ランダムで何が起こるか決まる） 
    private void TriggerEffect()
    {
        // ランダムで効果決定 
        int effectNumber = Random.Range(0, 3);
        switch (effectNumber)
        {
            case 0:
                // 敵にダメージ 
                Object.FindAnyObjectByType<EnemyAreaManager>()?.TakeDamageToAll(damageToEnemy);
                AudioManager.Instance.PlaySE(AttackSoundEffect);
                break;
            case 1:
                // 味方にダメージ 
                Object.FindAnyObjectByType<AllyAreaManager>()?.TakeDamageToSharedHP(damageToAlly);
                AudioManager.Instance.PlaySE(AttackSoundEffect);
                break;
            case 2:
                // 味方回復 
                Object.FindAnyObjectByType<AllyAreaManager>()?.HealSharedHP(healToAlly);
                AudioManager.Instance.PlaySE(HealSoundEffect);
                break;
        }
    }
}
