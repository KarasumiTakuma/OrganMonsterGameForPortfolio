using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// モンスター配置エリアの管理クラス。
/// モンスターの生成・削除・共通処理(攻撃を与えるなど)を提供する。
/// EnemyAreaManager / AllyAreaManager管理クラスの親クラスとして機能する。
/// </summary>
public class MonsterAreaManager : MonoBehaviour
{
    [Header("Monster Settings")]

    /// <summary>
    /// 生成に使用するモンスター共通Prefab。EnemyAreaManager / AllyAreaManagerの
    /// どちらもこのPrefabを使用する。
    /// </summary>
    [SerializeField] protected GameObject monsterPrefab;

    /// <summary> モンスターの生成位置。UIレイアウト上のスロットや配置ポイントを想定。</summary>
    [SerializeField] protected Transform[] spawnPoints;

    /// <summary>
    /// 現在エリア内に生成されているモンスター一覧リスト。実体の管理・参照用。
    /// </summary>
    protected List<Monster> spawnedMonsters = new List<Monster>();


    /// <summary>
    /// 指定されたモンスターデータを元にモンスターを生成する。
    /// 既存のモンスターは全て削除される。
    /// </summary>
    /// <param name="monsterDataList">
    /// 生成対象のモンスターデータ一覧。spawnPoints の順番に対応して配置される。
    /// </param>
    protected virtual void SpawnMonsters(List<MonsterData> monsterDataList)
    {
        ClearMonsters();

        int dataIndex = 0; // monsterDataList 参照用インデックス

        // 各スポーンポイントにモンスターを生成
        foreach (var point in spawnPoints)
        {

            // データ数を超えた場合は生成終了
            if (dataIndex >= monsterDataList.Count) break;

            // モンスターインスタンス生成（親をスポーンポイントに設定）
            GameObject monsterObject = Instantiate(monsterPrefab, point);

            // UI調整用RectTransformを取得
            RectTransform monsterRect = monsterObject.GetComponent<RectTransform>();
            RectTransform pointRect = point.GetComponent<RectTransform>();

            // UIサイズ・位置をスポーンポイントに合わせる
            if (monsterRect != null && pointRect != null)
            {
               
                monsterRect.sizeDelta = pointRect.sizeDelta;   // サイズを合わせる
                monsterRect.anchoredPosition = Vector2.zero;  // 中央の位置に配置
                monsterRect.localScale = Vector3.one;   // スケールを固定
            }

            Monster monster = monsterObject.GetComponent<Monster>();

            // Enemy / Ally 型別の初期化処理
            if (monster is Enemy enemy)
            {
                // 敵モンスターとして初期化
                enemy.InitializeSet(monsterDataList[dataIndex] as EnemyMonsterData);
            }
            else if (monster is Ally ally)
            {
                // 味方モンスターとして初期化
                ally.InitializeSet(monsterDataList[dataIndex] as AllyMonsterData);
            }

            // 管理リストへ登録
            spawnedMonsters.Add(monster);
            dataIndex++;
        }
    }

    /// <summary>
    /// エリア内の全モンスターに対してダメージ処理を行う。
    /// 子クラスで実装されるフック。
    /// </summary>
    /// <param name="damageAmount"> 与えるダメージ量 </param>
    protected virtual void ApplyDamageToAllMonsters(int damageAmount)
    {
        // 子クラスで処理実装
    }

    /// <summary>
    /// 生成済みモンスターを全て削除する。
    /// </summary>
    protected virtual void ClearMonsters()
    {
        foreach (var monster in spawnedMonsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        
        // 管理リスト初期化
        spawnedMonsters.Clear();
    }

    /// <summary>
    /// 戦闘ログへメッセージを送信する。
    /// </summary>
    /// <param name="message"> 表示するログメッセージ </param>
    /// <param name="type"> ログ種別（攻撃・回復・注意など） </param>
    protected void Log(string message, BattleLogType type)
    {
        BattleLogManager.Instance.AddLog(message, type);  // UIログの送信
        Debug.Log(message);  // デバッグログ表示
    }
}
