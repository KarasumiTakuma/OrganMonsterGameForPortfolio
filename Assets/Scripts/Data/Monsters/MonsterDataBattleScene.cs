// ScriptableObject（スクリプタブルオブジェクト）はデータ専用のオブジェクト。
// モンスターのデータを持つオブジェクトを生成するスクリプト

using UnityEngine;
using System.Collections.Generic;

public class MonsterDataBattleScene : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private int monsterID;                 // このモンスターのID(図鑑に登録される時のものと同一)
    [SerializeField] private string monsterName;       // このモンスターの名前

    [Header("ゲームロジック用")]
    
    [SerializeField] private int maxHP;              // このモンスターの最大HP
    [SerializeField] private int attackPower;        // このモンスターの攻撃力

    [Header("UI表示用")]
    [SerializeField] private Sprite monsterImage;      // このモンスターの画像


    public int GetMonsterID() => monsterID;
    public string GetName() => monsterName;
    public int GetMaxHP() => maxHP;
    public int GetAttackPower() => attackPower;
    public Sprite GetImage() => monsterImage;

}
