using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    protected int monsterID;  // モンスターID
    protected string monsterName;  // モンスター名
    protected int maxHP;  // モンスターの最大HP
    protected int currentHP;  // モンスターの現在のHP(戦闘時のHP)
    protected int attackPower;  // モンスターの攻撃力
    protected Sprite monsterImage;  // モンスターのキャラ画像

    // 共通のUI更新
    protected void UpdateImage()
    {
        //(MonsterPrefabのMonsterCharacterの)ImageコンポーネントにmonsterImageをセットして見た目を更新する
        Image img = GetComponentInChildren<Image>();  // Monsterオブジェクト(MonsterPrefab)の子以下にあるImageコンポーネントを取得
        if (img != null)
            img.sprite = monsterImage; // imageコンポーネントの表示をmonsterImageに差し替える
    }

    // モンスターの初期状態(スポーン時の状態値)設定用メソッド（子クラスから呼び出す）
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

    // モンスターがダメージを受けた際に呼ばれるメソッド(HP・ダメージ管理)
    protected virtual void TakeDamage(int amount)   // Enemyクラスで参照する
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
    }

    // 生存判定
    protected bool IsAlive() => currentHP > 0;

    // ゲッター（必要な情報だけ外部に公開）
    public int GetMonsterID() => monsterID;
    public string GetMonsterName() => monsterName;
    public int GetMaxHP() => maxHP;
    public int GetCurrentHP() => currentHP;
    public int GetAttackPower() => attackPower;
    public Sprite GetImage() => monsterImage;
}
