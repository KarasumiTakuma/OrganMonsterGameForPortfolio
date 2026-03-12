using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// アイテムの売却システムを管理するクラス
/// </summary>
public class ItemSellSystem : MonoBehaviour
{
    [Header("Main Controls")]
    [SerializeField] private Button openPanelButton;   // 売却画面を開くボタン

    [Header("Sell Panel UI")]
    [SerializeField] private GameObject sellPanel;         // 売却パネル全体
    [SerializeField] private Button closePanelButton;      // パネルを閉じるボタン
    [SerializeField] private TMP_InputField quantityInput; // 個数入力
    [SerializeField] private TextMeshProUGUI nameText;     // アイテム名
    [SerializeField] private TextMeshProUGUI priceInfoText;// 価格情報
    [SerializeField] private Button executeSellButton;     // 売却実行ボタン

    [Header("Price Settings (レアリティごと)")]
    [SerializeField] private int organPricePerRarity = 100;     // 臓器売却単価：レアリティ × 100
    [SerializeField] private int monsterPricePerRarity = 500;   // モンスター売却単価：レアリティ × 500
    [SerializeField] private int artifactPricePerRarity = 2000; // アーティファクト売却単価：レアリティ × 2000

    // 内部変数
    private ScriptableObject currentSelectedItem;
    private int currentUnitPrice;
    private int currentOwnedCount;

    private void Start()
    {
        // --- ボタンイベント登録 ---
        if (openPanelButton != null)
            openPanelButton.onClick.AddListener(OnOpenPanelClicked);

        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(OnClosePanelClicked);

        if (executeSellButton != null)
            executeSellButton.onClick.AddListener(OnExecuteSellClicked);

        // --- 入力フィールドイベント ---
        if (quantityInput != null)
        {
            quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
            quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;  // Inteferのみ許可
        }

        // 初期状態設定
        sellPanel.SetActive(false);        // パネルは隠す
        openPanelButton.interactable = false; // 何も選択してないので「開くボタン」は押せない状態に設定
    }

    private void OnEnable()
    {
        GenericSlotUI.OnSlotClicked += OnItemSelected;
    }

    private void OnDisable()
    {
        GenericSlotUI.OnSlotClicked -= OnItemSelected;
    }

    // スロットがクリックされた時の処理
    private void OnItemSelected(ScriptableObject data)
    {
        // 1. 無効なデータが来た場合 -> 解除
        if (data == null)
        {
            ClearSelection(); // 開くボタンを無効化して、パネルも閉じる
            return;
        }

        // 2. すでに選択中のアイテムと同じものをクリックした場合 -> 解除（トグル動作）
        if (currentSelectedItem == data)
        {
            ClearSelection();
            return;
        }

        // 3. 新しいアイテムを選択した場合 -> 更新
        currentSelectedItem = data;
        openPanelButton.interactable = true; // 開くボタンを有効化！

        // もし既にパネルが開いていたら、表示内容を即座に更新する
        if (sellPanel.activeSelf)
        {
            SetupPanelDisplay();
        }
    }

    // 選択解除時の処理をまとめたメソッド
    private void ClearSelection()
    {
        currentSelectedItem = null;
        openPanelButton.interactable = false; // 開くボタンを無効化
        sellPanel.SetActive(false);           // パネルが開いていたら閉じる
    }

    // 「売却画面を開く」ボタンが押された時
    private void OnOpenPanelClicked()
    {
        if (currentSelectedItem == null) return;

        SetupPanelDisplay();      // データの計算と表示
        sellPanel.SetActive(true); // パネルを表示
    }

    // 「閉じる」ボタンが押された時
    private void OnClosePanelClicked()
    {
        sellPanel.SetActive(false);
    }

    // パネルの表示内容をセットアップするメソッド
    private void SetupPanelDisplay()
    {
        // 引数にoutをつけて、データ型ごとの情報と単価を一括で取得する
        if (TryGetItemData(currentSelectedItem, out string name, out int unitPrice, out int count))
        {
            currentUnitPrice = unitPrice;  // 計算済みの単価をセット
            currentOwnedCount = count;

            if (nameText != null) nameText.text = name;

            // 入力を1にリセット
            if (quantityInput != null) quantityInput.text = "1";

            UpdatePriceInfo(1);
        }
    }

    // データ型ごとの情報と「単価」を取得するメソッド
    private bool TryGetItemData(ScriptableObject data, out string name, out int unitPrice, out int count)
    {
        name = ""; unitPrice = 0; count = 0;

        if (data == null) return false;

        if (data is OrganData o)
        {
            name = o.GetName();
            count = o.GetCount();
            // 臓器の価格計算
            unitPrice = o.GetRarity() * organPricePerRarity;
            return true;
        }

        if (data is MonsterData m)
        {
            name = m.GetName();
            count = m.GetCount();
            // モンスターの価格計算
            unitPrice = m.GetRarity() * monsterPricePerRarity;
            return true;
        }

        if (data is ArtifactData a)
        {
            name = a.GetName();
            count = a.GetCount();
            // アーティファクトの価格計算
            unitPrice = a.GetRarity() * artifactPricePerRarity;
            return true;
        }

        return false;
    }

    // 個数入力が変わった時
    private void OnQuantityInputChanged(string input)
    {
        if (int.TryParse(input, out int quantity)) // int.TryParseでinputを整数に変換できるか試す
        {
            if (quantity > currentOwnedCount)
            {
                quantity = currentOwnedCount;
                quantityInput.text = quantity.ToString();
            }
            else if (quantity < 0)
            {
                quantity = 0;
                quantityInput.text = "0";
            }
            UpdatePriceInfo(quantity);
        }
        else
        {
            UpdatePriceInfo(0);
        }
    }

    // 価格情報の更新と、売却実行ボタンの有効/無効を切り替えるメソッド
    private void UpdatePriceInfo(int quantity)
    {
        int total = quantity * currentUnitPrice;
        if (priceInfoText != null)
            priceInfoText.text = $"@{currentUnitPrice} x {quantity} = <color=yellow>{total} Pt</color>";

        // 売却実行ボタンは個数が正しくないと押せない
        executeSellButton.interactable = (quantity > 0); // 個数が0以下なら無効化
    }

    // 売却実行
    private void OnExecuteSellClicked()
    {
        if (currentSelectedItem == null) return;
        if (!int.TryParse(quantityInput.text, out int quantity) || quantity <= 0) return;

        int totalPoints = quantity * currentUnitPrice;

        // データ更新
        var player = GameManager.Instance.PlayerData;
        player.AddPoints(totalPoints);

        // アイテムの所持数を減らす
        if (currentSelectedItem is OrganData o) player.RemoveOrgan(o, quantity);
        else if (currentSelectedItem is MonsterData m) player.RemoveMonster(m, quantity);
        else if (currentSelectedItem is ArtifactData a) player.RemoveArtifact(a, quantity);

        Debug.Log("売却完了");

        // 売却後の更新処理
        if (TryGetItemData(currentSelectedItem, out _, out _, out int newCount))
        {
            currentOwnedCount = newCount;
            // まだ持っているならUI更新、なければ閉じる
            if (currentOwnedCount > 0)
            {
                quantityInput.text = "1";
                UpdatePriceInfo(1);
            }
            else
            {
                sellPanel.SetActive(false); // 売り切ったら閉じる
                currentSelectedItem = null;
                openPanelButton.interactable = false; // 開くボタンも無効化
            }
        }
    }
}