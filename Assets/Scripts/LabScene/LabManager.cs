using UnityEngine;
using UnityEngine.UI;

public class LabManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button goToButtle;
    // 合成
    public Button goToSynthesis;
    // 錬成
    public Button goToStudy;
    // ショップ
    public Button goToShop;
    public Button goToInventry;
    public Button goToHistory;
    public Button goToPartyEdit;

    void Start()
    {
        goToButtle.onClick.AddListener(GameManager.Instance.GoToBattle);
        goToSynthesis.onClick.AddListener(GameManager.Instance.GoToSynthesis);
        goToShop.onClick.AddListener(GameManager.Instance.GoToShop);
        goToStudy.onClick.AddListener(GameManager.Instance.GoToStudy);
        goToInventry.onClick.AddListener(GameManager.Instance.GoToInventory);
        goToHistory.onClick.AddListener(GameManager.Instance.GoToHistory);
        goToPartyEdit.onClick.AddListener(GameManager.Instance.GoToPartyEdit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
