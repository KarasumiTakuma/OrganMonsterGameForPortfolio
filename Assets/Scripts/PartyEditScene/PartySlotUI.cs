using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
/// <summary>
/// パーティ編成画面のひとつのスロットUIを管理するクラス
/// </summary>
public class PartySlotUI : MonoBehaviour
{　　 
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI monsterName;
    [SerializeField] private Button selectButton;
    [SerializeField] private int slotIndex; // 0, 1, 2 のどれか

    // 空のアイコン
    [SerializeField] private Sprite emptyIcon;

    // クリックされたことを知らせるイベント
    public event Action<int> OnClick;

    void Start()
    {
        selectButton.onClick.AddListener(() => OnClick?.Invoke(slotIndex));
        selectButton.onClick.AddListener(() => Debug.Log(slotIndex));
    }

    // データを表示する
    public void Setup(MonsterData monster)
    {
        if (monster != null)
        {
            monsterIcon.gameObject.SetActive(true);
            monsterIcon.sprite = monster.GetIcon();
            monsterName.text = monster.GetName();
        }
        else
        {
            monsterIcon.gameObject.SetActive(true);
            monsterIcon.sprite = emptyIcon;
            monsterName.text = "Empty";
        }
    }
}