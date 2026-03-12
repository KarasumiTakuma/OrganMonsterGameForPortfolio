using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }

    // プレイヤーデータへの参照
    public PlayerData PlayerData { get; private set; }

    // 現在挑戦中のステージ番号
    public int currentStageIndex { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioClip mainBGM; // InspectorでメインBGMを設定
    // [SerializeField] private AudioClip battleBGM; // バトルシーンで流すBGM

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            // 自分はテスト用の仮のマネージャーなので、自分を破棄して処理を終える
            Destroy(gameObject);
            return; // returnで、これ以降のAwake処理を実行しない
        }
        // 以下は、自分が最初のGameManagerだった場合のみ実行される
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PlayerData = GetComponentInChildren<PlayerData>();
    }
    private void Start()
    {
        // 起動時にBGMの再生をAudioManagerに依頼
        if (mainBGM != null)
        {
            AudioManager.Instance.PlayBGM(mainBGM);
        }

    }

    private void OnEnable()
    {
        // シーンがロードされたら、OnSceneLoadedメソッドを呼ぶように予約
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // オブジェクトが破棄される際は、予約を解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンが新しくロードされるたびに呼び出されるメソッド
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ロードされたシーンの名前で処理を分岐
        switch (scene.name)
        {
            case "BattleScene":
                // AudioManager.Instance.PlayBGM(battleBGM);
                // BattleManagerがBGMを設定するので何もしない
                break;

            default:
                // 上記（バトルシーン）以外に当てはまらない、その他全てのシーン
                AudioManager.Instance.PlayBGM(mainBGM);
                break;
        }
    }

    /// <summary>
    /// デバッグ用。Rキーを押すとセーブデータ(およびゲームデータ)が削除される。
    /// </summary>
    //     void Update()
    //     {
    // #if UNITY_EDITOR
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         SaveManager.Instance.DeleteSaveData();
    //     }
    // #endif
    //     }

    // 新規ゲーム開始処理
    public void StartNewGame()
    {
        // 既存データを完全リセット
        PlayerData.ResetData();

        // 初期モンスターID（仕様として固定）
        int[] initialMonsterIds = { 1, 2, 3 };

        // ID → MonsterData に変換し、
        // 初期モンスター3体をPlayerDataに追加していく
        foreach (int id in initialMonsterIds)
        {
            MonsterData monster = DataManager.Instance.GetMonsterByID(id);
            if (monster == null)
            {
                Debug.LogError($"初期モンスターID {id} が見つかりません");
                continue;
            }

            // インベントリに該当の初期モンスターを追加
            PlayerData.AddMonster(monster, 1);
        }

        SaveManager.Instance.SaveGame();
    }

    // BGM再生を public メソッド化(ボス戦でBGMを変更してその後戻す時など)
    public void PlayMainBGM()
    {
        if (mainBGM != null)
        {
            AudioManager.Instance.PlayBGM(mainBGM);
        }
    }

    // --- シーン切り替え用のメソッド ---
    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void GoToLab()
    {
        SceneManager.LoadScene("LabScene");
    }
    // ショップシーン
    public void GoToShop()
    {
        SceneManager.LoadScene("ShopScene");
    }
    // モンスターインベントリシーン
    public void GoToInventory()
    {
        SceneManager.LoadScene("InventoryScene");
    }
    // 臓器インベントリシーン

    // 合成シーン
    public void GoToSynthesis()
    {
        SceneManager.LoadScene("SynthesisScene");
    }
    // 錬成シーン
    public void GoToStudy()
    {
        SceneManager.LoadScene("GachaScene");
    }

    public void GoToStageSelectScene()
    {
        // どのステージに挑戦するか、などの情報を保持してシーンをロード
        //this.currentStageIndex = stageIndex;
        SceneManager.LoadScene("StageSelectScene");
    }

    public void GoToHistory()
    {
        SceneManager.LoadScene("HistoryScene");
    }
    public void GoToPartyEdit()
    {
        SceneManager.LoadScene("PartyEditScene");
    }
    public Scene GetCurrentActiveScene() => SceneManager.GetActiveScene();  // 現在アクティブなシーン(再生中のシーン)を取得するゲットメソッド
}