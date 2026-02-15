using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Monster : MonoBehaviour
{
    protected int monsterID;  // モンスターID
    protected string monsterName;  // モンスター名
    protected int maxHP;  // モンスターの最大HP
    protected int currentHP;  // モンスターの現在のHP(戦闘時のHP)
    protected int attackPower;  // モンスターの攻撃力
    protected Sprite monsterImage;  // モンスターのキャラ画像
    protected bool isDead = false;  // 死亡判定。trueならこのモンスターは死んでいる(小クラスEnemyで利用)

    [SerializeField, Header("ダメージエフェクトの時間")] private float damageEffectTime;
    [SerializeField, Header("エフェクトの点滅回数")] private int blinkCount = 3;

    private Image characterImage; // モンスターの画像コンポーネントを保持する変数
    private Color originalColor;  // モンスターの元画像の色
    private Color damageColor = Color.red;  // ダメージ時のモンスターの色。赤に設定しておく。

    private void Awake()
    {
        characterImage = GetComponentInChildren<Image>();  //　アタッチされている画像コンポーネントを取得
        if(characterImage != null)
            originalColor = characterImage.color;  // モンスターの元画像の色を保持
    }


    protected void InitializeBase(int id, string name, int maxHp, int attack, Sprite image)
    {
        monsterID = id;
        monsterName = name;
        maxHP = maxHp;
        currentHP = maxHp;  // 初期はcurrentHPは最大体力(子クラスEnemyでのみ用いる)
        attackPower = attack;
        monsterImage = image;  // monsterImageに画像データ(Sprite)として保持する

        UpdateImage();
    }
    // 共通のUI更新
    protected void UpdateImage()
    {
        // モンスターの元画像コンポーネントcharacterImageの画像データにmonsterImageの画像データをセットして見た目を更新する
        if (characterImage != null)
            characterImage.sprite = monsterImage;
    }

    protected void PlayDamageEffect()
    {
        if(characterImage == null) return;

        StartCoroutine(DamageEffectCoroutine());
    }

    private IEnumerator DamageEffectCoroutine()
    {
        if(characterImage == null) yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            characterImage.color = damageColor;
            yield return new WaitForSeconds(damageEffectTime);
            
            characterImage.color = originalColor;
            yield return new WaitForSeconds(damageEffectTime);
        }
    }

    // モンスターの初期状態(スポーン時の状態値)設定用メソッド（子クラスから呼び出す）

    // モンスターがダメージを受けた際に呼ばれるメソッド(HP・ダメージ管理)
    protected virtual void TakeDamage(int amount)   // Enemyクラスで参照する
    {
        currentHP = Mathf.Max(currentHP - amount, 0);

        // ダメージエフェクト再生
        PlayDamageEffect();

        if (currentHP <= 0)
        {
            isDead = true;
            OnDeath();
        }
    }


    // このモンスターが死んだときにするべき処理。子クラスEnemyでオーバーライドする
    protected virtual void OnDeath()
    {

    }



    // ゲッター（必要な情報だけ外部に公開）
    public int GetMonsterID() => monsterID;
    public string GetMonsterName() => monsterName;
    public int GetHP() => maxHP;
    public int GetCurrentHP() => currentHP;
    public int GetAttackPower() => attackPower;
    public Sprite GetImage() => monsterImage;
    // 死亡判定のゲッター
    public bool GetIsDead() => isDead;
}
