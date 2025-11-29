using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタンにアタッチしてクリック音を鳴らす
/// </summary>
[RequireComponent(typeof(Button))] // Buttonコンポーネントが必要(ない場合自動でアタッチされる) 
public class PlaySoundButton : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound; // 鳴らしたい音

    void Start()
    {
        Button btn = GetComponent<Button>();
        // ボタンが押されたら、AudioManagerに音を鳴らすよう依頼する
        btn.onClick.AddListener(() => AudioManager.Instance.PlaySE(clickSound));
    }
}