using UnityEngine;
using System.Collections.Generic;

public enum MonsterType { Fire, Water, Grass, Other } //炎、水、草、その他 これらは仮


/// <summary>
/// モンスター1体分の静的データを表す ScriptableObject。
/// 図鑑情報・バトル用基本ステータス・所持カード情報などを保持する。
/// </summary>
[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/MonsterData")]
public class MonsterData : ScriptableObject, IDisplayable
{
    [Header("基本情報")]
    /// <summary>モンスター固有の識別ID。図鑑・進行管理・保存データとの紐付けに使用</summary>
    [SerializeField] private int monsterID;

    /// <summary>モンスターの表示名</summary>
    [SerializeField] private string monsterName;

    [Header("ゲームロジック用")]

    /// <summary> モンスターの最大HP</summary>
    [SerializeField] private int maxHP;

    /// <summary>モンスターの攻撃力。敵モンスターの時のみ利用</summary>
    [SerializeField] private int attackPower;

    /// <summary>モンスターの属性タイプ</summary>
    [SerializeField] private MonsterType type;

    /// <summary>モンスターのレアリティ(1~5)</summary>
    [SerializeField] private int rarity;

    [Header("UI表示用")]
    [TextArea]

    /// <summary>図鑑などで表示されるモンスターの説明文</summary>
    [SerializeField] private string description;

    /// <summary>未発見状態のときに表示されるヒント文。</summary>
    [SerializeField] private string hint;

    /// <summary>モンスターの画像表示用スプライト</summary>
    [SerializeField] private Sprite icon;

    /// <summary>シルエット表示用のスプライト。未解放状態の演出などに使用される。</summary>
    [SerializeField] private Sprite shadowIcon;

    /// <summary> このモンスターが提供するカードのリスト。 バトル準備時にデッキ構築の元データとして使用される。</summary>
    [Header("このモンスターが提供するカード (10枚)")]
    public List<CardData> cards = new List<CardData>();


    // --- ゲッターメソッド ---
    /// <summary>モンスターIDを取得する</summary>
    public int GetID() => monsterID;

    /// <summary>モンスター名を取得する</summary>
    public string GetName() => monsterName;

    /// <summary>最大HPを取得する</summary>
    public int GetMaxHP() => maxHP;

    /// <summary>攻撃力を取得する</summary>
    public int GetAttackPower() => attackPower;

    /// <summary>モンスターのタイプを取得する</summary>
    public MonsterType GetMonsterType() => type;

    /// <summary>レアリティを取得する</summary>
    public int GetRarity() => rarity;

    /// <summary>説明文を取得する</summary>
    public string GetDescription() => description;

    /// <summary>未発見時ヒントを取得する</summary>
    public string GetHint() => hint;

    /// <summary>メインアイコンを取得する</summary>
    public Sprite GetIcon() => icon;

    /// <summary>影アイコンを取得する</summary>
    public Sprite GetShadowIcon() => shadowIcon;


    // ===== セッターメソッド =====

    /// <summary>モンスターIDを設定する</summary>
    /// <param name="newID">設定するID</param>
    public void SetID(int newID) { monsterID = newID; }

    /// <summary>モンスター名を設定する</summary>
    /// <param name="newName">設定する名前</param>
    public void SetName(string newName) { monsterName = newName; }

    /// <summary>最大HPを設定する</summary>
    /// <param name="newMaxHP">設定する最大HP</param>
    public void SetMaxHP(int newMaxHP) { maxHP = newMaxHP; }

    /// <summary>攻撃力を設定する</summary>
    /// <param name="newAttack">設定する攻撃力</param>
    public void SetAttackPower(int newAttack) { attackPower = newAttack; }

    /// <summary>モンスタータイプを設定する</summary>
    /// <param name="newType">設定するタイプ</param>
    public void SetMonsterType(MonsterType newType) { type = newType; }

    /// <summary>レアリティを設定する</summary>
    /// <param name="newRarity">設定するレアリティ</param>
    public void SetRarity(int newRarity) { rarity = newRarity; }

    /// <summary>説明文を設定する</summary>
    /// <param name="newDesc">設定する説明文</param>
    public void SetDescription(string newDesc) { description = newDesc; }

    /// <summary>ヒント文を設定する</summary>
    /// <param name="newHint">設定するヒント文</param>
    public void SetHint(string newHint) { hint = newHint; }

    /// <summary>メインアイコンを設定する</summary>
    /// <param name="newIcon">設定するスプライト</param>
    public void SetIcon(Sprite newIcon) { icon = newIcon; }

    /// <summary>影アイコンを設定する</summary>
    /// <param name="newIcon">設定するスプライト</param>
    public void SetShadowIcon(Sprite newIcon) { shadowIcon = newIcon; }


    // ===== 所持数関連 =====

    /// <summary>
    /// プレイヤーがこのモンスターを何体所持しているかを取得する。
    /// 所持データが存在しない場合は0を返す。
    /// </summary>
    /// <returns>所持数</returns>
    public int GetCount()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedMonsters[this];
        }
        return 0;
    }
}