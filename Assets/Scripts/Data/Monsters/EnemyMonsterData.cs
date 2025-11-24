using UnityEngine;
using System.Collections.Generic;

// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる(Create > Data > EnemyMonserData でデータオブジェを作成)。
[CreateAssetMenu(fileName = "EnemyMonsterData", menuName = "Data/EnemyMonsterData")]
public class EnemyMonsterData : MonsterData
{
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string enemyDescription;            // Imageクリック時の敵情報表示用

    public string GetEnemyDescription() => enemyDescription;  // descriptionを外部から取得するためのゲッター
}