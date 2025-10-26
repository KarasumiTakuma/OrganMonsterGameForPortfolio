// ScriptableObject（スクリプタブルオブジェクト）はデータ専用のオブジェクト。
// 敵モンスターのデータを持つオブジェクトを生成するスクリプト

using UnityEngine;
using System.Collections.Generic;

// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる(Create > Data > EnemyMonserData でデータオブジェを作成)。
[CreateAssetMenu(fileName = "EnemyMonsterData", menuName = "Data/EnemyMonsterData")]
public class EnemyMonsterData : ScriptableObject
{
    [Header("基本情報")]
    public int monsterID;                 // 敵のID(図鑑に登録される時のものと同一)
    public string enemyMonsterName;       // この敵モンスターの名前

    [Header("ゲームロジック用")]
    
    public int maxHP;              // この敵モンスターの最大HP
    public int attackPower;        // この敵モンスターの攻撃力

    [Header("UI表示用")]
    [TextArea]
    public string description;            // Imageクリック時の敵情報表示用

    public Sprite enemyMonsterImage;      // この敵モンスターの画像


    public int GetMonsterID() => monsterID;
    public string GetName() => enemyMonsterName;
    public Sprite GetImage() => enemyMonsterImage;
    public int GetMaxHP() => maxHP;
    public int GetAttackPower() => attackPower;


}
