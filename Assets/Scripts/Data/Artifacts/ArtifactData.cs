using UnityEngine;

// アーティファクトの効果がどのタイミングで発動するかなどを管理するenum（必要に応じて拡張）
public enum ArtifactEffectTrigger { OnBattleStart, OnTurnStart, OnCardPlay }

[CreateAssetMenu(fileName = "NewArtifactData", menuName = "Data/Artifact Data")]
public class ArtifactData : ScriptableObject
{
    [Header("アーティファクトの基本情報")]
    [SerializeField] private int artifactID;
    [SerializeField] private string artifactName;
    [SerializeField] private int artifactRarity;
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite shadowIcon;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private string hint;

    [Header("ゲームロジック用")]
    [SerializeField] private ArtifactEffectTrigger trigger; // 効果の発動タイミング
    [SerializeField] private int value; // 効果の量（例: +1, +5%など

    // --- ゲッターメソッド ---
    public int GetArtifactID() => artifactID;
    public string GetArtifactName() => artifactName;
    public int GetArtifactRarity() => artifactRarity;
    public Sprite GetIcon() => icon;
    public Sprite GetShadowIcon() => shadowIcon;
    public string GetDescription() => description;
    public string GetHint() => hint;
    public ArtifactEffectTrigger GetTrigger() => trigger;
    public int GetValue() => value;

    // --- セッターメソッド ---
    public void SetArtifactID(int newID) { artifactID = newID; }
    public void SetArtifactName(string newName) { artifactName = newName; }
    public void SetIcon(Sprite newIcon) { icon = newIcon; }
    public void SetShadowIcon(Sprite newIcon) { shadowIcon = newIcon; }
    public void SetDescription(string newDesc) { description = newDesc; }
    public void SetHint(string newHint) { hint = newHint; }
    public void SetTrigger(ArtifactEffectTrigger newTrigger) { trigger = newTrigger; }
    public void SetValue(int newValue) { value = newValue; }

    public int GetCount() 
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.ownedArtifacts.ContainsKey(this))
        {
            return GameManager.Instance.PlayerData.ownedArtifacts[this];
        }
        return 0;
    }
}