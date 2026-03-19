using UnityEngine;

/// <summary>
/// BGMをSEを再生するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioSource bgmAudioSource; // BGM再生プレイヤー
    [SerializeField] private AudioSource seAudioSource;  // SE再生プレイヤー
    [SerializeField] private AudioSource loopSeSource1; // 途中で停止できるSE
    [SerializeField] private AudioSource loopSeSource2; // 重ねるエフェクト用

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
        if (clip == null || seAudioSource == null) return; // 再生するクリップやSE再生プレイヤーがセットされていないなら何もしない

            // PlayOneShotで、音を重ねて再生する
            seAudioSource.PlayOneShot(clip); // 曲をセットして再生まで一度に
    }

    // 途中で止められるSE1（ループSE）を再生する
    public void PlayLoopSE1(AudioClip clip, float volumeScale = 0.6f)
    {
        if (clip == null) return;

        loopSeSource1.clip = clip;
        loopSeSource1.loop = true; // ループさせる場合
        loopSeSource1.volume = volumeScale;
        loopSeSource1.Play();
    }
    // ループSE1を止める
    public void StopLoopSE1()
    {
        loopSeSource1.Stop();
        loopSeSource1.clip = null;
    }
    // LoopSE 2 (重ねる音)
    public void PlayLoopSE2(AudioClip clip)
    {
        loopSeSource2.clip = clip;
        loopSeSource2.loop = true;
        loopSeSource2.Play();
    }
    // SE2を止める
    public void StopLoopSE2()
    {
        loopSeSource2.Stop();
    }
    
    // 両方止める便利メソッド
    public void StopAllLoopSE()
    {
        StopLoopSE1();
        StopLoopSE2();
    }
}