using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ゲーム内に存在する全ての合成レシピを管理し、与えられた素材リストに一致するレシピを検索して、
/// 結果のモンスターデータを返すロジッククラス。
/// </summary>
public class MonsterSynthesizer
{
    private List<SynthesisRecipe> allRecipes;

    public void LoadAllRecipes()
    {
        // Assets/Resources/というパスの中のRecipesフォルダの中からSynthesisRecipeアセットを探す
        allRecipes = Resources.LoadAll<SynthesisRecipe>("Recipes").ToList();
    }

    // レシピを確認して合成する
    public MonsterData Synthesize(List<OrganData> inputIngredients)
    {
        foreach (var recipe in allRecipes)
        {
            // レシピと入力された素材の数が違う場合は、次のレシピへ
            if (recipe.ingredients.Count != inputIngredients.Count)
            {
                continue;
            }

            // 素材が一致するかどうかをチェック
            // レシピと渡された素材をID順にそーそして、比較する
            var sortedRecipeIngredients = recipe.ingredients.OrderBy(organ => organ.GetID()).ToList();
            var sortedInputIngredients = inputIngredients.OrderBy(organ => organ.GetID()).ToList();

            // 順番を揃えたリストが完全に一致するかどうかを判定
            if (sortedRecipeIngredients.SequenceEqual(sortedInputIngredients))
            {
                // 一致したら、結果のモンスターを返す
                return recipe.resultingMonster;
            }
        }

        // 一致するレシピがなければ、nullを返す
        return null;
    }

    /// <summary>
    /// 現在の素材リストから、次に追加可能な素材の候補リストを返す
    /// </summary>
    public List<ScriptableObject> GetCompletableIngredients(List<ScriptableObject> currentIngredients)
    {
        // 何も選択されていない場合は、全ての素材が候補になる（またはnullを返して全開放を示す）
        if (currentIngredients == null || currentIngredients.Count == 0)
        {
            return null; // "制限なし" を意味する
        }

        HashSet<ScriptableObject> potentialIngredients = new HashSet<ScriptableObject>();

        foreach (var recipe in allRecipes)
        {
            // このレシピが、現在の素材リストを「部分的に」満たしているかチェック
            List<ScriptableObject> remainingRecipeIngredients = new List<ScriptableObject>(recipe.ingredients);
            bool isMatch = true;

            foreach (var inputItem in currentIngredients)
            {
                // レシピの中に、入力された素材があるか？
                // (厳密な一致を見るため、IDだけでなく参照や型でチェックが必要だが、ここではListのRemoveを利用)
                // ※ScriptableObjectの比較は参照比較になる
                if (remainingRecipeIngredients.Contains(inputItem))
                {
                    remainingRecipeIngredients.Remove(inputItem);
                }
                else
                {
                    // このレシピには含まれていない素材が入力されている -> 候補外
                    isMatch = false;
                    break;
                }
            }

            // 現在の入力がレシピの一部であり、かつまだ足りない素材がある場合
            if (isMatch && remainingRecipeIngredients.Count > 0)
            {
                // 残っている素材を候補として追加
                foreach (var remaining in remainingRecipeIngredients)
                {
                    potentialIngredients.Add(remaining);
                }
            }
        }

        return potentialIngredients.ToList();
    }
}