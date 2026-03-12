using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SynthesisRecipeEditor : EditorWindow
{
    // --- 変数定義 ---
    // 全レシピを保持するリスト
    private List<SynthesisRecipe> allRecipes = new List<SynthesisRecipe>();
    // スクロール位置を保持する変数
    private Vector2 scrollPosition;

    // 新規作成用の変数をリスト形式で保持
    private List<OrganData> newIngredients = new List<OrganData>() { null }; // 最初は1つのスロットから
    private MonsterData newResultingMonster;
    private string newRecipeFileName = "";
    // 検索用変数
    private string searchQuery = "";
    // ソート用変数
    // 並び替えの種類を定義するenum
    private enum SortType
    {
        // アセット名でソート
        AssetName_Ascending, // 昇順
        AssetName_Descending, // 降順
        // 素材数でソート
        IngredientCount_Ascending, // 昇順
        IngredientCount_Descending  // 降順
    }
    // 現在選択されている並び替え方法を保持する変数
    private SortType currentSortType = SortType.AssetName_Ascending;
    
    [MenuItem("Tools/合成レシピエディタ")]
    public static void ShowWindow()
    {
        GetWindow<SynthesisRecipeEditor>("合成レシピエディタ");
    }

    private void OnEnable()
    {
        LoadAllRecipes();
    }

    private void OnGUI()
    {
        // --- 既存レシピの一覧表示 ---
        GUILayout.Label("合成レシピ一覧", EditorStyles.boldLabel);
        if (GUILayout.Button("レシピリストを更新"))
        {
            LoadAllRecipes();
        }

        // --- 検索機能 ---
        searchQuery = EditorGUILayout.TextField("レシピ名で検索", searchQuery);
        // 検索でフィルタリングされたリスト
        var filteredRecipes = string.IsNullOrEmpty(searchQuery) ? allRecipes : allRecipes.Where(r => r.name.ToLower().Contains(searchQuery.ToLower())).ToList();

        // --- ソート機能 ---
        // enumを使ったドロップダウンメニューを表示
        currentSortType = (SortType)EditorGUILayout.EnumPopup("並び替え", currentSortType);

        // フィルタリングようのリストを生成
        var sortedRecipes = new List<SynthesisRecipe>(filteredRecipes);

        switch (currentSortType)
        {
            case SortType.AssetName_Ascending:
                sortedRecipes = sortedRecipes.OrderBy(r => r.name).ToList();
                break;
            case SortType.AssetName_Descending:
                sortedRecipes = sortedRecipes.OrderByDescending(r => r.name).ToList();
                break;
            case SortType.IngredientCount_Ascending:
                sortedRecipes = sortedRecipes.OrderBy(r => r.ingredients.Count).ToList();
                break;
            case SortType.IngredientCount_Descending:
                sortedRecipes = sortedRecipes.OrderByDescending(r => r.ingredients.Count).ToList();
                break;
        }

        SynthesisRecipe recipeToDelete = null;
        // スクロール開始位置、毎回0から描くのでスクロール位置を保持しておく
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var recipe in sortedRecipes)
        {
            if (recipe != null)
            {
                // 横並びを開始
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(recipe.name, EditorStyles.boldLabel);
                // インデントレベルを 1 つ深くする(少し右にずらす)
                EditorGUI.indentLevel++;
                // ラベル文字列、現在の参照、受け入れる型、シーン上のオブジェクトの不可
                EditorGUILayout.ObjectField("結果", recipe.resultingMonster, typeof(MonsterData), false);
                for (int i = 0; i < recipe.ingredients.Count; i++)
                {
                    EditorGUILayout.ObjectField($"素材 {i + 1}", recipe.ingredients[i], typeof(OrganData), false);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                // 「-」ボタンを横に追加し、押されたら削除対象として記録
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    // 確認ダイアログを表示
                    if (EditorUtility.DisplayDialog("レシピの削除", $"本当にレシピ '{recipe.name}' を削除しますか？", "はい", "いいえ"))
                    {
                        recipeToDelete = recipe;
                    }
                }

                EditorGUILayout.EndHorizontal(); // 横並び終了
                EditorGUILayout.Space(10); // レシピ間のスペース
            }
        }

        // スクロール終了位置
        EditorGUILayout.EndScrollView();

        // ループが終わった後で、安全にアセットを削除
        if (recipeToDelete != null)
        {
            string path = AssetDatabase.GetAssetPath(recipeToDelete);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            LoadAllRecipes(); // リストを再読み込みして表示を更新
        }

        // --- 区切り線 ---
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(20);

        // --- 新規レシピ作成エリア ---
        GUILayout.Label("新規レシピ作成", EditorStyles.boldLabel);

        int indexToRemove = -1; // 削除する要素のインデックスを記録する変数
        // 材料のスロットを描画（可変長対応）
        for (int i = 0; i < newIngredients.Count; i++)
        {
            // 横並び開始
            EditorGUILayout.BeginHorizontal();
            newIngredients[i] = (OrganData)EditorGUILayout.ObjectField($"素材 {i + 1}", newIngredients[i], typeof(OrganData), false);

            // 「-」ボタンでリストから素材を削除
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                indexToRemove = i;
            }
            // 横並び終了
            EditorGUILayout.EndHorizontal();
        }

        // forループが終わった後で、安全にリストから要素を削除
        if (indexToRemove != -1)
        {
            newIngredients.RemoveAt(indexToRemove);
        }

        // 「+」ボタンでリストに新しい空のスロットを追加
        if (GUILayout.Button("素材スロットを追加 (+)"))
        {
            newIngredients.Add(null);
        }

        EditorGUILayout.Space(10); // 少しスペースを空ける

        newResultingMonster = (MonsterData)EditorGUILayout.ObjectField("生まれるモンスター", newResultingMonster, typeof(MonsterData), false);
        newRecipeFileName = EditorGUILayout.TextField("新しいレシピのファイル名", newRecipeFileName);

        if (GUILayout.Button("新規レシピとして保存"))
        {
            CreateNewRecipe();
        }
    }

    private void LoadAllRecipes()
    {
        allRecipes.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SynthesisRecipe");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SynthesisRecipe recipe = AssetDatabase.LoadAssetAtPath<SynthesisRecipe>(path);
            if (recipe != null)
            {
                allRecipes.Add(recipe);
            }
        }
    }
    
    private void CreateNewRecipe()
    {
        // リストの中身を1つずつ空でないか確認、どちらかに空があればエラー
        if (newIngredients.Any(item => item == null) || newResultingMonster == null)
        {
            EditorUtility.DisplayDialog("エラー", "全ての素材と結果モンスターを設定してください。", "OK");
            return;
        }
        if (string.IsNullOrEmpty(newRecipeFileName))
        {
            EditorUtility.DisplayDialog("エラー", "ファイル名を入力してください。", "OK");
            return;
        }

        // SynthesisRecipeのインスタンス作成
        SynthesisRecipe newRecipe = ScriptableObject.CreateInstance<SynthesisRecipe>();
        newRecipe.ingredients = new List<OrganData>(newIngredients);
        newRecipe.resultingMonster = newResultingMonster;
        
        // 1. 保存したい理想のパスを作成
        string folderPath = "Assets/Resources/Recipes";
        string desiredPath = $"{folderPath}/{newRecipeFileName}.asset";

        // 2. Unityに、重複しないユニークなパスを生成してもらう
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath);

        // 3. 生成されたユニークなパスでアセットを作成
        AssetDatabase.CreateAsset(newRecipe, uniquePath);
        AssetDatabase.SaveAssets();
        // プロジェクトウィンドウの変更を更新
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", $"新しいレシピを {uniquePath} に保存しました。", "OK");
        LoadAllRecipes();
        
        // 入力欄をクリア
        newIngredients = new List<OrganData>() { null };
        newResultingMonster = null;
        newRecipeFileName = "";
    }
}