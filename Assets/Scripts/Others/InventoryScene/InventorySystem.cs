using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;

/// <summary>
/// インベントリシーンのシステムを構成するクラス
/// </summary>
public class InventorySystem : BaseTabbedGridSystem
{
    [SerializeField] private int maxSlots;
    protected override void Start()
    {
        // 親クラスのStart()を実行
        base.Start();

        GenerateSlots(maxSlots, organGridContent, organSlots);
        GenerateSlots(maxSlots, monsterGridContent, monsterSlots);
        GenerateSlots(maxSlots, artifactGridContent, artifactSlots);

        // 最初はモンスターパネルを表示
        ShowMonsterPanel();

        // PlayerDataの変更イベントを購読
        PlayerData.OnInventoryChanged += RefreshCurrentView;
    }

    private void OnDestroy() // Startで登録した場合はOnDestroyで解除
    {
        // イベント解除（エラー防止）
        PlayerData.OnInventoryChanged -= RefreshCurrentView;
    }

    // 現在開いているタブだけ再描画する
    private void RefreshCurrentView()
    {
        if (organPanel.activeSelf) PopulateOrganGrid();
        else if (monsterPanel.activeSelf) PopulateMonsterGrid();
        else if (artifactPanel.activeSelf) PopulateArtifactGrid();
    }

    // 臓器グリッドにデータを表示
    protected override void PopulateOrganGrid()
    {
        // PlayerDataから臓器リストを取得し、ID順でソート
        var ownedOrgans = GameManager.Instance.PlayerData.ownedOrgans;
        List<OrganData> sortedOrganKeys = ownedOrgans.Keys.OrderBy(k => k.GetID()).ToList();

        // 全スロットをループして、表示を更新
        for (int i = 0; i < organSlots.Count; i++)
        {
            if (i < sortedOrganKeys.Count)
            {
                OrganData organ = sortedOrganKeys[i];
                int count = ownedOrgans[organ];
                organSlots[i].gameObject.SetActive(true); // スロットを表示
                organSlots[i].Setup(organ, count);
            }
            else
            {
                organSlots[i].Clear();
                //organSlots[i].gameObject.SetActive(false); // 不要なスロットは非表示
            }
        }
    }

    // モンスターグリッドにデータを表示
    protected override void PopulateMonsterGrid()
    {
        var ownedMonsters = GameManager.Instance.PlayerData.ownedMonsters;
        List<MonsterData> sortedMonsterKeys = ownedMonsters.Keys.OrderBy(k => k.GetID()).ToList();

        for (int i = 0; i < monsterSlots.Count; i++)
        {
            if (i < sortedMonsterKeys.Count)
            {
                MonsterData monster = sortedMonsterKeys[i];
                int count = ownedMonsters[monster];
                monsterSlots[i].gameObject.SetActive(true);
                monsterSlots[i].Setup(monster, count);
            }
            else
            {
                monsterSlots[i].Clear();
                //monsterSlots[i].gameObject.SetActive(false);
            }
        }
    }

    protected override void PopulateArtifactGrid()
    {
        var ownedArtifacts = GameManager.Instance.PlayerData.ownedArtifacts;
        List<ArtifactData> sortedArtifacts = ownedArtifacts.Keys.OrderBy(k => k.GetArtifactID()).ToList();

        for (int i = 0; i < artifactSlots.Count; i++)
        {
            if (i < sortedArtifacts.Count)
            {
                ArtifactData artifact = sortedArtifacts[i];
                int count = ownedArtifacts[artifact];
                artifactSlots[i].gameObject.SetActive(true);
                artifactSlots[i].Setup(artifact, count);
            }
            else
            {
                artifactSlots[i].Clear();
                //monsterSlots[i].gameObject.SetActive(false);
            }
        }
    }
}