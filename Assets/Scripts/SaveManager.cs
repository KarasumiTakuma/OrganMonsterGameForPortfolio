using UnityEngine;
using System.IO; // ファイル操作に必須

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string saveFilePath; // セーブファイルの完全パス
    private const string SAVE_KEY = "MyGameSaveData"; // PlayerPrefs用のキー

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 親(GameManager)がDontDestroyOnLoadなので自分も消えない
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// セーブデータのキーを持っているかどうかを返す
    /// </summary>
    /// <returns>セーブデータを持っているか否か</returns>
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    /// <button>
    /// ゲームをセーブする
    /// </button>
    public void SaveGame()
    {
        // 1. PlayerDataからセーブ用のデータを生成
        SaveData saveData = GameManager.Instance.PlayerData.CreateSaveData();

        // 2. SaveDataオブジェクトをJSON形式の文字列に変換
        string jsonString = JsonUtility.ToJson(saveData);

        // 3. PlayerPrefsにJSON文字列を丸ごと保存
        PlayerPrefs.SetString(SAVE_KEY, jsonString);
        PlayerPrefs.Save();
        Debug.Log("セーブ完了 (PlayerPrefs)");
    }

    /// <button>
    /// ゲームをロードする
    /// </button>
    public void LoadGame()
    {
        // PlayerPrefsにキーが存在するか確認
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            // PlayerPrefsからJSON文字列を読み込む
            string jsonString = PlayerPrefs.GetString(SAVE_KEY);

            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonString);
            GameManager.Instance.PlayerData.LoadFromSaveData(saveData);
            Debug.Log("ロード完了 (PlayerPrefs)");
        }
    }

    // セーブデータの削除およびメモリ上のゲームデータの初期化用メソッド
    public void DeleteSaveData()
    {
        // PlayerPrefsが持つキーを削除して、セーブデータを削除する
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();

            // ゲームデータもリセット
            GameManager.Instance.PlayerData.ResetData();

            Debug.Log("セーブデータを削除＆ゲームデータを初期化しました");
        }
        else
        {
            Debug.Log("削除対象のセーブデータは存在しません。ゲームデータのみ、初期化しました");
            GameManager.Instance.PlayerData.ResetData();
        }
    }
}