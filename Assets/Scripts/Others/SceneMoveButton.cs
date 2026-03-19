using UnityEngine;

public class SceneMoveButton : MonoBehaviour
{
    public void GoToLab()
    {
        GameManager.Instance.GoToLab();
    }

    public void GoToStageSelect()
    {
        GameManager.Instance.GoToStageSelectScene();
    }

    public void GoToTitle()
    {
        GameManager.Instance.GoToTitle();
    }
}