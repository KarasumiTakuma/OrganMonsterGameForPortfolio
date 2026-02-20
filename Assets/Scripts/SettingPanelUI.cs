using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 設定パネルのプレハブにアタッチし、UI操作を管理するクラス
/// </summary>
public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button closeButton;
    [SerializeField] private AudioClip clickSE; // InspectorでSEを設定
    [SerializeField] private Button saveButton;

    [Header("SelectTargetingMode")]
    [SerializeField] private Button targetingModeButton;  // ターゲットモードの切り替え設定へのボタン
    [SerializeField] private GameObject targetingModePanel;   // ターゲットモード切り替えパネル
    [SerializeField] private Button clickTargetModeButton;    // 敵クリック選択型に切り替えるボタン
    [SerializeField] private Button dragAutoTargetButton;   // ドラッグ自動ターゲット型に切り替えるボタン
    [SerializeField] private Button targetingModePanelCloseButton;   // ターゲットモード切り替えパネルを閉じるボタン


    void Start()
    {
        // 起動時に、SettingsManagerが保持している現在の音量値をスライダーに反映
        bgmSlider.value = SettingsManager.Instance.BgmVolume;
        seSlider.value = SettingsManager.Instance.SeVolume;

        // スライダーが操作されたら、SettingsManagerの値を更新
        bgmSlider.onValueChanged.AddListener(SettingsManager.Instance.SetBgmVolume);
        seSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSeVolume);

        // 閉じるボタンが押されたら、SettingsManagerに閉じるよう依頼、クリック音を鳴らす
        closeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            SettingsManager.Instance.CloseSettingsPanel();
        });

        saveButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            SaveManager.Instance.SaveGame();
        });


        //  ターゲットモードパネルの初期状態
        targetingModePanel.SetActive(false);
        UpdateTargetingModeVisual();

        // ターゲットモード切替ボタン
        targetingModeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            targetingModePanel.SetActive(true);
        });

        // 敵クリック選択型のボタン
        clickTargetModeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            SettingsManager.Instance.SetTargetingMode(TargetingMode.ClickedTarget);
            UpdateTargetingModeVisual();
        });

        // ドラッグ自動ターゲット型のボタン
        dragAutoTargetButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            SettingsManager.Instance.SetTargetingMode(TargetingMode.DragAutoTarget);
            UpdateTargetingModeVisual();
        });

        // ターゲットモード切り替えパネルを閉じるボタン
        targetingModePanelCloseButton.onClick.AddListener(() =>
         {
             AudioManager.Instance.PlaySE(clickSE);
             PlayerPrefs.SetInt(
             "TargetingMode",
             (int)SettingsManager.Instance.CurrentTargetingMode
         );
             PlayerPrefs.Save();

             targetingModePanel.SetActive(false);
         });
    }

    // 2つのターゲットモード切り替えボタンの見た目を変更するメソッド
    private void UpdateTargetingModeVisual()
    {
        var targetingMode = SettingsManager.Instance.CurrentTargetingMode;
        // 現在のターゲットモードが敵クリック選択型なら、trueを返す。
        bool isClick = targetingMode == TargetingMode.ClickedTarget;

        Color selectedColor = new Color(0.4f, 0.4f, 0.4f);
        Color normalColor = Color.white;

        ColorBlock clickColors = clickTargetModeButton.colors;
        ColorBlock dragColors = dragAutoTargetButton.colors;

        // 現在選択中のターゲットモードのボタンを薄暗く、選択していないモードの方は明るくする
        clickColors.normalColor = isClick ? selectedColor : normalColor;
        clickColors.highlightedColor = isClick ? selectedColor : normalColor;
        clickColors.pressedColor = isClick ? selectedColor : normalColor;
        clickColors.selectedColor = isClick ? selectedColor : normalColor;

        dragColors.normalColor = isClick ? normalColor : selectedColor;
        dragColors.highlightedColor = isClick ? normalColor : selectedColor;
        dragColors.pressedColor = isClick ? normalColor : selectedColor;
        dragColors.selectedColor = isClick ? normalColor : selectedColor;

        clickTargetModeButton.colors = clickColors;
        dragAutoTargetButton.colors = dragColors;
    }
}