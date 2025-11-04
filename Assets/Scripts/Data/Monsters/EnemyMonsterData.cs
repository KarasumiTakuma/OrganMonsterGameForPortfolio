using UnityEngine;
using System.Collections.Generic;

// この行を追加することで、Unityのメニューからデータアセットを作成できるようになる(Create > Data > EnemyMonserData でデータオブジェを作成)。
[CreateAssetMenu(fileName = "EnemyMonsterData", menuName = "Data/EnemyMonsterData")]
public class EnemyMonsterData : MonsterDataBattleScene
{
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string description;            // Imageクリック時の敵情報表示用
}
