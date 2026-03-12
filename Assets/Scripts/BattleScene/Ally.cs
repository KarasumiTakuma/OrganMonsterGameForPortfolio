using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 味方モンスター1体を表すクラス。
/// Monster クラスを継承し、
/// バトルシーンにおける味方キャラクターの表示・基本ステータスを管理する。
/// HPは Monster クラスが保持する maxHP / currentHP を使用するが、
/// 実際の戦闘ロジック上のHP管理は AllyAreaManager による「共有HP」で行われる。
/// 本クラスは主に表示・演出用の役割を担う。
/// </summary>
public class Ally : Monster
{

    /// <summary>
    /// 味方モンスターの初期化処理。
    /// AllyMonsterData に定義されたステータス情報をもとに、
    /// Monster 基底クラスの共通初期化処理を呼び出して、
    /// 表示用ステータス（名前・最大HP・攻撃力・画像）を設定する。
    /// ※ このメソッドでは個別のHP管理ロジックは行わない。
    /// 実際のHP増減は AllyAreaManager 側の共有HPシステムに委ねられる。
    /// </summary>
    /// <param name="allyMonsterData">
    /// 初期化に使用する味方モンスターデータ。
    /// 名前・最大HP・攻撃力・アイコンなどの静的データを保持している。
    /// </param>
    public void InitializeFromData(AllyMonsterData allyMonsterData)
    {
        InitializeBase(
            allyMonsterData.GetID(),
            allyMonsterData.GetName(),
            allyMonsterData.GetMaxHP(),
            allyMonsterData.GetAttackPower(),
            allyMonsterData.GetIcon()
        );
    }

}
