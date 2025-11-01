using UnityEngine;

/// <summary>
/// 味方モンスターの基本データを保持する ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllyMonsterData", menuName = "Data/AllyMonsterData")]
public class AllyMonsterData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private int allyID;               // 味方モンスターのID（識別用）
    [SerializeField] private string allyName;          // 名前

    [Header("ステータス")]
    [SerializeField] private int maxHP;                // この味方単体の最大HP（合算対象）
    [SerializeField] private int attackPower;          // 攻撃力（今後の拡張用）

    [Header("UI表示用")]
    [TextArea]
    [SerializeField] private string description;       // キャラクター説明
    [SerializeField] private Sprite allyImage;         // 画像（UI上の見た目）

    // --- Getter群 ---
    public int GetMonsterID() => allyID;
    public string GetName() => allyName;
    public int GetMaxHP() => maxHP;
    public int GetAttackPower() => attackPower;
    public string GetDescription() => description;
    public Sprite GetImage() => allyImage;
}
