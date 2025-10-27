// using UnityEngine;
// using System.Collections.Generic;

// /// <summary>
// /// プレイヤーが所持している臓器とその数を管理するシングルトン。
// /// 臓器の追加や削除といった、インベントリのデータ操作は全てこのクラスを通して行う。
// /// </summary>
// public class InventoryManager : MonoBehaviour
// {
//     public static InventoryManager Instance { get; private set; }

//     // どの臓器(OrganData)を、いくつ(int)持っているかを管理する辞書
//     public Dictionary<OrganData, int> ownedOrgans = new Dictionary<OrganData, int>();

//     void Awake()
//     {
//         Instance = this;
//     }

//     // 臓器を追加するメソッド
//     public void AddOrgan(OrganData organ, int amount)
//     {
//         if (ownedOrgans.ContainsKey(organ))
//         {
//             ownedOrgans[organ] += amount; // 既に持っていれば数を増やす
//         }
//         else
//         {
//             ownedOrgans.Add(organ, amount); // 新しく追加
//         }

//         Debug.Log(organ.organName + " を " + amount + "個追加しました。");
//         // ここでUI更新のイベントを発行するのが理想的
//     }

//     // 臓器を削除するメソッド
//     public void RemoveOrgan(OrganData organ)
//     {
//         ownedOrgans[organ]--;
//         if (ownedOrgans[organ] <= 0)
//         {
//             ownedOrgans.Remove(organ);
//         }
//     }
// }