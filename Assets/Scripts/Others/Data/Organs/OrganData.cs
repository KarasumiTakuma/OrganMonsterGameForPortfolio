using UnityEngine;

// 臓器のカテゴリをenumで定義
public enum OrganCategory { Viscera, Muscle, Bone, Other } //内臓、筋肉、骨格、その他

[CreateAssetMenu(fileName = "NewOrganData", menuName = "Data/Organ Data")]
public class OrganData : ScriptableObject, IDisplayable
{
    [Header("基本情報")]
    [SerializeField] private int organID;
    [SerializeField] private string organName;

    [Header("ゲームロジック用")] //合成で使う
    [SerializeField] private OrganCategory category; //カテゴリー
    [SerializeField] private int rarity; // 1~5

    [Header("UI表示用")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite shadowIcon;
    [TextArea]
    [SerializeField] private string description; // 図鑑用の説明文
    [SerializeField] private string hint; // 未発見の時のヒント
    [SerializeField] private int price; // ショップで購入する時の価格

    // --- ゲッターメソッド ---
    public int GetID() => organID;
    public string GetName() => organName;
    public OrganCategory GetCategory() => category;
    public int GetRarity() => rarity;
    public Sprite GetIcon() => icon;
    public Sprite GetShadowIcon() => shadowIcon;
    public string GetDescription() => description;
    public string GetHint() => hint;
    public int GetPrice() => price;

    // --- セッターメソッド ---
    public void SetID(int newID) { organID = newID; }
    public void SetName(string newName) { organName = newName; }
    public void SetCategory(OrganCategory newCategory) { category = newCategory; }
    public void SetRarity(int newRarity) { rarity = newRarity; }
    public void SetIcon(Sprite newIcon) { icon = newIcon; }
    public void SetShadowIcon(Sprite newIcon) { shadowIcon = newIcon; }
    public void SetDescription(string newDesc) { description = newDesc; }
    public void SetHint(string newHint) { hint = newHint; }
    public void SetPrice(int newPrice) { price = newPrice; }

    // --- 外部のデータを参照するゲッター ---
    public int GetCount()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedOrgans[this];
        }
        return 0;
    }
}