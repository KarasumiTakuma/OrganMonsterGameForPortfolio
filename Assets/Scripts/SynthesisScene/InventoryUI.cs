using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ToList()を使うために必要

/// <summary>
/// プレイヤーの所持品データを元に、インベントリのUI表示を管理するクラス。
/// ゲーム開始時に指定された最大数まで空のスロットを生成し、
/// データに応じて各スロットの表示内容を更新する。
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public GameObject inventorySlotPrefab; // スロットのプレハブ
    public Transform contentPanel;         // スロットを生成する親オブジェクト
    public int maxSlots = 50;             // インベントリの最大スロット数

    // 生成した全スロットの参照を保存しておくリスト
    public List<InventorySlotUI> SlotUIs { get; private set; } = new List<InventorySlotUI>();

    // スロットの参照を渡すゲッターメソッド
    //public List<InventorySlotUI> GetSlotUIs() => SlotUIs;

    private void OnEnable()
    {
        // PlayerDataからの放送（OnInventoryChanged）を聞き始めたら、
        // 自分のUpdateDisplayメソッドを呼び出すように予約する
        PlayerData.OnInventoryChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        // このUIが非表示になったら、放送を聞くのをやめる（メモリリーク防止）
        PlayerData.OnInventoryChanged -= UpdateDisplay;
    }

    void Start()
    {
        // 最初に空のスロットを最大数だけ生成する
        for (int i = 0; i < maxSlots; i++)
        {
            // prefabのクローンをcontentPanelに配置
            GameObject slotGO = Instantiate(inventorySlotPrefab, contentPanel);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            slotUI.ClearSlot(); // スロットを空の状態にする
            SlotUIs.Add(slotUI);
        }

        // インベントリの初期表示を更新
        UpdateDisplay();
    }

    // インベントリの表示を更新する関数
    public void UpdateDisplay()
    {
        // GameManager.Instance.PlayerDataから最新の所持品リストを取得
        var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        List<OrganData> ownedOrgansList = ownedOrgans.Keys.ToList();
        // ID順の昇順で並び替える
        ownedOrgansList = ownedOrgansList.OrderBy(organ => organ.organID).ToList();

        // 全スロットをループして、表示を更新する
        for (int i = 0; i < SlotUIs.Count; i++)
        {
            // 表示すべきアイテムがまだある場合
            if (i < ownedOrgansList.Count)
            {
                OrganData organ = ownedOrgansList[i];
                int count = ownedOrgans[organ];
                SlotUIs[i].Setup(organ, count); // スロットにデータを設定
            }
            // 表示すべきアイテムがもうない（空のスロット）の場合
            else
            {
                SlotUIs[i].ClearSlot(); // スロットを空にする
            }
        }
    }
}