using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 「戦闘用の設定パネル」のプレハブにアタッチし、UI操作を管理するクラス
/// </summary>
public class SettingsPanelUIForBattleScene : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button closeButton;
    [SerializeField] private AudioClip clickSE; // InspectorでSEを設定
    [SerializeField] private Button returnToLabSceneButton;

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
    
        // ラボシーンへ移動するボタン
        returnToLabSceneButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE(clickSE);
            GameManager.Instance.GoToLab();
            SettingsManager.Instance.CloseSettingsPanel();
        });
    }
}