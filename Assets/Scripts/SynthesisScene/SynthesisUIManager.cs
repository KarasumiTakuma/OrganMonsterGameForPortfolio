using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class SynthesisUIManager : MonoBehaviour
{
    [Header("合成スロットのUI")]
    // Inspectorで左側の3つのスロット(Image)を設定
    public List<Image> synthesisSlots;

    [Header("合成ボタンと結果表示")]
    // 合成ボタンを設定
    public Button synthesisButton;
    public TextMeshProUGUI synthesisButtonText;
    private Color buttonTextOriginalColor; // テキストの元の色を記憶
    // 生成するモンスターを表示するイメージを設定
    public Image resultMonsterImage;

    [Header("インベントリUIへの参照")]
    // Inspectorでインベントリパネルを設定
    public InventoryUI inventoryUI;

    // 現在選択されている素材リスト
    private List<OrganData> selectedIngredients = new List<OrganData>();
    // 合成ロジック本体
    private MonsterSynthesizer synthesizer = new MonsterSynthesizer();
    // 表示中のレシピ結果
    private MonsterData currentRecipeResult;
    // 所持していないモンスターは？を表示する
    public List<Sprite> unknownIconsByRarity;
    // 範囲外(？を用意していないレアリティ)はデフォルトの？
    private const int DEFAULT_UNKNOWN_ICON_INDEX = 0;
    
    [Header("演出UIへの参照")]
    // InspectorでPerformanceManagerを持つオブジェクトを設定
    public PerformanceManager performanceManager;

    /// <summary>
    /// このUIが表示状態になったときに呼び出される。
    /// インベントリスロットからのクリックイベントの購読を開始する。
    /// </summary>
    private void OnEnable()
    {
        InventorySlotUI.OnSelectionChanged += HandleSelectionChanged;
        // 合成ロジック用のレシピをロード
        synthesizer.LoadAllRecipes();
    }

    /// <summary>
    /// このUIが非表示状態になったときに呼び出される。
    /// メモリリークを防ぐため、クリックイベントの購読を必ず解除する。
    /// </summary>
    private void OnDisable()
    {
        InventorySlotUI.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void Start()
    {
        // ボタンがクリックされるとPerformSynthesisメソッドを呼び出す予約
        synthesisButton.onClick.AddListener(PerformSynthesis);

        if (synthesisButtonText != null) buttonTextOriginalColor = synthesisButtonText.color;

        // 3つの合成スロットそれぞれに、クリックイベントを登録する
        for (int i = 0; i < synthesisSlots.Count; i++)
        {
            // ループ変数をローカルにコピー（ラムダ式で正しく使うため）
            int index = i;

            // ImageにButtonコンポーネントがなければ追加
            Button button = synthesisSlots[i].GetComponent<Button>();
            if (button == null)
            {
                button = synthesisSlots[i].gameObject.AddComponent<Button>();
                // ボタンの見た目が変わらないように設定
                button.transition = Selectable.Transition.None;
            }

            // ボタンがクリックされたら、HandleSynthesisSlotClickメソッドを「そのインデックス番号」で呼び出す
            button.onClick.AddListener(() => HandleSynthesisSlotClick(index));
        }

        UpdateSynthesisUI(); // 初期表示を更新
    }


    // スロットの選択状態が変わるたびに呼ばれる
    private void HandleSelectionChanged(OrganData organ, bool isSelected)
    {
        // スロットから送られてきた情報に基づいて、リストを更新
        if (isSelected)
        {
            // 選択されたので、リストに追加（上限チェック）
            if (selectedIngredients.Count < synthesisSlots.Count)
            {
                selectedIngredients.Add(organ);
            }
        }
        else
        {
            // 選択解除されたので、リストから除去
            selectedIngredients.Remove(organ);
        }

        UpdateSynthesisUI();
        UpdateInventorySelection();
        UpdateInventoryInteractability();
    }

    // 合成UI全体の表示を更新する
    private void UpdateSynthesisUI()
    {
        // 1. 合成スロットのアイコンを更新
        for (int i = 0; i < synthesisSlots.Count; i++)
        {
            if (i < selectedIngredients.Count)
            {
                synthesisSlots[i].enabled = true;
                synthesisSlots[i].sprite = selectedIngredients[i].GetIcon();
            }
            else
            {
                synthesisSlots[i].enabled = false; // 空のスロットは非表示
            }
        }

        // 2. レシピと照合して、合成可能かチェック
        currentRecipeResult = synthesizer.Synthesize(selectedIngredients);

        // 3. 合成ボタンの有効/無効を切り替え
        bool isInteractable = (currentRecipeResult != null);
        synthesisButton.interactable = isInteractable;
        // 合成ボタンの文字の透明度も変更
        if (synthesisButtonText != null)
        {
            if (isInteractable)
            {
                // 押せる時は、元の色に戻す
                synthesisButtonText.color = buttonTextOriginalColor;
            }
            else
            {
                // 押せない時は、元の色のアルファ値（透明度）を半分にする
                Color disabledColor = buttonTextOriginalColor;
                disabledColor.a = 0.5f; // 0.0 (透明) ～ 1.0 (不透明)
                synthesisButtonText.color = disabledColor;
            }
        }

        // 4. 結果表示を更新（プレビュー）
        if (currentRecipeResult != null)
        {
            resultMonsterImage.enabled = true;
            if (GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(currentRecipeResult))
            {
                resultMonsterImage.sprite = currentRecipeResult.GetIcon();
            }
            else
            {
                // モンスターのレアリティ（1～5）を、リストのインデックス（0～4）に変換
                int iconIndex = currentRecipeResult.GetRarity() - 1;

                // インデックスがリストの範囲内にあるか安全にチェック
                if (iconIndex >= 0 && iconIndex < unknownIconsByRarity.Count)
                {
                    resultMonsterImage.sprite = unknownIconsByRarity[iconIndex];
                }
                else
                {
                    // 範囲外の場合（またはリストが空）のデフォルト処理
                    if (unknownIconsByRarity.Count > 0)
                        resultMonsterImage.sprite = unknownIconsByRarity[DEFAULT_UNKNOWN_ICON_INDEX];
                }
            }
            
        }
        else
        {
            resultMonsterImage.enabled = false;
        }
    }

    // 合成ボタンが押されたときに呼ばれる
    private void PerformSynthesis()
    {
        if (currentRecipeResult == null) return;

        // --- 実際にアイテムを消費してモンスターを入手する処理 ---

        // 1. GameManager.Instance.PlayerDataからselectedIngredientsを消費
        foreach (var ingredient in selectedIngredients)
        {
            // 今回は1つずつ消費する想定
            // GameManager.Instance.PlayerData.ownedOrgans[ingredient]--;
            // if(GameManager.Instance.PlayerData.ownedOrgans[ingredient] <= 0)
            // {
            //     GameManager.Instance.PlayerData.ownedOrgans.Remove(ingredient);
            // }
            GameManager.Instance.PlayerData.RemoveOrgan(ingredient);
        }

        // 2. PlayerDataにcurrentRecipeResultを追加
        GameManager.Instance.PlayerData.AddMonster(currentRecipeResult, 1);

        // 3. 合成演出UIを呼びだす
        if (performanceManager != null)
        {
            performanceManager.ShowPerformance(currentRecipeResult);
        }
        else
        {
            Debug.LogWarning("PerformanceManagerが設定されていません。");
        }

        // 4. InventoryUIの表示を更新
        inventoryUI.UpdateDisplay();

        Debug.Log(currentRecipeResult.GetName() + " を生成しました！");

        // 5. 合成後、選択をクリアしてUIを再更新
        selectedIngredients.Clear();
        UpdateSynthesisUI();
        UpdateInventorySelection();

        ResetInventoryInteractability();
    }
    private void UpdateInventorySelection()
    {
        // InventoryUIが持つ全スロットのリストを取得
        foreach (var slot in inventoryUI.SlotUIs)
        {
            // そのスロットの臓器データを取得
            OrganData organInSlot = slot.GetAssignedOrgan();

            // その臓器が選択中リストに含まれているか、かつスロットが空でないか
            bool isSelected = organInSlot != null && selectedIngredients.Contains(organInSlot);

            // スロットに選択状態を伝えて色を変更させる
            slot.SetSelected(isSelected);
        }
    }
    
    /// <summary>
    /// 左側の合成スロット（0, 1, 2番目）がクリックされたときに呼ばれる
    /// スロットをクリックすると選択を解除する
    /// </summary>
    private void HandleSynthesisSlotClick(int index)
    {
        // もし、クリックされたスロットにアイテムが設定されていれば
        if (index < selectedIngredients.Count)
        {
            // 選択リストからそのアイテムを削除
            selectedIngredients.RemoveAt(index);
            
            // UIを更新
            UpdateSynthesisUI();
            // インベントリの選択色も更新
            UpdateInventorySelection();
        }
    }
    // インベントリの選択制限を更新する
    private void UpdateInventoryInteractability()
    {
        // 1. 次に追加可能な素材のリストを取得
        // (selectedIngredientsは List<OrganData> なので、キャストが必要な場合があります)
        List<ScriptableObject> currentList = selectedIngredients.Cast<ScriptableObject>().ToList();
        List<ScriptableObject> validIngredients = synthesizer.GetCompletableIngredients(currentList);

        // 2. インベントリの全スロットをループして制御
        // (InventoryUIの SlotUIs プロパティが public である必要があります)
        foreach (var slot in inventoryUI.SlotUIs)
        {
            OrganData organ = slot.GetAssignedOrgan();
            
            // 空スロットなら無視
            if (organ == null) continue;

            // 既に選択されているスロットは、解除できるように常に有効
            if (selectedIngredients.Contains(organ))
            {
                slot.SetInteractable(true);
                continue;
            }

            // 候補リストに含まれているなら有効、それ以外は無効
            if (validIngredients == null || validIngredients.Contains(organ))
            {
                slot.SetInteractable(true);
            }
            else
            {
                slot.SetInteractable(false);
            }
        }
    }

    // インベントリの全スロットを選択可能な状態に戻す
    private void ResetInventoryInteractability()
    {
        foreach (var slot in inventoryUI.SlotUIs)
        {
            // 全てのスロットを有効（選択可能）にする
            slot.SetInteractable(true);
        }
    }
}