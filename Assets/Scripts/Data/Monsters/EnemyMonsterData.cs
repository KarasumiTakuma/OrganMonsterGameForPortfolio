using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敵モンスター専用のデータクラス。MonsterDataを継承し、
/// 敵キャラクター向けの追加情報（主にUI表示用）を保持する。
/// このクラスはScriptableObjectとして作成される。
/// </summary>
/// <remarks>
/// 現時点のゲーム仕様では、本クラスが持つ追加情報は
/// 実際の処理では使用されていない。
/// 将来的なUI拡張（敵詳細表示、図鑑、ボス説明など）を想定して定義している。
/// </remarks>
[CreateAssetMenu(fileName = "EnemyMonsterData", menuName = "Data/EnemyMonsterData")]
public class EnemyMonsterData : MonsterData
{
     
    /// <summary>敵モンスターの詳細説明文。敵画像クリック時や、敵詳細UIでの表示を想定したテキスト。</summary>
    /// <remarks>
    /// ※ 本ゲームの現行バージョンでは未使用。
    /// 今後、敵情報表示UIや図鑑機能を追加する際に利用する想定。
    /// </remarks>
    [Header("UI用追加情報")]
    [TextArea]
    [SerializeField] private string enemyDetailDescription;

    /// <summary>敵モンスターの詳細説明文を取得する。</summary>
    /// <returns> 敵詳細表示用の説明文文字列</returns>
    public string GetEnemyDetailDescription() => enemyDetailDescription;  // descriptionを外部から取得するためのゲッター
}