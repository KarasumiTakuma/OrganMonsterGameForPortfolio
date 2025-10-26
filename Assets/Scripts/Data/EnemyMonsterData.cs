// ScriptableObject（スクリプタブルオブジェクト）はデータ専用のオブジェクト。
// 敵モンスターのデータを持つオブジェクトを生成するスクリプト

using UnityEngine;
using System.Collections.Generic;

// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる(Create > Data > EnemyMonserData でデータオブジェを作成)。
[CreateAssetMenu(fileName = "EnemyMonsterData", menuName = "Data/EnemyMonsterData")]
public class EnemyMonsterData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private int monsterID;                 // 敵のID(図鑑に登録される時のものと同一)
    [SerializeField] private string enemyMonsterName;       // この敵モンスターの名前

    [Header("ゲームロジック用")]
    
    [SerializeField] private int maxHP;              // この敵モンスターの最大HP
    [SerializeField] private int attackPower;        // この敵モンスターの攻撃力

    [Header("UI表示用")]
    [TextArea]
    [SerializeField] private string description;            // Imageクリック時の敵情報表示用

    [SerializeField] private Sprite enemyMonsterImage;      // この敵モンスターの画像


    public int GetMonsterID() => monsterID;
    public string GetName() => enemyMonsterName;
    public int GetMaxHP() => maxHP;
    public int GetAttackPower() => attackPower;
    public Sprite GetImage() => enemyMonsterImage;

}
