using UnityEngine;

// アーティファクトの効果がどのタイミングで発動するかなどを管理するenum（必要に応じて拡張）
public enum ArtifactEffectTrigger { OnBattleStart, OnTurnStart, OnCardPlay }

[CreateAssetMenu(fileName = "NewArtifactData", menuName = "Data/Artifact Data")]
public class ArtifactData : ScriptableObject
{
    [Header("アーティファクトの基本情報")]
    public int artifactID;
    public string artifactName;
    public Sprite icon;
    public Sprite shadowIcon;
    [TextArea]
    public string description;

    [Header("ゲームロジック用")]
    public ArtifactEffectTrigger trigger; // 効果の発動タイミング
    public int value; // 効果の量（例: +1, +5%など）
}