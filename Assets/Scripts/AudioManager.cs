using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmAudioSource; // BGM再生プレイヤー
    [SerializeField] private AudioSource seAudioSource;  // SE再生プレイヤー

    private void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            // 親がDontDestroyOnLoadなので自分も消えない
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmAudioSource.clip = clip; // BGMプレイヤーに曲をセット
        bgmAudioSource.Play(); // 曲を再生する
    }

    // ★★★ SE再生用のメソッドを新しく追加 ★★★
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            // PlayOneShotで、音を重ねて再生する
            seAudioSource.PlayOneShot(clip); // 曲をセットして再生まで一度に
        }
    }
}