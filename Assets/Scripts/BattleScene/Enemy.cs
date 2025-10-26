using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵1体の情報とダメージ等の挙動を管理するスクリプト
/// HP管理、攻撃力、画像などを保持
/// </summary>
public class Enemy : MonoBehaviour
{
    // 敵モンスターのモンスターID
    private int monsterID;

    // 敵の名前
    private string enemyMonsterName;

    // 最大HP
    private int maxHP;

    // 現在のHP
    private int currentHP;

    // 敵の攻撃力（将来的に攻撃処理に利用）
    private int attackPower;

    // 敵キャラクター画像
    private Sprite enemyMonsterImage;

    // HPゲージを操作するコンポーネント
    [SerializeField] private HpGaugeController hpGauge;

    // 敵の初期設定用メソッド
    public void InitializeSet(EnemyMonsterData enemyMonsterData)
    {
        this.monsterID = enemyMonsterData.GetMonsterID();
        this.enemyMonsterName = enemyMonsterData.GetName();
        this.maxHP = enemyMonsterData.GetMaxHP();
        this.currentHP = enemyMonsterData.GetMaxHP();  // 初期はcurrentHPは最大体力
        this.attackPower = enemyMonsterData.GetAttackPower();
        this.enemyMonsterImage = enemyMonsterData.GetImage(); // enemyMonsterImageに画像データ(Sprite)として保持する

        //(EnemyPrefabのEnemyCharacterの)ImageコンポーネントにenemyMonsterImageをセットして見た目を更新する
        Image imageComp = GetComponentInChildren<Image>(); // Enemyオブジェクト(EnemyPrefab)の子以下にあるImageコンポーネントを取得
        if (imageComp != null)
            imageComp.sprite = this.enemyMonsterImage;  //imageコンポーネントの表示をenemyMonsterImageに差し替える
    }



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


    public int GetMonsterID() => this.monsterID;
    public string GetMonsterName() => this.enemyMonsterName;
    public int GetMaxHP() => this.maxHP;
    public int GetCurrentHP() => this.currentHP;
    public int GetAttackPower() => this.attackPower;
    public Sprite GetImage() => this.enemyMonsterImage;
}
