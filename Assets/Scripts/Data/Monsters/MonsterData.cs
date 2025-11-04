using UnityEngine;
using System.Collections.Generic;

public enum MonsterType { Fire, Water, Grass, Other } //炎、水、草、その他 これらは仮
// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる
[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/MonsterData")]
public class MonsterData : ScriptableObject, IDisplayable
{
    [Header("基本情報")]
    [SerializeField] private int monsterID;
    [SerializeField] private string monsterName;

    [Header("ゲームロジック用")]
    [SerializeField] private int maxHp;
    [SerializeField] private int attackPower;
    // public SkillData specialSkill; // スキルなどもデータとして紐付けられる
    [SerializeField] private MonsterType type; //タイプ
    [SerializeField] private int rarity; // 1~5

    [Header("UI表示用")]
    [TextArea]
    [SerializeField] private string description; // 図鑑用の説明文
    [SerializeField] private string hint; // 未発見の時のヒント

    [SerializeField] private Sprite icon; // モンスターのイラスト
    [SerializeField] private Sprite shadowIcon;

    [Header("このモンスターが提供するカード (10枚)")]
    public List<CardData> cards = new List<CardData>();


    // --- ゲッターメソッド ---
    public int GetID() => monsterID;
    public string GetName() => monsterName;
    public int GetHP() => maxHp;
    public int GetAttackPower() => attackPower;
    public MonsterType GetMonsterType() => type;
    public int GetRarity() => rarity;
    public string GetDescription() => description;
    public string GetHint() => hint;
    public Sprite GetIcon() => icon;
    public Sprite GetShadowIcon() => shadowIcon;

    // --- セッターメソッド ---
    public void SetID(int newID) { monsterID = newID; }
    public void SetName(string newName) { monsterName = newName; }
    public void SetHP(int newHP) { maxHp = newHP; }
    public void SetAttackPower(int newAttack) { attackPower = newAttack; }
    public void SetMonsterType(MonsterType newType) { type = newType; }
    public void SetRarity(int newRarity) { rarity = newRarity; }
    public void SetDescription(string newDesc) { description = newDesc; }
    public void SetHint(string newHint) { hint = newHint; }
    public void SetIcon(Sprite newIcon) { icon = newIcon; }
    public void SetShadowIcon(Sprite newIcon) { shadowIcon = newIcon; }

    public int GetCount() 
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedMonsters[this];
        }
        return 0;
    }
}
