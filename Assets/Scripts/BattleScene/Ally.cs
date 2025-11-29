using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 味方1体の情報を管理するスクリプト
/// 画像・名前・攻撃力・最大HPなどの基本情報を管理
/// 個別HPは表示用で、実際のHP管理は AllyAreaManager の共有HPで行う
/// </summary>
public class Ally : Monster
{

    // 初期設定用メソッド
    // AllyMonsterData で初期化
    public void InitializeSet(AllyMonsterData allyMonsterData)
    {
        InitializeBase(
            allyMonsterData.GetID(),
            allyMonsterData.GetName(),
            allyMonsterData.GetHP(),
            allyMonsterData.GetAttackPower(),
            allyMonsterData.GetIcon()
        );


    }

    public void AllyPlayDamageEffect()
    {
        PlayDamageEffect();  // 画像を赤くする
    }

}
