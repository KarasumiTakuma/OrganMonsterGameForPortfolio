using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 画面全体に表示される回復エフェクトを制御するクラス。
/// 半透明のImageをフェードイン・フェードアウトさせることで、
/// 回復時の視覚的な演出をする。
/// </summary>
public class ScreenHealEffect : MonoBehaviour
{
    /// <summary>回復エフェクトとして表示する Image コンポーネント。画面全体を覆う半透明のUI画像を想定</summary>
    [SerializeField] private Image healEffectImage;

    /// <summary>フェードイン・フェードアウトにかかる時間（秒）。両方の処理で共通して使用される。</summary>
    [SerializeField] private float fadeTime = 0.5f;


    /// <summary>回復エフェクトImageの最小アルファ値（完全に透明）</summary>
    private const float healEffectImageAlphaMin = 0f;

    /// <summary>回復エフェクトImageの最大アルファ値（半透明）</summary>
    private const float healEffectImageAlphaMax = 0.4f;

    /// <summary>現在再生中の回復エフェクト用コルーチン。多重再生を防ぐために保持する</summary>
    private Coroutine effectCoroutine;

    /// <summary>
    /// 回復エフェクトを再生する。
    /// 既にエフェクトが再生中の場合は一度停止し、
    /// 最初から再生し直す。
    /// </summary>
    public void PlayHealEffect()
    {
        // すでに実行中のエフェクトがあるなら止める
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(HealEffect());
    }

    /// <summary>
    /// 回復エフェクトのフェードイン・フェードアウト処理を行うコルーチン。
    /// Image のアルファ値を時間経過に応じて補間し、
    /// なめらかな表示・非表示を実現する。
    /// </summary>
    private IEnumerator HealEffect()
    {
        Color color = healEffectImage.color;

        // フェードイン処理（透明 → 半透明）
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(healEffectImageAlphaMin, healEffectImageAlphaMax, t / fadeTime);
            healEffectImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // フェードアウト処理（半透明 → 透明）
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(healEffectImageAlphaMax, healEffectImageAlphaMin, t / fadeTime);
            healEffectImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        effectCoroutine = null;
    }
}
