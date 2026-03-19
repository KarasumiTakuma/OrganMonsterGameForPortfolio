using UnityEngine;

public class DebugController : MonoBehaviour
{
    void Update()
    {
        // 「M」キーを押したら、研究ポイントを10000増やす
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                GameManager.Instance.PlayerData.AddPoints(10000);
                Debug.Log("デバッグ: 研究ポイントを10000追加しました。");
            }
        }

        // 「D」キーが押されたら
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.LogWarning("--- デバッグ: 全てのセーブデータと設定をリセットします ---");
            
            // 1. PlayerPrefsに保存された全データを物理的に削除
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save(); // 変更を即時保存
            
            // 2. PlayerDataの現在のデータをリセット
            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                GameManager.Instance.PlayerData.ResetData();
            }

            // 3. SettingsManagerの現在の設定をリセット（デフォルト値を再読み込みさせる）
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.LoadSettings();
            }

            Debug.Log("リセットが完了しました。");
        }
    }
}