using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // ロード済みの全データを保持
    [HideInInspector]
    public List<OrganData> AllOrgans { get; private set; }
    [HideInInspector]
    public List<MonsterData> AllMonsters { get; private set; }
    [HideInInspector]
    public List<ArtifactData> AllArtifacts { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllGameData(); // 起動時に全データをロード
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Resourcesフォルダから全データをロードする
    private void LoadAllGameData()
    {
        // ★★★ Resources.LoadAll を使う ★★★
        AllOrgans = Resources.LoadAll<OrganData>("Data/Organs").OrderBy(o => o.organID).ToList();
        AllMonsters = Resources.LoadAll<MonsterData>("Data/Monsters").OrderBy(m => m.monsterID).ToList();
        AllArtifacts = Resources.LoadAll<ArtifactData>("Data/Artifacts").OrderBy(a => a.artifactID).ToList();
    }
}