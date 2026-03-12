using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// 図鑑シーンのシステムを構成するクラス
/// </summary>
public class HistorySystem : BaseTabbedGridSystem
{
    // --- 全アイテムリスト ---
    private List<OrganData> allOrgansInGame = new List<OrganData>(); // 素材全種類
    private List<MonsterData> allMonstersInGame = new List<MonsterData>(); // モンスター全種類
    private List<ArtifactData> allArtifactsInGame = new List<ArtifactData>();// アーティファクト全種類

    protected override void Start()
    {
        base.Start();

        // --- ゲーム内の全アイテムデータをロード ---
        // --- DataManagerからロード済みのリストを取得 ---
        allOrgansInGame = DataManager.Instance.AllOrgans;
        allMonstersInGame = DataManager.Instance.AllMonsters;
        allArtifactsInGame = DataManager.Instance.AllArtifacts;

        // --- 事前に空のスロットを生成 ---
        GenerateSlots(allOrgansInGame.Count, organGridContent, organSlots);
        GenerateSlots(allMonstersInGame.Count, monsterGridContent, monsterSlots);
        GenerateSlots(allArtifactsInGame.Count, artifactGridContent, artifactSlots);

        // 最初はモンスター図鑑を表示
        ShowMonsterPanel();
    }

    // アイテムが所持済みかチェックするヘルパー関数
    private bool IsOwned(ScriptableObject data)
    {
        if (data is OrganData organData) return GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(organData);
        if (data is MonsterData monsterData) return GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(monsterData);
        if (data is ArtifactData artifactData) return GameManager.Instance.PlayerData.ownedArtifacts.ContainsKey(artifactData);
        return false;
    }

    protected override void PopulateOrganGrid()
    {
        // PlayerDataから「発見済み」の臓器リストを取得
        var discoveredOrgans = GameManager.Instance.PlayerData.discoveredOrgans;
        //var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        for (int i = 0; i < organSlots.Count; i++)
        {
            OrganData organ = allOrgansInGame[i];
            if (discoveredOrgans.Contains(organ))
            {
                // 発見済み -> 通常表示 (所持数はPlayerDataから取得)
                int count = GameManager.Instance.PlayerData.ownedOrgans.ContainsKey(organ) ? GameManager.Instance.PlayerData.ownedOrgans[organ] : 0;
                organSlots[i].Setup(organ, count);
            }
            else
            {
                // 未発見 -> 影表示
                organSlots[i].SetupAsUnknown(organ);
            }
        }
    }

    protected override void PopulateMonsterGrid()
    {
        // PlayerDataから「発見済み」のモンスターリストを取得
        var discoveredMonsters = GameManager.Instance.PlayerData.discoveredMonsters;
        
        for (int i = 0; i < monsterSlots.Count; i++)
        {
            MonsterData monster = allMonstersInGame[i]; // 全モンスターを順番にチェック

            if (discoveredMonsters.Contains(monster))
            {
                // 発見済み -> 通常表示
                int count = GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(monster) ? GameManager.Instance.PlayerData.ownedMonsters[monster] : 0;
                monsterSlots[i].Setup(monster, count);
            }
            else
            {
                // 未発見 -> 影表示
                monsterSlots[i].SetupAsUnknown(monster);
            }
        }
    }

    protected override void PopulateArtifactGrid()
    {
        // PlayerDataから「発見済み」のモンスターリストを取得
        var discoveredArtifacts = GameManager.Instance.PlayerData.discoveredArtifacts;
        
        for (int i = 0; i < artifactSlots.Count; i++)
        {
            ArtifactData artifact = allArtifactsInGame[i];

            if (discoveredArtifacts.Contains(artifact))
            {
                // 発見済み -> 通常表示
                int count = GameManager.Instance.PlayerData.ownedArtifacts.ContainsKey(artifact) ? GameManager.Instance.PlayerData.ownedArtifacts[artifact] : 0;
                artifactSlots[i].Setup(artifact, count);
            }
            else
            {
                // 未発見 -> 影表示
                artifactSlots[i].SetupAsUnknown(artifact);
            }
        }
    }
}