using UnityEngine;
using UnityEngine.UI;
using System;
//using TMPro;

/// <summary>
/// インベントリスロット一つ分のUI表示と、クリックイベントの発行を担当するクラス。
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    //public TextMeshProUGUI countText;
    public Text countText;
    private Button button;
    public Image background;
    private bool isSelected = false; // 自分が選択されているかを記憶する

    // このスロットが担当する臓器データ
    private OrganData assignedOrganData;

    // このスロットがクリックされたときに外部に通知するためのイベント
    public static event Action<OrganData, bool> OnSelectionChanged;

    private void Awake()
    {
        button = GetComponent<Button>();
        // ボタンがなければ追加する（安全対策）
        if (button == null) { button = gameObject.AddComponent<Button>(); }
        button.onClick.AddListener(HandleClick);
        SetSelected(false); // 初期色は非選択状態
    }

    // ボタンがクリックされたときに呼ばれる
    private void HandleClick()
    {
        if (assignedOrganData != null)
        {
            // 1. 選択状態を反転させる
            isSelected = !isSelected;
            // 2. 自分の色を更新する
            // SetSelected(isSelected);
            // 3. 自分の新しい状態を、外部に通知する
            OnSelectionChanged?.Invoke(assignedOrganData, isSelected);
        }
    }

    // 自分が担当している臓器データを外部に教えるための関数
    public OrganData GetAssignedOrgan()
    {
        return assignedOrganData;
    }
    // 選択状態に応じて、背景色を変更するメソッド
    public void SetSelected(bool isSelected)
    {
        if (background != null)
        {
            background.color = isSelected ? new Color32(78, 78, 255, 100) : new Color(1, 1, 1, 0);
        }
    }

    // データを受け取ってスロットの見た目を設定する
    public void Setup(OrganData organ, int count)
    {
        assignedOrganData = organ; // 担当データを記憶
        icon.enabled = true;
        icon.sprite = organ.GetIcon();
        countText.text = count.ToString();
        // 新しくセットアップされた時は、必ず非選択状態に戻す
        isSelected = false; 
        SetSelected(false);
    }

    // スロットを空の状態にする
    public void ClearSlot()
    {
        assignedOrganData = null;
        icon.enabled = false; // アイコンを非表示にする
        countText.text = "";
        isSelected = false;
        SetSelected(false);
    }
    
    /// <summary>
    /// スロットの有効/無効を切り替える
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        // ボタンのクリックを有効/無効化
        if (button != null)
        {
            button.interactable = interactable;
        }

        // 見た目を半透明にして「選べない」ことを伝える
        if (icon != null)
        {
            Color c = icon.color;
            c.a = interactable ? 1.0f : 0.3f; // 無効なら透明度を30%にする
            icon.color = c;
        }
    }

}