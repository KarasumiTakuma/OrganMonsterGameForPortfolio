using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 設定ボタンにアタッチして設定パネルを開く
/// </summary>
[RequireComponent(typeof(Button))]
public class SettingsButtonLink : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        // ボタンが押されたら、シングルトンのSettingsManagerを呼び出す
        button.onClick.AddListener(() => SettingsManager.Instance.OpenSettingsPanel());
    }
}