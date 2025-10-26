using UnityEngine;
using System.Collections.Generic;

public enum MonsterType { Fire, Water, Grass, Other } //炎、水、草、その他 これらは仮
// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる
[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject, IDisplayable
{
    [Header("基本情報")]
    public int monsterID;
    public string monsterName;

    [Header("ゲームロジック用")]
    public int maxHp;
    public int attackPower;
    // public SkillData specialSkill; // スキルなどもデータとして紐付けられる
    public MonsterType type; //タイプ
    public int rarity; // 1~5

    [Header("UI表示用")]
    [TextArea]
    public string description; // 図鑑用の説明文

    public Sprite icon; // モンスターのイラスト
    public Sprite shadowIcon;

    [Header("このモンスターが提供するカード (10枚)")]
    public List<CardData> cards = new List<CardData>();

    public Sprite GetIcon() { return icon; }
    public string GetName() { return monsterName; }
    public int GetCount() 
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedMonsters[this];
        }
        return 0;
    }
}
