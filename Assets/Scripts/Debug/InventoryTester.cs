using System.Collections.Generic;
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    // Inspectorで、テスト追加したい臓器アセットを設定する
    public List<OrganData> testOrgan = new List<OrganData>();

    // InspectorでInventorySystemがアタッチされているオブジェクトを設定


    void Update()
    {
        // 「P」キーが押されたら
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 1. PlayerDataにテスト用の臓器を1個追加する
            foreach (OrganData organ in testOrgan)
            {
                GameManager.Instance.PlayerData.AddOrgan(organ, 1);
            }
        }

        // 2. ★★★ UIの表示更新を命令する ★★★
        // if (inventorySystem != null)
        // {
        //     inventorySystem.ShowOrganPanel(); // これがPopulateOrganGridを呼び出す
        // }
    }
}