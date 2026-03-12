using UnityEngine;

public class CreditsUI : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private AudioClip clickSE;

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
        AudioManager.Instance.PlaySE(clickSE);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        AudioManager.Instance.PlaySE(clickSE);
    }
}