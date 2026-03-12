using System.Collections;
using UnityEngine;

/// <summary>
/// 具体的な行動パターン：通常攻撃のみを行うAI
/// EnemyLogicBase を継承しているので、EnemyMonsterDataにセットできる
/// </summary>
[CreateAssetMenu(fileName = "SimpleAttackLogic", menuName = "EnemyLogic/Simple Attack")]
public class SimpleAttackLogic : EnemyLogicBase
{
    [SerializeField] private AudioClip attackSound; // 攻撃音

    // ここに「敵のターンに何をするか」を具体的に書く
    public override IEnumerator ExecuteTurn(Enemy self, AllyAreaManager allyManager, EnemyAreaManager enemyManager)
    {
        // 1. ログを出す
        Debug.Log($"{self.GetMonsterName()} の攻撃！");

        // 2. 少し待つ（演出用）
        yield return new WaitForSeconds(0.5f);

        // 3. ダメージ計算 (例: 攻撃力の 90% ~ 110% のランダムダメージ)
        int damage = Mathf.RoundToInt(self.GetAttackPower() * Random.Range(0.9f, 1.1f));

        // 4. プレイヤー（共有HP）にダメージを与える
        allyManager.TakeDamageToSharedHP(damage);

        // 5. 音を鳴らす
        if (attackSound != null)
        {
            AudioManager.Instance.PlaySE(attackSound);
        }

        // 6. 攻撃後の余韻
        yield return new WaitForSeconds(0.5f);
    }
}