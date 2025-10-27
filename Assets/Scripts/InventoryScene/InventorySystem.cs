using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// インベントリシーンのシステムを構成するクラス
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("UI Panels")]
    //[SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject organPanel;
    [SerializeField] private GameObject monsterPanel;
    [SerializeField] private GameObject artifactPanel;

    [Header("Grid Contents")]
    [SerializeField] private Transform organGridContent;
    [SerializeField] private Transform monsterGridContent;
    [SerializeField] private Transform artifactGridContent;

    [Header("Tab Buttons")]
    [SerializeField] private Button organTabButton;
    [SerializeField] private Button monsterTabButton;
    [SerializeField] private Button artifactTabButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject genericSlotPrefab;

    [Header("Tab Colors")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color deselectedColor = Color.white;

    // 生成したスロットの参照を保存しておくリスト
    private List<GenericSlotUI> organSlots = new List<GenericSlotUI>();
    private List<GenericSlotUI> monsterSlots = new List<GenericSlotUI>();
    private List<GenericSlotUI> artifactSlots = new List<GenericSlotUI>();
    public int maxSlots;
    // 現在選択されているスロットのデータを保持する
    private ScriptableObject selectedItem;

    void Start()
    {
        // --- 事前に空のスロットを生成 ---
        // 臓器用スロット
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, organGridContent);
            organSlots.Add(slotGO.GetComponent<GenericSlotUI>());
        }
        // モンスター用スロット
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, monsterGridContent);
            monsterSlots.Add(slotGO.GetComponent<GenericSlotUI>());
        }
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, artifactGridContent);
            artifactSlots.Add(slotGO.GetComponent<GenericSlotUI>());
        }

        // タブボタンに、対応するパネルを表示するメソッドを登録
        organTabButton?.onClick.AddListener(ShowOrganPanel);
        monsterTabButton?.onClick.AddListener(ShowMonsterPanel);
        artifactTabButton?.onClick.AddListener(ShowArtifactPanel);

        // 最初は臓器パネルを表示
        ShowOrganPanel();
    }

    private void OnEnable()
    {
        // 詳細表示などのためにクリックイベントを購読
        // クリックイベントを購読
        GenericSlotUI.OnSlotClicked += HandleSlotClick;
    }

    private void OnDisable()
    {
        // 購読を解除
        GenericSlotUI.OnSlotClicked -= HandleSlotClick;
    }


    private void HandleSlotClick(ScriptableObject clickedData)
    {
        Debug.Log(clickedData);
        if (clickedData == null) return;

        // もし、クリックされたアイテムが既に選択中のものだったら
        if (selectedItem == clickedData)
        {
            // 選択を解除する
            selectedItem = null;
        }
        else
        {
            // 新しくそのアイテムを選択する
            selectedItem = clickedData;
        }

        // ここで詳細パネルにselectedItemの情報を表示する処理を呼ぶ
        // ShowDetail(selectedItem);

        // 全てのスロットの色を更新
        UpdateAllSlotColors();
    }

    private void UpdateAllSlotColors()
    {
        // organSlotsとmonsterSlotsの両方をチェック
        foreach (var slot in organSlots.Concat(monsterSlots))
        {
            var dataInSlot = slot.GetAssignedData();
            // このスロットのデータが、選択中のデータと一致するか？
            bool isSelected = dataInSlot != null && dataInSlot == selectedItem;
            slot.SetSelected(isSelected);
        }
    }

    private void UpdateTabColors()
    {
        // organPanelが表示されているかどうかに応じて色を設定
        organTabButton.GetComponent<Image>().color = organPanel.activeSelf ? selectedColor : deselectedColor;
        monsterTabButton.GetComponent<Image>().color = monsterPanel.activeSelf ? selectedColor : deselectedColor;
        artifactTabButton.GetComponent<Image>().color = artifactPanel.activeSelf ? selectedColor : deselectedColor;
    }

    public void ShowOrganPanel()
    {
        //Debug.Log("Organパネル表示");
        organPanel.SetActive(true);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(false);
        PopulateOrganGrid();
        UpdateTabColors();
    }

    public void ShowMonsterPanel()
    {
        //Debug.Log("Monsterパネル表示");
        organPanel.SetActive(false);
        artifactPanel.SetActive(false);
        monsterPanel.SetActive(true);
        PopulateMonsterGrid();
        UpdateTabColors();
    }

    public void ShowArtifactPanel()
    {
        //Debug.Log("Artifactパネル表示");
        organPanel.SetActive(false);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(true);
        PopulateArtifactGrid();
        UpdateTabColors();
    }

    // 臓器グリッドにデータを表示
    private void PopulateOrganGrid()
    {
        // PlayerDataから臓器リストを取得し、ID順でソート
        var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        List<OrganData> sortedOrganKeys = ownedOrgans.Keys.OrderBy(k => k.organID).ToList();

        // 全スロットをループして、表示を更新
        for (int i = 0; i < organSlots.Count; i++)
        {
            if (i < sortedOrganKeys.Count)
            {
                OrganData organ = sortedOrganKeys[i];
                int count = ownedOrgans[organ];
                organSlots[i].gameObject.SetActive(true); // スロットを表示
                organSlots[i].Setup(organ, count);
            }
            else
            {
                organSlots[i].Clear();
                //organSlots[i].gameObject.SetActive(false); // 不要なスロットは非表示
            }
        }
    }

    // モンスターグリッドにデータを表示
    private void PopulateMonsterGrid()
    {
        var ownedMonsters = GameManager.Instance.PlayerData.ownedMonsters;
        List<MonsterData> sortedMonsterKeys = ownedMonsters.Keys.OrderBy(k => k.monsterID).ToList();

        for (int i = 0; i < monsterSlots.Count; i++)
        {
            if (i < sortedMonsterKeys.Count)
            {
                MonsterData monster = sortedMonsterKeys[i];
                int count = ownedMonsters[monster];
                monsterSlots[i].gameObject.SetActive(true);
                monsterSlots[i].Setup(monster, count);
            }
            else
            {
                monsterSlots[i].Clear();
                //monsterSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void PopulateArtifactGrid()
    {
        var ownedArtifacts = GameManager.Instance.PlayerData.ownedArtifacts;
        List<ArtifactData> sortedArtifacts = ownedArtifacts.OrderBy(k => k.artifactID).ToList();

        for (int i = 0; i < artifactSlots.Count; i++)
        {
            if (i < sortedArtifacts.Count)
            {
                ArtifactData artifact = sortedArtifacts[i];
                //int count = ownedArtifacts[artifact];
                artifactSlots[i].gameObject.SetActive(true);
                artifactSlots[i].Setup(artifact);
            }
            else
            {
                artifactSlots[i].Clear();
                //monsterSlots[i].gameObject.SetActive(false);
            }
        }
    }
}