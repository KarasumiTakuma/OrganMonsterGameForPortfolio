using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// タイトルシーンを管理するクラス。
/// 
/// ・「はじめから」「続きから」ボタンの制御
/// ・セーブデータの有無によるUI状態変更
/// ・ボタン押下時のゲーム開始処理の分岐
/// を担当する。
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    /// <summary>「はじめから」ボタン。新規ゲーム開始処理を実行する</summary>
    [SerializeField] private Button startNewButton;

    /// <summary>続きから」ボタン。セーブデータが存在する場合のみ有効化される</summary>
    [SerializeField] private Button continueButton;

    /// <summary>ボタン押下時に再生するクリックSE</summary>
    [SerializeField] private AudioClip clickSE;

    /// <summary>「続きから」ボタンの見た目制御用CanvasGroup。セーブデータが存在しない場合、透明度や入力可否を変更する</summary>
    [SerializeField] private CanvasGroup continueButtonCanvasGroup;


    /// <summary>
    /// 初期化処理。
    /// ボタンイベントの登録と、セーブデータ有無に応じたUI状態を設定する。
    /// </summary>
    private void Start()
    {
        // ボタン押下時のイベント登録
        startNewButton.onClick.AddListener(OnStartNewGame);
        continueButton.onClick.AddListener(OnContinueGame);

        // セーブデータが存在するか確認
        bool hasSaveData = SaveManager.Instance.HasSaveData();

        // セーブデータの存在確認
        continueButton.interactable = hasSaveData;

        // 見た目の制御
        continueButtonCanvasGroup.alpha = hasSaveData ? 1.0f : 0.4f;
        continueButtonCanvasGroup.blocksRaycasts = hasSaveData;
    }

    /// <summary>
    /// 「はじめから」ボタン押下時の処理。
    /// 既存セーブを削除し、新規ゲームを開始する。
    /// </summary>
    private void OnStartNewGame()
    {
        AudioManager.Instance.PlaySE(clickSE);
        SaveManager.Instance.DeleteSaveData();
        GameManager.Instance.StartNewGame();
        GameManager.Instance.GoToLab();
    }

    /// <summary>
    /// 「続きから」ボタン押下時の処理。
    /// セーブデータを読み込み、ゲームを再開する。
    /// </summary>
    private void OnContinueGame()
    {
        AudioManager.Instance.PlaySE(clickSE);
        SaveManager.Instance.LoadGame();
        GameManager.Instance.GoToLab();
    }
}