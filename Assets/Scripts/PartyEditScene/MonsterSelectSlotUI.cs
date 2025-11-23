using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 入れ替え候補として並ぶモンスター１体分を管理するクラス
/// </summary>
public class MonsterSelectSlotUI : MonoBehaviour
{
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button selectButton;
    // 「編成中」であることを示すカバー
    [SerializeField] private GameObject selectedCover;

    private MonsterData monsterData;
    // このスロットが選択されたときに、対応する MonsterData を通知するイベント
    public event Action<MonsterData> OnSelected;

    void Start()
    {
        selectButton.onClick.AddListener(() => OnSelected?.Invoke(monsterData));
    }
    public void Setup(MonsterData monster, bool isSeledted)
    {
        monsterData = monster;
        monsterIcon.sprite = monster.GetIcon();
        nameText.text = monster.GetName();
        // 選択済みの場合はボタンを無効化、未選択の場合は有効化
        if (isSeledted)
        {
            selectButton.interactable = false;
            if (selectedCover) selectedCover.SetActive(true);
        }
        else
        {
            selectButton.interactable = true;
            if (selectedCover) selectedCover.SetActive(false);
        }
    }
}

