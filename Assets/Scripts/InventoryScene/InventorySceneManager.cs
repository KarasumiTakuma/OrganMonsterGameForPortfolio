using UnityEngine;
using UnityEngine.UI;

public class InventorySceneManager : MonoBehaviour
{
    public Button backButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GameManager.Instance.GoToLab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
