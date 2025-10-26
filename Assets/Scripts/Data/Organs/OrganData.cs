using UnityEngine;

// 臓器のカテゴリをenumで定義
public enum OrganCategory { Viscera, Muscle, Bone, Other } //内臓、筋肉、骨格、その他

[CreateAssetMenu(fileName = "NewOrganData", menuName = "Data/Organ Data")]
public class OrganData : ScriptableObject, IDisplayable
{
    [Header("基本情報")]
    public int organID;
    public string organName;

    [Header("ゲームロジック用")] //合成で使う
    public OrganCategory category; //カテゴリー
    public int rarity; // 1~5

    [Header("UI表示用")]
    public Sprite icon;
    public Sprite shadowIcon;
    [TextArea]
    public string description; // 図鑑用の説明文
    public int price; // ショップで購入する時の価格

    public Sprite GetIcon() => icon;
    public string GetName() => organName;
    // PlayerDataインスタンスからデータの数を返すメソッド
    public int GetCount()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedOrgans[this];
        }
        return 0;
    }
}