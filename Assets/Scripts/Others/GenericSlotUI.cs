using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// インベントリのスロット一つ分のUI表示とクリックイベントの発行を担当する、汎用的なクラス。
/// </summary>
public class GenericSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image icon;
    public Sprite defaultUnknownIcon; // 共通の「？」アイコン
    public TextMeshProUGUI countText;
    private Button button;
    public Image background;
   
    // このスロットに何が表示されているかを記憶する
    private ScriptableObject assignedData;

    // このスロットがクリックされたことを外部に通知するイベント
    public static event Action<ScriptableObject> OnSlotClicked;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) button = gameObject.AddComponent<Button>();
        // クリックされたら、自分が記憶しているデータをイベントで通知する
        button.onClick.AddListener(HandleClick);
        SetSelected(false); // 初期色は非表示（透明）にする
    }

    // ボタンがクリックされた時に呼ばれる
    private void HandleClick()
    {
        if (assignedData != null)
        {
            // isSelected = !isSelected;
            // SetSelected(isSelected);
            // クリックされたことを、自分自身の参照を添えて通知する
            OnSlotClicked?.Invoke(assignedData);

        }
    }

    // 自分が何のデータを担当しているか外部に教える
    public ScriptableObject GetAssignedData()
    {
        return assignedData;
    }

    // データを受け取ってスロットの見た目を設定する
    public void SetSelected(bool isSelected)
    {
        if (background != null)
        {
            background.color = isSelected ? new Color32(78, 78, 255, 100) : new Color(1, 1, 1, 0);
        }
    }

    /// <summary>
    /// OrganDataを受け取って、スロットの見た目を設定する
    /// </summary>
    public void Setup(OrganData data, int count)
    {
        assignedData = data;
        icon.enabled = true;
        icon.sprite = data.GetIcon();
        countText.text = count.ToString();
    }
    
    /// <summary>
    /// MonsterDataを受け取って、スロットの見た目を設定する（メソッドのオーバーロード）
    /// </summary>
    public void Setup(MonsterData data, int count)
    {
        assignedData = data;
        icon.enabled = true;
        icon.sprite = data.GetIcon();
        countText.text = count.ToString();
    }

    /// <summary>
    /// ArtifactDataを受け取って、スロットの見た目を設定する
    /// </summary>
    public void Setup(ArtifactData data, int count)
    {
        assignedData = data;
        icon.enabled = true;
        icon.sprite = data.GetIcon();
        countText.text = count.ToString();
    }

    /// <summary>
    /// スロットを空の状態にする
    /// </summary>
    public void Clear()
    {
        assignedData = null;
        icon.enabled = false;
        icon.sprite = null;
        countText.text = "";
    }

    /// <summary>
    /// 未所持アイテムとしてスロットを設定する
    /// </summary>
    public void SetupAsUnknown(ScriptableObject data)
    {
        assignedData = data;
        icon.enabled = true;
        countText.text = ""; // 個数は表示しない

        // 影画像を表示するロジック
        if (data is MonsterData monsterData && monsterData.GetShadowIcon() != null)
        {
            icon.sprite = monsterData.GetShadowIcon();
        }
        else if (data is OrganData organData && organData.GetShadowIcon() != null)
        {
            icon.sprite = organData.GetIcon();
        }
        else if (data is OrganData artifactData && artifactData.GetShadowIcon() != null)
        {
            icon.sprite = artifactData.GetShadowIcon();
        }
        else
        {
            // 共通の「？」画像を使う場合
            icon.sprite = defaultUnknownIcon;
            //icon.sprite = null; // または単に非表示
            //icon.enabled = false;
        }

        // 名前を「？？？」にする
        // nameText.text = "？？？";
    }
}