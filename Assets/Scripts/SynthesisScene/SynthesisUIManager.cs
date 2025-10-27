using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SynthesisUIManager : MonoBehaviour
{
    [Header("合成スロットのUI")]
    // Inspectorで左側の3つのスロット(Image)を設定
    public List<Image> synthesisSlots;

    [Header("合成ボタンと結果表示")]
    // 合成ボタンを設定
    public Button synthesisButton;
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
                synthesisSlots[i].sprite = selectedIngredients[i].icon;
            }
            else
            {
                synthesisSlots[i].enabled = false; // 空のスロットは非表示
            }
        }

        // 2. レシピと照合して、合成可能かチェック
        currentRecipeResult = synthesizer.Synthesize(selectedIngredients);

        // 3. 合成ボタンの有効/無効を切り替え
        synthesisButton.interactable = (currentRecipeResult != null);

        // 4. 結果表示を更新（プレビュー）
        if (currentRecipeResult != null)
        {
            resultMonsterImage.enabled = true;
            if (GameManager.Instance.PlayerData.ownedMonsters.ContainsKey(currentRecipeResult))
            {
                resultMonsterImage.sprite = currentRecipeResult.icon;
            }
            else
            {
                int rarity = currentRecipeResult.rarity;

                switch (rarity)
                {
                    case 5:
                        // 安全のためリストのサイズチェック
                        if (unknownIconsByRarity.Count > 4)
                            resultMonsterImage.sprite = unknownIconsByRarity[4];
                        break;
                    case 4:
                        if (unknownIconsByRarity.Count > 3)
                            resultMonsterImage.sprite = unknownIconsByRarity[3];
                        break;
                    case 3:
                        if (unknownIconsByRarity.Count > 2)
                            resultMonsterImage.sprite = unknownIconsByRarity[2];
                        break;
                    case 2:
                        if (unknownIconsByRarity.Count > 1)
                            resultMonsterImage.sprite = unknownIconsByRarity[1];
                        break;
                    case 1:
                        if (unknownIconsByRarity.Count > 0)
                            resultMonsterImage.sprite = unknownIconsByRarity[0];
                        break;
                    default:
                        // どのレアリティにも当てはまらない場合（エラー対策）
                        if (unknownIconsByRarity.Count > 0)
                            resultMonsterImage.sprite = unknownIconsByRarity[0];
                        break;
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

        // 3. InventoryUIの表示を更新
        inventoryUI.UpdateDisplay();

        Debug.Log(currentRecipeResult.monsterName + " を生成しました！");

        // 合成後、選択をクリアしてUIを再更新
        selectedIngredients.Clear();
        UpdateSynthesisUI();
        //UpdateInventorySelection();
    }
    private void UpdateInventorySelection()
    {
        // InventoryUIが持つ全スロットのリストを取得
        foreach (var slot in inventoryUI.GetSlotUIs())
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
}