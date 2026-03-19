using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RarityStarDisplay : MonoBehaviour
{
    // Inspectorで、並べた5つの星(Image)をここに設定
    public List<Image> starImages;

    /// <summary>
    /// レアリティ（1～5）に応じて星の表示を更新します
    /// </summary>
    public void SetRarity(int rarity)
    {
        // 5つの星を順番にチェック
        for (int i = 0; i < starImages.Count; i++)
        {
            if (i < rarity)
            {
                // レアリティの数だけ星をONにする
                starImages[i].gameObject.SetActive(true);
            }
            else
            {
                // 残りはOFFにする
                starImages[i].gameObject.SetActive(false);
            }
        }
    }
}