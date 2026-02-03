using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenHealEffect : MonoBehaviour
{
    [SerializeField] private Image healEffectImage;  // 回復エフェクトのImage
    [SerializeField] private float fadeTime = 0.5f;  // フェードイン / フェードアウトにかかる時間

    private const float healEffectImageAlphaMin = 0f;  // Imageが完全に透明
    private const float healEffectImageAlphaMax = 0.4f; // 半透明程度

    private Coroutine effectCoroutine;

    // 回復エフェクト再生用の外部呼び出しメソッド
    public void PlayHealEffect()
    {
        // すでに実行中のエフェクトがあるなら止める
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(HealEffect());
    }

    // 回復エフェクトのフェードイン/アウトのアニメーションの動きを滑らかに処理するCoroutine
    private IEnumerator HealEffect()
    {
        Color color = healEffectImage.color;  // 回復エフェクトImageの色情報を取得

        // 回復エフェクトのフェードイン処理
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            // エフェクトイメージのalpha値(透明度)を0 ~ 0.4に滑らかに変化させていく
            // 前フレームからの経過割合 t / fadeTime (0 ~ 1)に応じて透明度を変化させる
            float alpha = Mathf.Lerp(healEffectImageAlphaMin, healEffectImageAlphaMax, t / fadeTime);
            healEffectImage.color = new Color(color.r, color.g, color.b, alpha);  // 元のRGB色はそのまま使い、透明度(alpha)だけを変化させる
            yield return null;  // 1フレーム待機
        }

        // フェードアウト処理
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            // エフェクトイメージのalpha値(透明度)を0.4~0に滑らかに変化させていく

            float alpha = Mathf.Lerp(healEffectImageAlphaMax, healEffectImageAlphaMin, t / fadeTime);
            healEffectImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }
}
