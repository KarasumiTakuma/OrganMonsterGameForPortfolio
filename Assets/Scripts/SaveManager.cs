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
        
        // OSごとに異なる、安全な保存場所のパスを取得
        // 例: C:/Users/[UserName]/AppData/LocalLow/[CompanyName]/[ProductName]/savedata.json
        saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");
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
        
        // // 3. JSON文字列をファイルとしてディスクに書き込む
        // File.WriteAllText(saveFilePath, jsonString);
        // Debug.Log("セーブ完了: " + saveFilePath);

        // ファイルに書き込む代わりに、PlayerPrefsにJSON文字列を丸ごと保存
        PlayerPrefs.SetString(SAVE_KEY, jsonString);
        PlayerPrefs.Save();
        Debug.Log("セーブ完了 (PlayerPrefs)");
    }

    /// <button>
    /// ゲームをロードする
    /// </button>
    public void LoadGame()
    {
        // // 1. セーブファイルが存在するか確認
        // if (File.Exists(saveFilePath))
        // {
        //     // 2. ファイルからJSON文字列を読み込む
        //     string jsonString = File.ReadAllText(saveFilePath);

        //     // 3. JSON文字列をSaveDataオブジェクトに変換
        //     SaveData saveData = JsonUtility.FromJson<SaveData>(jsonString);

        //     // 4. PlayerDataにデータを復元させる
        //     GameManager.Instance.PlayerData.LoadFromSaveData(saveData);
        //     Debug.Log("ロード完了");
        // }
        // else
        // {
        //     Debug.LogWarning("セーブファイルが見つかりません。");
        // }

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

    /// <summary>
    /// セーブファイルを削除し、現在のゲームデータをリセットする
    /// </summary>
    // public void DeleteAndResetData()
    // {
    //     // 1. 物理ファイルを削除
    //     if (File.Exists(saveFilePath))
    //     {
    //         File.Delete(saveFilePath);
    //         Debug.Log("セーブファイルを削除しました。");
    //     }
    //     else
    //     {
    //         Debug.Log("セーブファイルは存在しませんでした。");
    //     }

    //     // 2. PlayerDataのランタイムデータをリセット
    //     GameManager.Instance.PlayerData.ResetData();
    // }
}