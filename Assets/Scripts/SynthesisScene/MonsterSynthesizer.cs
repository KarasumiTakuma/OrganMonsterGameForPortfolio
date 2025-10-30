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
}