using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// プレイヤーのデータを管理するクラス
/// </summary>
public class PlayerData : MonoBehaviour
{
    [Header("プレイヤーの情報")]
    [SerializeField] private int researchPoints; // 研究ポイント

    // --- データ変更を通知するためのイベント ---
    public static event Action OnInventoryChanged;

    // 所持している臓器とその数
    public Dictionary<OrganData, int> ownedOrgans = new Dictionary<OrganData, int>();
    // 所持しているモンスター
    public Dictionary<MonsterData, int> ownedMonsters = new Dictionary<MonsterData, int>();
    // public List<MonsterData> unlockedMonsters = new List<MonsterData>();
    // 所持しているアーティファクト
    public Dictionary<ArtifactData, int> ownedArtifacts = new Dictionary<ArtifactData, int>();
    // 現在編成中のパーティ（最大3体）
    private List<MonsterData> currentParty = new List<MonsterData>();
    // 読み取り専用のプロパティ（外からは見れるけど書き換えられない）
    public IReadOnlyList<MonsterData> CurrentParty => currentParty;

    [Header("図鑑用の発見済みリスト")]
    public List<MonsterData> discoveredMonsters = new List<MonsterData>();
    public List<OrganData> discoveredOrgans = new List<OrganData>();
    public List<ArtifactData> discoveredArtifacts = new List<ArtifactData>();

    // --- デバック用 ---
    [Header("デバッグ用インベントリ表示")]
    [SerializeField] private List<OrganData> organKeys = new List<OrganData>();
    [SerializeField] private List<int> organValues = new List<int>();
    [SerializeField] private List<MonsterData> monsterKeys = new List<MonsterData>();
    [SerializeField] private List<int> monsterValues = new List<int>();

    // --- インスペクターの表示を更新するためのメソッドを追加 ---
    // Updateはゲーム実行中に毎フレーム呼び出される
    private void Update()
    {
        // エディタで再生中のみ、デバッグリストを更新する（パフォーマンスのため）
        #if UNITY_EDITOR
        UpdateDebugLists();
        #endif
    }

    // OnValidateからも呼び出して、非再生中の編集にも対応
    private void OnValidate()
    {
        // すぐに更新するとパフォーマンスに影響する場合があるため、
        // 念のためエディタの更新ループで一度だけ呼ばれるようにする
        UnityEditor.EditorApplication.delayCall += UpdateDebugLists;
    }

    // 更新処理を一つのメソッドにまとめる
    private void UpdateDebugLists()
    {
        // nullチェックを追加して、エディタでのエラーを防ぐ
        if (ownedOrgans == null || ownedMonsters == null) return;
        
        organKeys = ownedOrgans.Keys.ToList();
        organValues = ownedOrgans.Values.ToList();
        monsterKeys = ownedMonsters.Keys.ToList();
        monsterValues = ownedMonsters.Values.ToList();
    }

    private void Awake()
    {
        // パーティメンバーリストの初期化
        if (currentParty.Count < 3)
        {
            // 必要な数になるまでnullを追加
            while (currentParty.Count < 3)
            {
                currentParty.Add(null);
            }
        }
    }

    // --- データ操作用の関数  ---

    public int GetPoints() => researchPoints;

    public void AddPoints(int amount)
    {
        researchPoints += amount;
    }

    // ガチャなどでポイントが足りるか確認して足りると消費、足りないとfalseを返す
    public bool UsePoints(int amount)
    {
        if (researchPoints >= amount)
        {
            researchPoints -= amount;
            return true; // 消費成功
        }
        return false; // ポイント不足
    }

    public void AddOrgan(OrganData organ, int amount)
    {
        if (ownedOrgans.ContainsKey(organ))
        {
            ownedOrgans[organ] += amount;
        }
        else
        {
            ownedOrgans.Add(organ, amount);
        }
        // もし、まだ発見済みリストになければ追加する
        if (!discoveredOrgans.Contains(organ))
        {
            discoveredOrgans.Add(organ);
        }
        // イベント発行
        OnInventoryChanged?.Invoke();
    }
    // 臓器を削除するメソッド
    public void RemoveOrgan(OrganData organ)
    {
        if (ownedOrgans.ContainsKey(organ))
        {
            ownedOrgans[organ]--;
            if (ownedOrgans[organ] <= 0)
            {
                ownedOrgans.Remove(organ);
            }
            // イベント発行
            OnInventoryChanged?.Invoke();
        }    
            
    }

    public void AddMonster(MonsterData monster, int amount)
    {
        if (ownedMonsters.ContainsKey(monster))
        {
            ownedMonsters[monster] += amount;
        }
        else
        {
            ownedMonsters.Add(monster, amount);
        }
        // もし、まだ発見済みリストになければ追加する
        if (!discoveredMonsters.Contains(monster))
        {
            discoveredMonsters.Add(monster);
        }
        // イベント発行
        OnInventoryChanged?.Invoke();
    }

    public void RemoveMonster(MonsterData monster)
    {
        if (ownedMonsters.ContainsKey(monster))
        {
            ownedMonsters[monster]--;
            if (ownedMonsters[monster] <= 0)
            {
                ownedMonsters.Remove(monster);
            }
            // イベント発行
            OnInventoryChanged?.Invoke();
        }
    }

    public void AddArtifact(ArtifactData artifact, int amount)
    {
        if (ownedArtifacts.ContainsKey(artifact))
        {
            ownedArtifacts[artifact] += amount;
        }
        else
        {
            ownedArtifacts.Add(artifact, amount);
        }
        // もし、まだ発見済みリストになければ追加する
        if (!discoveredArtifacts.Contains(artifact))
        {
            discoveredArtifacts.Add(artifact);
        }
        // イベント発行
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 編成画面から呼び出される、パーティ編成を設定するメソッド
    /// </summary>
    /// <param name="newParty"></param>
    public void SetPartyMember(int slotIndex, MonsterData newMonster)
    {
        currentParty[slotIndex - 1] = newMonster;
    }

    /// --- セーブ・ロード用メソッド ---
    public SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        saveData.researchPoints = this.researchPoints;
        saveData.discoveredMonsters = this.discoveredMonsters;
        saveData.discoveredOrgans = this.discoveredOrgans;
        saveData.discoveredArtifacts = this.discoveredArtifacts;
        saveData.currentParty = this.currentParty;

        // DictionaryをList<OrganSaveData>に変換
        saveData.ownedOrgans = new List<OrganSaveData>();
        foreach (var entry in ownedOrgans)
        {
            saveData.ownedOrgans.Add(new OrganSaveData { organ = entry.Key, count = entry.Value });
        }

        // DictionaryをList<MonsterSaveData>に変換
        saveData.ownedMonsters = new List<MonsterSaveData>();
        foreach (var entry in ownedMonsters)
        {
            saveData.ownedMonsters.Add(new MonsterSaveData { monster = entry.Key, count = entry.Value });
        }

        // DictionaryをList<ArtifactSaveData>に変換
        saveData.ownedArtifacts = new List<ArtifactSaveData>();
        foreach (var entry in ownedArtifacts)
        {
            saveData.ownedArtifacts.Add(new ArtifactSaveData { artifact = entry.Key, count = entry.Value });
        }

        return saveData;
    }

    // ロードしたSaveDataオブジェクトから、自分のデータを復元する
    public void LoadFromSaveData(SaveData saveData)
    {
        this.researchPoints = saveData.researchPoints;
        this.discoveredOrgans = saveData.discoveredOrgans;
        this.discoveredMonsters = saveData.discoveredMonsters;
        this.discoveredArtifacts = saveData.discoveredArtifacts;
        // パーティ情報の復元（もしSaveDataにpartyがあれば）
        if (saveData.currentParty != null)
        {
            this.currentParty = saveData.currentParty;
        }
        else
        {
            // セーブデータにパーティがない（初回など）場合は新しいリストにする
            this.currentParty = new List<MonsterData>();
        }

        // ロード後に必ず3枠確保する
        while (this.currentParty.Count < 3)
        {
            this.currentParty.Add(null);
        }

        // List<OrganSaveData>をDictionaryに変換
        ownedOrgans.Clear();
        foreach (var entry in saveData.ownedOrgans)
        {
            if (entry.organ != null)
            {
                ownedOrgans.Add(entry.organ, entry.count);
            }
        }

        // List<MonsterSaveData>をDictionaryに変換
        ownedMonsters.Clear();
        foreach (var entry in saveData.ownedMonsters)
        {
            if (entry.monster != null)
            {
                ownedMonsters.Add(entry.monster, entry.count);
            }
        }

        // List<ArtifactSaveData>をDictionaryに変換
        ownedArtifacts.Clear();
        foreach (var entry in saveData.ownedArtifacts)
        {
            ownedArtifacts.Add(entry.artifact, entry.count);
        }
    }
    /// <summary>
    /// 全てのセーブデータを初期状態にリセットする
    /// </summary>
    public void ResetData()
    {
        researchPoints = 0;

        ownedOrgans.Clear();
        ownedMonsters.Clear();
        ownedArtifacts.Clear();

        discoveredMonsters.Clear();
        discoveredOrgans.Clear();
        discoveredArtifacts.Clear();

        Debug.Log("プレイヤーデータをリセットしました。");

        // UIにも変更を通知
        OnInventoryChanged?.Invoke();
    }
}