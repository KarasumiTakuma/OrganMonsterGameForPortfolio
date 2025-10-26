using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DetailSystem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject detailPanel; // 詳細パネル全体
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI objectName;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI ownedCountText;
    [SerializeField] private Image typeIcon;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private ScriptableObject currentSelectedItem;

    [Header("Icon Settings")]
    [Tooltip("OrganCategoryのenumの順番に合わせて設定 (Viscera, Muscle, Bone, Other)")]
    [SerializeField] private List<Sprite> organCategoryIcons;
    [Tooltip("MonsterTypeのenumの順番に合わせて設定 (Fire, Water, Grass, Other)")]
    [SerializeField] private List<Sprite> monsterTypeIcons;

    private void OnEnable()
    {
        GenericSlotUI.OnSlotClicked += ShowDetail;
        // 最初は非表示にしておく
        if (detailPanel != null) detailPanel.SetActive(false);
    }
    private void OnDisable()
    {
        GenericSlotUI.OnSlotClicked -= ShowDetail;
    }

    /// <summary>
    /// 詳細パネルの表示を更新する
    /// </summary>
    private void ShowDetail(ScriptableObject data)
    {
        // データがnullならパネルを非表示にして終了
        if (data == null)
        {
            if (detailPanel != null) detailPanel.SetActive(false);
            return;
        }
        // クリックされたのが、既に選択中のアイテムの場合
        if (data == currentSelectedItem)
        {
            // パネルを非表示にし、選択を解除して終了
            detailPanel.SetActive(false);
            currentSelectedItem = null;
            return;
        }
        currentSelectedItem = data; // 新しく選択されたアイテムを記憶

        if (detailPanel != null) detailPanel.SetActive(true);

        // 受け取ったデータがOrganData型かどうかをチェック
        if (data is OrganData organData)
        {
            icon.sprite = organData.icon;
            objectName.text = organData.organName;
            rarityText.text = organData.rarity.ToString();
            ownedCountText.text = organData.GetCount().ToString();
            descriptionText.text = organData.description;

            // カテゴリーに応じてアイコンを設定
            int categoryIndex = (int)organData.category;
            if (organCategoryIcons != null && organCategoryIcons.Count > categoryIndex)
            {
                typeIcon.enabled = true;
                typeIcon.sprite = organCategoryIcons[categoryIndex];
            }
            else
            {
                typeIcon.enabled = false;
            }
        }
        // 受け取ったデータがMonsterData型かどうかをチェック
        else if (data is MonsterData monsterData)
        {
            icon.sprite = monsterData.GetIcon();
            objectName.text = monsterData.GetName();
            rarityText.text = monsterData.GetRarity().ToString();
            ownedCountText.text = monsterData.GetCount().ToString();
            descriptionText.text = monsterData.GetDescription;

            // タイプに応じてアイコンを設定
            int typeIndex = (int)monsterData.type;
            if (monsterTypeIcons != null && monsterTypeIcons.Count > typeIndex)
            {
                typeIcon.enabled = true;
                typeIcon.sprite = monsterTypeIcons[typeIndex];
            }
            else
            {
                typeIcon.enabled = false;
            }
        }
    }
}