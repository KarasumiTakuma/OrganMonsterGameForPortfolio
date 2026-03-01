using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomBehaviorLogic", menuName = "EnemyLogic/Random Behavior")]
public class RandomBehaviorLogic : EnemyLogicBase
{
    [Header("確率設定 (合計100になるように)")]
    [SerializeField, Range(0, 100)] private int attackChance = 60;
    [SerializeField, Range(0, 100)] private int healChance = 20;
    // インスペクター上では合計100になるように設定するが、実際のロジックでは attackChance と healChance の合計を引いた残りをガード確率として扱う(バグ防止)
    //[SerializeField, Range(0, 100)] private int guardChance = 20;
    [Header("アクション設定")]
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float guardMultiplier = 0.5f; // ダメージ半減
    [SerializeField] private int guardDuration = 3;        // 3ターン持続

    [Header("効果音")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip guardSound;

    public override IEnumerator ExecuteTurn(Enemy self, AllyAreaManager allyManager, EnemyAreaManager enemyManager)
    {
        int roll = Random.Range(0, 100);

        // --- 攻撃 (0 ~ 59) ---
        if (roll < attackChance)
        {
            Debug.Log($"{self.GetMonsterName()} の攻撃！");
            yield return new WaitForSeconds(0.5f);

            int damage = self.GetAttackPower();
            allyManager.TakeDamageToSharedHP(damage);

            if (attackSound != null) AudioManager.Instance.PlaySE(attackSound);
        }
        // --- 回復 (60 ~ 79) ---
        else if (roll < attackChance + healChance)
        {
            Debug.Log($"{self.GetMonsterName()} の回復行動！");
            yield return new WaitForSeconds(0.5f);

            self.Heal(healAmount);

            if (healSound != null) AudioManager.Instance.PlaySE(healSound);
        }
        // --- ガード (80 ~ 99) ---
        else
        {
            Debug.Log($"{self.GetMonsterName()} は身を固めた！");
            yield return new WaitForSeconds(0.5f);

            // ★ここで「倍率」と「ターン数」を指定してバリアを張る
            // 既にバリア中でも、この処理でターン数が3に「上書き」される
            self.SetGuard(guardMultiplier, guardDuration);

            if (guardSound != null) AudioManager.Instance.PlaySE(guardSound);
        }

        yield return new WaitForSeconds(0.5f);
    }
}