using System.Collections.Generic;

/// <summary>
/// セーブ専用のデータクラス
/// </summary>
[System.Serializable]
public class SaveData
{
    // 保存したい単純なデータ
    public int researchPoints;

    // Dictionaryの代わりになるList
    public List<OrganSaveData> ownedOrgans;
    public List<MonsterSaveData> ownedMonsters;
    public List<ArtifactSaveData> ownedArtifacts;

    // 図鑑のリスト
    public List<OrganData> discoveredOrgans;
    public List<MonsterData> discoveredMonsters;
    public List<ArtifactData> discoveredArtifacts;
}

// 臓器辞書を保存するためのヘルパークラス
[System.Serializable]
public class OrganSaveData
{
    public OrganData organ;
    public int count;
}

// モンスター辞書を保存するためのヘルパークラス
[System.Serializable]
public class MonsterSaveData
{
    public MonsterData monster;
    public int count;
}

// アーティファクト辞書を保存するためのヘルパークラス
[System.Serializable]
public class ArtifactSaveData
{
    public ArtifactData artifact;
    public int count;
}
