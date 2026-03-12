using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    public Button goToLab;
    void Start()
    {
        goToLab.onClick.AddListener(GameManager.Instance.GoToLab);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
