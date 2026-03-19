using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    [SerializeField] private Button backButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(GameManager.Instance.GoToLab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
