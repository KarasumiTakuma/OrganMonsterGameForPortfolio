using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// MonsterData を元に、
/// バトル用のAllyMonsterDataを生成する変換ユーティリティクラス。
/// ScriptableObject の新規インスタンス生成、
/// 基本ステータスのコピー、
/// 所持カード情報の複製、
/// を責務として持つ。
/// </summary>
public static class MonsterDataConverter
{
    /// <summary>
    /// 元となる MonsterData から、新しい AllyMonsterData を生成して返す。
    /// 主に「所持モンスターをバトル用データへ変換する」目的で使用される。
    /// 元データ自体は変更せず、コピーされた独立インスタンスを生成する。
    /// </summary>
    /// <param name="sourceMonsterData">変換元となるMonsterData</param>
    /// <returns>生成された AllyMonsterData。引数がnullの場合はnullを返す</returns>
    public static AllyMonsterData CreateAllyMonsterFrom(MonsterData sourceMonsterData)
    {
        if (sourceMonsterData == null) return null;

        AllyMonsterData allyData = ScriptableObject.CreateInstance<AllyMonsterData>();

        // MonsterData が持つ共通情報を AllyMonsterData 側へ転写
        allyData.SetID(sourceMonsterData.GetID());
        allyData.SetName(sourceMonsterData.GetName());
        allyData.SetMaxHP(sourceMonsterData.GetMaxHP());
        allyData.SetAttackPower(sourceMonsterData.GetAttackPower());
        allyData.SetMonsterType(sourceMonsterData.GetMonsterType());
        allyData.SetRarity(sourceMonsterData.GetRarity());
        allyData.SetDescription(sourceMonsterData.GetDescription());
        allyData.SetHint(sourceMonsterData.GetHint());
        allyData.SetIcon(sourceMonsterData.GetIcon());
        allyData.SetShadowIcon(sourceMonsterData.GetShadowIcon());

        // List を新規生成することで、元データとの参照共有を防ぐ
        // （バトル中の変更が元のsourceMonsterDataデータへ影響しないようにするため）
        allyData.cards = new List<CardData>(sourceMonsterData.cards);

        return allyData;
    }
}
