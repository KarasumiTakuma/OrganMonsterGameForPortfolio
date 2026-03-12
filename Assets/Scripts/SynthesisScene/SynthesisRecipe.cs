using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 複数の臓器（素材）と、それによって生まれる一体のモンスター（結果）の組み合わせを定義する、
/// ScriptableObjectベースのレシピデータ。
/// </summary>
[CreateAssetMenu(fileName = "NewSynthesisRecipe", menuName = "Data/Synthesis Recipe")]
public class SynthesisRecipe : ScriptableObject
{
    [Header("素材となる臓器")]
    public List<OrganData> ingredients = new List<OrganData>(); // 材料リスト

    [Header("結果として生まれるモンスター")]
    public MonsterData resultingMonster;
}