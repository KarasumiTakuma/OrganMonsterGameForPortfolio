using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// モンスター合成時の演出パネル（結果表示）を管理するクラス
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    [Header("UIコンポーネント")]
    // 合成演出をするパネル
    [SerializeField] private GameObject performancePanel;
    // 表示するモンスター
    [SerializeField] private Image monsterImage;
    // モンスター名を表示するテキスト
    [SerializeField] private TextMeshProUGUI monsterNameText;
    // パネルを閉じるボタン
    [SerializeField] private Button closeButton;
    [Header("レアリティ表示")]
    public RarityStarDisplay rarityDisplay;

    void Start()
    {
        // 1. 閉じるボタンが押されたら「HidePanel」メソッドを呼び出すように登録
        closeButton.onClick.AddListener(HidePanel);
        
        // 2. ゲーム開始時はパネルを非表示にする
        HidePanel();
    }

    /// <summary>
    /// 合成結果のモンスターデータを受け取り、パネルを表示する
    /// </summary>
    /// <param name="monster">合成されたモンスターのデータ</param>
    public void ShowPerformance(MonsterData monster)
    {
        if (monster == null) return;

        // 受け取ったモンスターの情報をUIコンポーネントに設定
        monsterImage.sprite = monster.GetIcon();
        monsterNameText.text = monster.GetName();
        if (rarityDisplay != null)
        {
            rarityDisplay.SetRarity(monster.GetRarity());
        }

        // パネルを表示状態にする
        performancePanel.SetActive(true);
    }

    /// <summary>
    /// パネルを非表示にする（閉じるボタン用）
    /// </summary>
    private void HidePanel()
    {
        performancePanel.SetActive(false);
    }
}
