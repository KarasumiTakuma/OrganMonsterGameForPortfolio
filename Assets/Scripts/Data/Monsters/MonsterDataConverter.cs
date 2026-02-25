using UnityEngine;
using System.Collections.Generic;
public static class MonsterDataConverter
{
    /// <summary>
    /// 元の MonsterData から新しい AllyMonsterData を作成して返す
    /// </summary>
    public static AllyMonsterData ToAllyMonster(MonsterData baseData)
    {
        if (baseData == null) return null;

        // 新しい AllyMonsterData を生成
        AllyMonsterData ally = ScriptableObject.CreateInstance<AllyMonsterData>();

        // 基本情報をコピー
        ally.SetID(baseData.GetID());
        ally.SetName(baseData.GetName());
        ally.SetMaxHP(baseData.GetMaxHP());
        ally.SetAttackPower(baseData.GetAttackPower());
        ally.SetMonsterType(baseData.GetMonsterType());
        ally.SetRarity(baseData.GetRarity());
        ally.SetDescription(baseData.GetDescription());
        ally.SetHint(baseData.GetHint());
        ally.SetIcon(baseData.GetIcon());
        ally.SetShadowIcon(baseData.GetShadowIcon());

        // カードもコピー
        ally.cards = new List<CardData>(baseData.cards);

        return ally;
    }
}
