using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// InventorySystemとHistorySystemの
/// 共通の機能を持つ親クラス
/// </summary>
public abstract class BaseTabbedGridSystem : MonoBehaviour
{
    [Header("UI Panels")]
    // 素材配置パネル
    [SerializeField] protected GameObject organPanel;
    // モンスター配置パネル
    [SerializeField] protected GameObject monsterPanel;
    // アーティファクト配置パネル
    [SerializeField] protected GameObject artifactPanel;

    [Header("Grid Contents")]
    // 素材を配置するためのスクロール領域
    [SerializeField] protected Transform organGridContent;
    // モンスターを配置するためのスクロール領域
    [SerializeField] protected Transform monsterGridContent;
    // アーティファクトを配置するためのスクロール領域
    [SerializeField] protected Transform artifactGridContent;

    [Header("Tab Buttons")]
    [SerializeField] protected Button organTabButton;
    [SerializeField] protected Button monsterTabButton;
    [SerializeField] protected Button artifactTabButton;

    [Header("Prefabs")]
    // 配置するスロットプハブ
    [SerializeField] protected GameObject genericSlotPrefab;

    [Header("Tab Colors")]
    // 選択中の色
    [SerializeField] protected Color selectedColor = Color.yellow;
    // 非選択中の色
    [SerializeField] protected Color deselectedColor = Color.white;

    // --- 事前生成したスロット ---
    protected List<GenericSlotUI> organSlots = new List<GenericSlotUI>();
    protected List<GenericSlotUI> monsterSlots = new List<GenericSlotUI>();
    protected List<GenericSlotUI> artifactSlots = new List<GenericSlotUI>();

    // 現在選択されているアイテム
    protected ScriptableObject selectedItem;

    protected virtual void Start()
    {
        // --- タブボタンの設定 ---
        organTabButton?.onClick.AddListener(ShowOrganPanel);
        monsterTabButton?.onClick.AddListener(ShowMonsterPanel);
        artifactTabButton?.onClick.AddListener(ShowArtifactPanel);
    }

    // スロット生成を共通化
    protected void GenerateSlots(int count, Transform content, List<GenericSlotUI> slotList)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = Instantiate(genericSlotPrefab, content);
            // スクリプトをリストに追加
            slotList.Add(slotGO.GetComponent<GenericSlotUI>());
        }
    }

    protected void OnEnable()
    {
        GenericSlotUI.OnSlotClicked += HandleSlotClick;
    }
    protected void OnDisable()
    {
        GenericSlotUI.OnSlotClicked -= HandleSlotClick;
    }

    protected void HandleSlotClick(ScriptableObject clickedData)
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

    protected void UpdateAllSlotColors()
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

    protected void ShowOrganPanel()
    {
        organPanel.SetActive(true);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(false);
        PopulateOrganGrid();
        UpdateTabColors();
    }

    protected void ShowMonsterPanel()
    {
        organPanel.SetActive(false);
        monsterPanel.SetActive(true);
        artifactPanel.SetActive(false);
        PopulateMonsterGrid();
        UpdateTabColors();
    }

    protected void ShowArtifactPanel()
    {
        organPanel.SetActive(false);
        monsterPanel.SetActive(false);
        artifactPanel.SetActive(true);
        PopulateArtifactGrid();
        UpdateTabColors();
    }

    // --- 子クラスが実装すべき「抽象メソッド」 ---
    /// <summary>
    /// InventorySystemでは所持しているデータを表示
    /// HistorySystemでは発見済みを通常表示、未発見を影表示する
    /// </summary>
    protected abstract void PopulateOrganGrid();
    protected abstract void PopulateMonsterGrid();
    protected abstract void PopulateArtifactGrid();

}
