using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class HistorySystem : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject HistoryPanel;
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

    // --- 全アイテムリスト ---
    private List<OrganData> allOrgansInGame = new List<OrganData>();
    private List<MonsterData> allMonstersInGame = new List<MonsterData>();
    private List<ArtifactData> allArtifactsInGame = new List<ArtifactData>();

    // --- 事前生成したスロット ---
    private List<GenericSlotUI> organSlots = new List<GenericSlotUI>();
    private List<GenericSlotUI> monsterSlots = new List<GenericSlotUI>();
    private List<GenericSlotUI> artifactSlots = new List<GenericSlotUI>();

    // 現在選択されているアイテム
    private ScriptableObject selectedItem;

    void Start()
    {
        // --- ゲーム内の全アイテムデータをロード ---
        // --- DataManagerからロード済みのリストを取得 ---
        allOrgansInGame = DataManager.Instance.AllOrgans;
        allMonstersInGame = DataManager.Instance.AllMonsters;
        allArtifactsInGame = DataManager.Instance.AllArtifacts;

        // --- 事前に空のスロットを生成 ---
        GenerateSlots(allOrgansInGame.Count, organGridContent, organSlots);
        GenerateSlots(allMonstersInGame.Count, monsterGridContent, monsterSlots);
        GenerateSlots(allArtifactsInGame.Count, artifactGridContent, artifactSlots);
        //GenerateSlots(100, artifactGridContent, artifactSlots);

        // --- タブボタンの設定 ---
        organTabButton?.onClick.AddListener(ShowOrganPanel);
        monsterTabButton?.onClick.AddListener(ShowMonsterPanel);
        artifactTabButton?.onClick.AddListener(ShowArtifactPanel);

        // 最初はモンスター図鑑を表示 (など、好きな初期タブに)
        ShowMonsterPanel();
    }

    // スロット生成を共通化
    private void GenerateSlots(int count, Transform content, List<GenericSlotUI> slotList)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, content);
            slotList.Add(slotGO.GetComponent<GenericSlotUI>());
        }
    }

    private void OnEnable()
    {
        GenericSlotUI.OnSlotClicked += HandleSlotClick;
    }
    private void OnDisable()
    {
        GenericSlotUI.OnSlotClicked -= HandleSlotClick;
    }

    private void HandleSlotClick(ScriptableObject clickedData)
    {
        // 未所持アイテムをクリックした場合の処理を追加（例：詳細を表示しない）
        if (clickedData == null || !IsOwned(clickedData))
        {
            selectedItem = null; // 未所持なら選択解除
            // ShowDetail(null); // 詳細パネルも非表示に
        }
        else if (selectedItem == clickedData)
        {
            selectedItem = null;
        }
        else
        {
            selectedItem = clickedData;
            // ShowDetail(selectedItem);
        }
        UpdateAllSlotColors();
    }

    // アイテムが所持済みかチェックするヘルパー関数
    private bool IsOwned(ScriptableObject data)
    {
        if (data is OrganData od) return GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(od);
        if (data is MonsterData md) return GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(md);
        if (data is ArtifactData ad) return GameManager.Instance.PlayerData.ownedArtifacts.Contains(ad);
        return false;
    }

    private void UpdateAllSlotColors()
    {
        foreach (var slot in organSlots.Concat(monsterSlots).Concat(artifactSlots))
        {
            var dataInSlot = slot.GetAssignedData();
            bool isSelected = dataInSlot != null && dataInSlot == selectedItem;
            slot.SetSelected(isSelected);
        }
    }

    private void UpdateTabColors()
    {
        organTabButton.GetComponent<Image>().color = organPanel.activeSelf ? selectedColor : deselectedColor;
        monsterTabButton.GetComponent<Image>().color = monsterPanel.activeSelf ? selectedColor : deselectedColor;
        artifactTabButton.GetComponent<Image>().color = artifactPanel.activeSelf ? selectedColor : deselectedColor;
    }

    public void ShowOrganPanel()
    {
        organPanel.SetActive(true);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(false);
        PopulateOrganGrid();
        UpdateTabColors();
    }
    public void ShowMonsterPanel()
    {
        organPanel.SetActive(false);
        monsterPanel.SetActive(true);
        artifactPanel.SetActive(false);
        PopulateMonsterGrid();
        UpdateTabColors();
    }
    public void ShowArtifactPanel()
    {
        organPanel.SetActive(false);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(true);
        PopulateArtifactGrid();
        UpdateTabColors();
    }

    private void PopulateOrganGrid()
    {
        var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        for (int i = 0; i < organSlots.Count; i++)
        {
            OrganData organ = allOrgansInGame[i];
            if (ownedOrgans.ContainsKey(organ))
            {
                organSlots[i].Setup(organ, ownedOrgans[organ]);
            }
            else
            {
                organSlots[i].SetupAsUnknown(organ);
            }
        }
    }

    private void PopulateMonsterGrid()
    {
        var ownedMonsters = GameManager.Instance.PlayerData.ownedMonsters;
        for (int i = 0; i < monsterSlots.Count; i++)
        {
            MonsterData monster = allMonstersInGame[i];
            if (ownedMonsters.ContainsKey(monster))
            {
                monsterSlots[i].Setup(monster, ownedMonsters[monster]);
            }
            else
            {
                monsterSlots[i].SetupAsUnknown(monster);
            }
        }
    }

    private void PopulateArtifactGrid()
    {
        var ownedArtifacts = GameManager.Instance.PlayerData.ownedArtifacts;
        for (int i = 0; i < artifactSlots.Count; i++)
        {
            ArtifactData artifact = allArtifactsInGame[i];
            if (ownedArtifacts.Contains(artifact))
            {
                artifactSlots[i].Setup(artifact); // Artifact用のSetupをGenericSlotUIに追加
            }
            else
            {
                artifactSlots[i].SetupAsUnknown(artifact);
            }
        }
    }
}