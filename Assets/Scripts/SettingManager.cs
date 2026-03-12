using UnityEngine;
using UnityEngine.Audio; // AudioMixerを使うために必要
using UnityEngine.SceneManagement;

/// <summary>
/// 設定の管理を構成するクラス
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static SettingsManager Instance { get; private set; }
    [SerializeField] private AudioMixer mainMixer; // InspectorでAudioMixerアセットを設定

    [SerializeField] private GameObject settingsPanelPrefab; // 通常の設定パネルのプレハブ
    [SerializeField] private GameObject settingsPanelPrefabForBattle; // 戦闘用の設定パネル(BattleSceneやStageSelectSceneでの設定パネル)のプレハブ
    private GameObject currentSettingsPanel; // 生成したパネル

    // PlayerPrefsのキー
    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SE_VOLUME_KEY = "SeVolume";
    private const string TARGETING_MODE_KEY = "TargetingMode";

    // 現在の音量（デフォルトは1.0 = Max）
    public float BgmVolume { get; private set; } = 1.0f;
    public float SeVolume { get; private set; } = 1.0f;

    // 現在のターゲットモード(デフォルトは、DragAutoTarget)
    public TargetingMode CurrentTargetingMode { get; private set; } = TargetingMode.DragAutoTarget;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSettings(); // 起動時に保存された設定を読み込む
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Awake()で読み込んだ値を、ここでMixerに適用する
        // Start()はAwake()の後に呼ばれるため、Mixerが初期化された後になる
        SetBgmVolume(BgmVolume);
        SetSeVolume(SeVolume);
    }


    /// <summary>
    /// 設定パネルを開く
    /// </summary>
    public void OpenSettingsPanel()
    {

        if (currentSettingsPanel == null)
        {
            Canvas mainCanvas = FindAnyObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("設定パネルを表示するためのCanvasが見つかりません！");
                return;
            }

            // GameManagerから、現在アクティブなシーンを取得する
            Scene currentActiveScene = GameManager.Instance.GetCurrentActiveScene();

            // アクティブなシーンが「BattleScene」や「StageSelectScene」なら、
            if (currentActiveScene.name == "BattleScene" || currentActiveScene.name == "StageSelectScene")
            {
                // 戦闘用の設定パネルをCanvasに用意
                currentSettingsPanel = Instantiate(settingsPanelPrefabForBattle, mainCanvas.transform);
            }
            else //それ以外のシーンなら、
            {
                // 通常の設定パネルをCanvasに用意
                currentSettingsPanel = Instantiate(settingsPanelPrefab, mainCanvas.transform);
            }
            Time.timeScale = 0f; // ゲームを一時停止
        }
    }

    /// <summary>
    /// 設定パネルを閉じる（パネル自身から呼ばれる）
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (currentSettingsPanel != null)
        {
            SaveSettings(); // 閉じる前に保存
            Time.timeScale = 1f; // ゲームを再開
            Destroy(currentSettingsPanel);
            currentSettingsPanel = null;
        }
    }

    /// <summary>
    /// BGM音量を設定する（スライダーから呼ばれる）
    /// </summary>
    public void SetBgmVolume(float volume_0_to_1)
    {
        // スライダーから読み込む0~1の値
        BgmVolume = volume_0_to_1;

        // 人間の耳の感覚に合わせるため、値を3乗カーブさせる
        float targetVolume = Mathf.Pow(volume_0_to_1, 3.0f);

        // スライダーが0（一番左）の時は、-80dB（ほぼ無音）を設定
        float volume_dB = (volume_0_to_1 == 0) ? -80f : Mathf.Log10(targetVolume) * 20;

        // Clampedしておくと、万が一プラスの値になるのを防げて安全
        volume_dB = Mathf.Clamp(volume_dB, -80f, 0f);

        // Mixerの "BgmVolume" という名前のパラメータを変更
        mainMixer.SetFloat("BgmVolume", volume_dB);
    }

    /// <summary>
    /// SE音量を設定する（スライダーから呼ばれる）
    /// </summary>
    public void SetSeVolume(float volume_0_to_1)
    {
        SeVolume = volume_0_to_1;

        // 人間の耳の感覚に合わせるため、値を3乗カーブさせる
        float targetVolume = Mathf.Pow(volume_0_to_1, 3.0f);

        // スライダーが0（一番左）の時は、-80dB（ほぼ無音）を設定
        float volume_dB = (volume_0_to_1 == 0) ? -80f : Mathf.Log10(targetVolume) * 20;

        // Clampedしておくと、万が一プラスの値になるのを防げて安全
        volume_dB = Mathf.Clamp(volume_dB, -80f, 0f);

        // Mixerの "SeVolume" という名前のパラメータを変更
        mainMixer.SetFloat("SeVolume", volume_dB);
    }

    /// <summary>
    /// ターゲット選択モードの切り替え用メソッド
    /// </summary>
    public void SetTargetingMode(TargetingMode targetingMode)
    {
        CurrentTargetingMode = targetingMode;
    }

    /// <summary>
    /// 現在の設定をPlayerPrefsに保存する
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BgmVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SeVolume);

        PlayerPrefs.SetInt(TARGETING_MODE_KEY, (int)CurrentTargetingMode);

        PlayerPrefs.Save();
        Debug.Log("設定を保存しました。");
    }

    /// <summary>
    /// PlayerPrefsから設定を読み込み、Mixerに適用する
    /// </summary>
    public void LoadSettings()
    {
        // PlayerPrefsからキーをもとに値をロード (デフォルトは1.0 = Max)
        BgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        SeVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1.0f);

        // デフォルトのターゲット選択方式はドラッグ自動ターゲット型
        CurrentTargetingMode = (TargetingMode)PlayerPrefs.GetInt(
            TARGETING_MODE_KEY, (int)TargetingMode.DragAutoTarget
        );
    }


    // タイトルへ戻る処理
    public void ReturnToTitle()
    {
        SaveSettings();
        Time.timeScale = 1f;

        if (currentSettingsPanel != null)
        {
            Destroy(currentSettingsPanel);
            currentSettingsPanel = null;
        }

        GameManager.Instance.GoToTitle();
    }
}

// バトル時のターゲット選択方式の種類
public enum TargetingMode
{
    ClickedTarget,   // 敵クリック選択型
    DragAutoTarget   // ドラッグ自動ターゲット型
}