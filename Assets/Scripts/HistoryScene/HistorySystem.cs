using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// 図鑑シーンのシステムを構成するクラス
/// </summary>
public class HistorySystem : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject organPanel; // 素材配置パネル
    [SerializeField] private GameObject monsterPanel; // モンスター配置パネル
    [SerializeField] private GameObject artifactPanel; // アーティファクト配置パネル

    [Header("Grid Contents")]
    // 素材を配置するためのスクロール領域
    [SerializeField] private Transform organGridContent;
    // モンスターを配置するためのスクロール領域
    [SerializeField] private Transform monsterGridContent;
    // アーティファクトを配置するためのスクロール領域
    [SerializeField] private Transform artifactGridContent;

    [Header("Tab Buttons")]
    [SerializeField] private Button organTabButton;
    [SerializeField] private Button monsterTabButton;
    [SerializeField] private Button artifactTabButton;

    [Header("Prefabs")]
    // 配置するスロットプハブ
    [SerializeField] private GameObject genericSlotPrefab;

    [Header("Tab Colors")]
    // 選択中の色
    [SerializeField] private Color selectedColor = Color.yellow;
    // 非選択中の色
    [SerializeField] private Color deselectedColor = Color.white;

    // --- 全アイテムリスト ---
    private List<OrganData> allOrgansInGame = new List<OrganData>(); // 素材全種類
    private List<MonsterData> allMonstersInGame = new List<MonsterData>(); // モンスター全種類
    private List<ArtifactData> allArtifactsInGame = new List<ArtifactData>();// アーティファクト全種類

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

        // 最初はモンスター図鑑を表示
        ShowMonsterPanel();
    }

    // スロット生成を共通化
    private void GenerateSlots(int count, Transform content, List<GenericSlotUI> slotList)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, content);
            // スクリプトをリストに追加
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
        // 未入手アイテムをクリックした場合の処理を追加
        if (clickedData == null)
        {
            selectedItem = null; // 未所持なら選択解除
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
        if (data is OrganData organData) return GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(organData);
        if (data is MonsterData monsterData) return GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(monsterData);
        if (data is ArtifactData artifactData) return GameManager.Instance.PlayerData.ownedArtifacts.ContainsKey(artifactData);
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
        // PlayerDataから「発見済み」の臓器リストを取得
        var discoveredOrgans = GameManager.Instance.PlayerData.discoveredOrgans;
        //var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        for (int i = 0; i < organSlots.Count; i++)
        {
            OrganData organ = allOrgansInGame[i];
            if (discoveredOrgans.Contains(organ))
            {
                // 発見済み -> 通常表示 (所持数はPlayerDataから取得)
                int count = GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(organ) ? GameManager.Instance.PlayerData.ownedOrgans[organ] : 0;
                organSlots[i].Setup(organ, count);
            }
            else
            {
                // 未発見 -> 影表示
                organSlots[i].SetupAsUnknown(organ);
            }
        }
    }

    private void PopulateMonsterGrid()
    {
        // PlayerDataから「発見済み」のモンスターリストを取得
        var discoveredMonsters = GameManager.Instance.PlayerData.discoveredMonsters;
        
        for (int i = 0; i < monsterSlots.Count; i++)
        {
            MonsterData monster = allMonstersInGame[i]; // 全モンスターを順番にチェック

            if (discoveredMonsters.Contains(monster))
            {
                // 発見済み -> 通常表示
                int count = GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(monster) ? GameManager.Instance.PlayerData.ownedMonsters[monster] : 0;
                monsterSlots[i].Setup(monster, count);
            }
            else
            {
                // 未発見 -> 影表示
                monsterSlots[i].SetupAsUnknown(monster);
            }
        }
    }

    private void PopulateArtifactGrid()
    {
        // PlayerDataから「発見済み」のモンスターリストを取得
        var discoveredArtifacts = GameManager.Instance.PlayerData.discoveredArtifacts;
        
        for (int i = 0; i < artifactSlots.Count; i++)
        {
            ArtifactData artifact = allArtifactsInGame[i];

            if (discoveredArtifacts.Contains(artifact))
            {
                // 発見済み -> 通常表示
                int count = GameManager.Instance.PlayerData.ownedArtifacts.ContainsKey(artifact) ? GameManager.Instance.PlayerData.ownedArtifacts[artifact] : 0;
                artifactSlots[i].Setup(artifact, count);
            }
            else
            {
                // 未発見 -> 影表示
                artifactSlots[i].SetupAsUnknown(artifact);
            }
        }
    }
}