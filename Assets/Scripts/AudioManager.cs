using UnityEngine;

/// <summary>
/// BGMをSEを再生するクラス
/// </summary>
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
            // 2つ目のAudioManagerが見つかった場合は破壊
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return; // 再生するクリップがないなら何もしない
        
        // もし、再生したいBGMが既に再生中なら、何もしない
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
        {
            return;
        }

        bgmAudioSource.clip = clip; // BGMプレイヤーに曲をセット
        bgmAudioSource.loop = true; // BGMをループ
        bgmAudioSource.Play(); // 曲を再生する
    }

    // SE再生用のメソッドを新しく追加
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            // PlayOneShotで、音を重ねて再生する
            seAudioSource.PlayOneShot(clip); // 曲をセットして再生まで一度に
        }
    }
}