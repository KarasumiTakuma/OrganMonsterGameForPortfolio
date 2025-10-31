using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 味方1体の情報を管理するスクリプト
/// 画像・名前・攻撃力・最大HPなどを保持
/// 個別HPは表示用で、実際のHP管理は AllyAreaManager の共有HPで行う
/// </summary>
public class Ally : MonoBehaviour
{
    // 味方モンスターのID
    private int monsterID;

    // 味方の名前
    private string allyMonsterName;

    // 最大HP（表示用）
    private int maxHP;

    // 攻撃力（将来的にカード効果などで利用）
    private int attackPower;

    // 味方キャラクター画像
    private Sprite allyMonsterImage;

    // 初期設定用メソッド
    public void InitializeSet(AllyMonsterData allyMonsterData)
    {
        this.monsterID = allyMonsterData.GetMonsterID();
        this.allyMonsterName = allyMonsterData.GetName();
        this.maxHP = allyMonsterData.GetMaxHP();
        this.attackPower = allyMonsterData.GetAttackPower();
        this.allyMonsterImage = allyMonsterData.GetImage();

        // AllyPrefab内の Image コンポーネントに画像をセット
        Image imageComp = GetComponentInChildren<Image>();
        if (imageComp != null)
        {
            imageComp.sprite = this.allyMonsterImage;
        }
    }

    // 以下は getter メソッド
    public int GetMonsterID() => monsterID;
    public string GetMonsterName() => allyMonsterName;
    public int GetMaxHP() => maxHP;
    public int GetAttackPower() => attackPower;
    public Sprite GetImage() => allyMonsterImage;
}
