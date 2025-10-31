using UnityEngine;
using UnityEngine.UI;

// シーン内の「設定ボタン」にアタッチする
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