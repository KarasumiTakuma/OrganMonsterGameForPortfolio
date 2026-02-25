using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MonstersEditor : EditorWindow
{
    // --- 変数定義 ---
    // 全てのモンスターの参照
    private List<MonsterData> allMonsters = new List<MonsterData>();
    private Vector2 scrollPosition;
    // 検索用変数
    private string searchQuery = "";
    // 各モンスターのFoldoutが開いているか(true)閉じているか(false)を保存
    private Dictionary<MonsterData, bool> monsterFoldoutStates = new Dictionary<MonsterData, bool>();
    private enum SortType
    {
        // 名前でソート(アセット名であなくモンスター名)
        Name_Ascending, // 昇順
        Name_Descending, // 降順
        // モンスターIDでソート
        MonsterID_Ascending,
        MonsterID_Descending,
        // レアリティでソート
        Rarity_Ascending,
        Rarity_Descending
    }
    private SortType currentSortType = SortType.MonsterID_Ascending;

    // --- 新規作成用の変数 ---
    private string newMonsterAssetName = "";
    private int newMonsterId = 0;
    private int newMonsterRarity = 1;
    private MonsterType newMonsterType = MonsterType.Other;
    private int newMonsterMaxHp = 100;
    private int newMonsterAttackPower = 5;
    private Sprite newMonsterIcon;

    [MenuItem("Tools/モンスター管理エディタ")]
    public static void ShowWindow()
    {
        GetWindow<MonstersEditor>("モンスター管理エディタ");
    }

    private void OnEnable()
    {
        LoadAllMonsters();
    }

    private void OnGUI()
    {
        // --- 制御UI ---
        GUILayout.Label("モンスタ一覧", EditorStyles.boldLabel);
        if (GUILayout.Button("データ更新"))
        {
            LoadAllMonsters();
        }
        // 検索欄
        searchQuery = EditorGUILayout.TextField("モンスター名で検索", searchQuery);
        currentSortType = (SortType)EditorGUILayout.EnumPopup("並び替え", currentSortType);

        EditorGUILayout.Space(10);

        // --- データ表示・編集エリア ---
        // スクロールを管理
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        // 検索してリストを絞る
        var filteredMonsters = string.IsNullOrEmpty(searchQuery) 
        ? allMonsters
        // LINQのWhereでフィルタリングする
        : allMonsters.Where(monster => monster.GetName().ToLower().Contains(searchQuery.ToLower())).ToList();
        // ソートしたリスト
        var sortedMonsters = SortMonsters(filteredMonsters);
        // 削除するアセットを保持
        MonsterData monsterToDelete = null;

        foreach (var monster in sortedMonsters)
        {
            // もしアセットが削除されて null になっていたら、スキップする
            if (monster == null) continue;

            // 横に描画開始
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            // 開閉状態を取得
            bool isFoldoutOpen = GetFoldoutState(monster);
            // 文字列補間($)を使い、モンスター名とアセット名を組み合わせる
            string displayName = $"{monster.GetName()} ({monster.name})";
            // Foldout(折りたたみヘッダー)を描画
            isFoldoutOpen = EditorGUILayout.Foldout(isFoldoutOpen, displayName, true, EditorStyles.foldoutHeader);
            // 辞書に開閉の状態(true or false)を記憶
            SetFoldoutState(monster, isFoldoutOpen);



            // 「-」削除ボタンを横に配置
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("モンスターの削除", $"本当に '{monster.GetName()}' を削除しますか？", "はい", "いいえ"))
                {
                    monsterToDelete = monster;
                }
            }
            EditorGUILayout.EndHorizontal(); // 横並び終了

            // もしFoldoutが開かれていたら、詳細情報を描画
            if (isFoldoutOpen)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck(); // 監視を開始

                // --- ゲッターで現在の値をUIに表示 ---
                // DelayedTextFieldはEnterかフォーカスを外した時に値の変更を確定させる->入力流にEndChangeCheck()が実行されるのを防ぐ
                string newName = EditorGUILayout.DelayedTextField("表示名", monster.GetName());
                int newID = EditorGUILayout.DelayedIntField("モンスターID", monster.GetID());
                int newRarity = EditorGUILayout.IntSlider("レアリティ", monster.GetRarity(), 1, 5);
                MonsterType newType = (MonsterType)EditorGUILayout.EnumPopup("タイプ", monster.GetMonsterType());
                int newMaxHP = EditorGUILayout.IntField("最大HP", monster.GetMaxHP());
                int newAttackPower = EditorGUILayout.IntField("攻撃力", monster.GetAttackPower());
                Sprite newIcon = (Sprite)EditorGUILayout.ObjectField("アイコン", monster.GetIcon(), typeof(Sprite), false, GUILayout.Height(64));
                Sprite newShadowIcon = (Sprite)EditorGUILayout.ObjectField("影アイコン", monster.GetShadowIcon(), typeof(Sprite), false, GUILayout.Height(64));

                EditorGUILayout.LabelField("説明文");
                string newDescription = EditorGUILayout.DelayedTextField(monster.GetDescription(), GUILayout.Height(40));
                EditorGUILayout.LabelField("ヒント");
                string newHint = EditorGUILayout.DelayedTextField(monster.GetHint(), GUILayout.Height(40));

                

                // --- カードリスト編集機能  ---
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("カードリスト", EditorStyles.boldLabel);
                int newCardCount = EditorGUILayout.IntField("カード枚数", monster.cards.Count);
                while (newCardCount != monster.cards.Count)
                {
                    if (newCardCount > monster.cards.Count) monster.cards.Add(null);
                    else monster.cards.RemoveAt(monster.cards.Count - 1);
                }
                for (int i = 0; i < monster.cards.Count; i++)
                {
                    monster.cards[i] = (CardData)EditorGUILayout.ObjectField($"カード {i + 1}", monster.cards[i], typeof(CardData), false);
                }

                // 監視を終了し、変更があったか確認
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(monster, "モンスターデータを変更"); // Undo(元に戻す)機能をサポート

                    // --- セッターメソッドを使って新しい値を書き込む ---
                    monster.SetName(newName);
                    monster.SetID(newID);
                    monster.SetRarity(newRarity);
                    monster.SetMonsterType(newType);
                    monster.SetMaxHP(newMaxHP);
                    monster.SetAttackPower(newAttackPower);
                    monster.SetIcon(newIcon);
                    monster.SetShadowIcon(newShadowIcon);
                    monster.SetDescription(newDescription);
                    monster.SetHint(newHint);

                    EditorUtility.SetDirty(monster); // アセットが変更されたことをUnityに通知
                    AssetDatabase.SaveAssets();      // (変更を即時保存。任意)
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        EditorGUILayout.EndScrollView();

        if (monsterToDelete != null)
        {
            EditorUtils.DeleteAsset(monsterToDelete);
            // 削除されたアセットを参照するEditorUtils.DeleteAssetはnullを返すが、明示的にnullとする
            monsterToDelete = null;
            LoadAllMonsters();
        }

        // --- 区切り線 ---
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(20);

        // --- 新規追加エリア ---
        GUILayout.Label("新規モンスターアセット作成", EditorStyles.boldLabel);
        newMonsterAssetName = EditorGUILayout.TextField("新しいアセット名", newMonsterAssetName);
        newMonsterId = EditorGUILayout.IntField("モンスターID", newMonsterId);
        newMonsterRarity = EditorGUILayout.IntSlider("レアリティ", newMonsterRarity, 1, 5);
        newMonsterType = (MonsterType)EditorGUILayout.EnumPopup("タイプ", newMonsterType);
        newMonsterIcon = (Sprite)EditorGUILayout.ObjectField("アイコン", newMonsterIcon, typeof(Sprite), false);

        if (GUILayout.Button("新規アセットとして保存"))
        {
            CreateNewMonster();
        }
    }

    private List<MonsterData> SortMonsters(List<MonsterData> monsters)
    {
        switch (currentSortType)
        {
            case SortType.Name_Ascending: return monsters.OrderBy(r => r.GetName()).ToList();
            case SortType.Name_Descending: return monsters.OrderByDescending(r => r.GetName()).ToList();
            case SortType.MonsterID_Ascending: return monsters.OrderBy(r => r.GetID()).ToList();
            case SortType.MonsterID_Descending: return monsters.OrderByDescending(r => r.GetID()).ToList();
            case SortType.Rarity_Ascending: return monsters.OrderBy(r => r.GetRarity()).ToList();
            case SortType.Rarity_Descending: return monsters.OrderByDescending(r => r.GetRarity()).ToList();
            default: return monsters;
        }
    }

    private void LoadAllMonsters()
    {
        allMonsters = EditorUtils.LoadAllAssets<MonsterData>();
    }

    // --- 新規作成の処理 ---
    private void CreateNewMonster()
    {
        if (string.IsNullOrEmpty(newMonsterAssetName))
        {
            EditorUtility.DisplayDialog("エラー", "アセット名を入力してください。", "OK");
            return;
        }

        MonsterData newMonster = ScriptableObject.CreateInstance<MonsterData>();
    
        // --- セッターメソッドを使って値を設定 ---
        newMonster.SetName(newMonsterAssetName);
        newMonster.SetID(newMonsterId);
        newMonster.SetRarity(newMonsterRarity);
        newMonster.SetMonsterType(newMonsterType);
        newMonster.SetMaxHP(newMonsterMaxHp);
        newMonster.SetAttackPower(newMonsterAttackPower);
        newMonster.SetIcon(newMonsterIcon);

        string folderPath = "Assets/Resources/Data/Monsters"; // 保存先フォルダ
        // フォルダがなければ作成
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Organs");
        }

        string desiredPath = $"{folderPath}/{newMonsterAssetName}.asset";
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath);

        AssetDatabase.CreateAsset(newMonster, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", $"新しい臓器を {uniquePath} に保存しました。", "OK");
        LoadAllMonsters();

        // 入力欄をクリア
        newMonsterAssetName = "";
        newMonsterId = 0;
        newMonsterRarity = 1;
        newMonsterIcon = null;
    }
    private bool GetFoldoutState(MonsterData monster)
    {
        if (!monsterFoldoutStates.ContainsKey(monster))
        {
            monsterFoldoutStates[monster] = false;
        }
        return monsterFoldoutStates[monster];
    }

    private void SetFoldoutState(MonsterData monster, bool state)
    {
        monsterFoldoutStates[monster] = state;
    }
}