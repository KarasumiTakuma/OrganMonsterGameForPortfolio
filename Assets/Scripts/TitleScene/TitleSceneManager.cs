using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button startNewButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup continueButtonCanvasGroup;

    private void Start()
    {
        startNewButton.onClick.AddListener(OnStartNewGame);
        continueButton.onClick.AddListener(OnContinueGame);

        bool hasSaveData = SaveManager.Instance.HasSaveData();

        // セーブが無い場合は「続きから」を無効化
        continueButton.interactable = hasSaveData;

        // 見た目の制御
        continueButtonCanvasGroup.alpha = hasSaveData ? 1.0f : 0.4f;
        continueButtonCanvasGroup.blocksRaycasts = hasSaveData;
    }

    // はじめから
    private void OnStartNewGame()
    {
        SaveManager.Instance.DeleteSaveData();
        GameManager.Instance.StartNewGame();
        GameManager.Instance.GoToLab();
    }

    // 続きから
    private void OnContinueGame()
    {
        SaveManager.Instance.LoadGame();
        GameManager.Instance.GoToLab();
    }
}