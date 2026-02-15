using System.Collections.Generic;
using UnityEngine;

public class MonsterAreaManager : MonoBehaviour
{
    [Header("Monster Settings")]
    [SerializeField] protected GameObject monsterPrefab;      // 敵・味方(モンスター)共通のPrefab
    [SerializeField] protected Transform[] spawnPoints;       // モンスターのスポーン位置
    protected List<Monster> spawnedMonsters = new List<Monster>(); // 生成したモンスターリスト

    // モンスターをスポーンさせる
    protected virtual void SpawnMonsters(List<MonsterData> monsterDataList)
    {
        // 既存のモンスターがいたら削除
        ClearMonsters();

        int dataIndex = 0; // monsterDataList 参照用カウンタ

        // 各スポーンポイントにモンスターを生成
        foreach (var point in spawnPoints)
        {
            // モンスターのスポーン位置数に対して、表示したいモンスター数が少ない場合に
            // ないはずのモンスターデータ(EnemyMonsterDataやAllyMonsterDataなど)にアクセスしないようにする
            if (dataIndex >= monsterDataList.Count) break;

            // monsterObjはそのスポーン位置(point)を親とする
            GameObject monsterObj = Instantiate(monsterPrefab, point);

            // monsterObjオブジェとそのスポーンポイント(point)のRectTransformを取得
            // RectTransformを調整するため（UI用）
            RectTransform monsterRect = monsterObj.GetComponent<RectTransform>();
            RectTransform pointRect = point.GetComponent<RectTransform>();

            if (monsterRect != null && pointRect != null)  //RectTransformが空値でないかを確認
            {
                //Inspector上で各オブジェクトのサイズ設定を誤っても正しいサイズにするため

                // モンスターオブジェクトのサイズをspawnPointに合わせる
                monsterRect.sizeDelta = pointRect.sizeDelta;

                // 各モンスターオブジェクトの位置をそれぞれの親であるpointのpivot位置(中心)にする
                monsterRect.anchoredPosition = Vector2.zero;

                // モンスターのサイズは1に固定
                monsterRect.localScale = Vector3.one;
            }

            // monsterObjのmonsterPrefabからMonsterスクリプト(モンスターの初期設定状態)を取得
            Monster monster = monsterObj.GetComponent<Monster>();

            // monsterがEnemyクラスのオブジェクトかAllyクラスのオブジェクトかで場合分け
            // MonsterクラスのInitializeBaseメソッドはprotectedで外部からアクセスできないので、
            // EnemyまたはAllyInitializeSetメソッドを用いる必要があるから
            if (monster is Enemy enemy) // monsterオブジェクトがEnemyクラスなら
            {
                // monsterDataList[dataIndex]を敵モンスターのデータ(Enemyクラス専用データ：EnemyMonsterData)
                // として、InitializeSetメソッドを呼び出して初期化
                enemy.InitializeSet(monsterDataList[dataIndex] as EnemyMonsterData);
            }
            else if (monster is Ally ally)
            {
                // monsterDataList[dataIndex]を味方モンスターのデータ(Allyクラス専用データ：AllyMonsterData)
                // として、InitializeSetメソッドを呼び出して初期化
                ally.InitializeSet(monsterDataList[dataIndex] as AllyMonsterData);
            }

            spawnedMonsters.Add(monster);
            dataIndex++;
        }
    }

    // EnemyAreaManagerクラスの場合、生成したモンスターが攻撃を受けた際にMonsterクラスのTakeDamageメソッドを呼び出して
    // そのモンスターへのダメージ処理をするメソッド。AllyAreaManagerクラスの場合は、共有HPへのダメージ処理
    // ↑コメント変更予定
    protected virtual void ApplyDamage(int index, int damage)
    {

    }

    // 生成した各モンスターの各々に対してMonsterクラスのTakeDamageメソッドを呼び出して、
    // 全体攻撃によるダメージ処理をするメソッド(味方側Allyは共有HPであるので、そのまま共有HPにダメージを与える)
    protected virtual void ApplyDamageToAll(int damage)
    {
        
    }


    // isAliveメソッドを追加予定

    // 残りモンスターの数をカウントするメソッド。返り値は生成済みモンスターリストspawnedMonstersの要素数
    protected virtual int GetMonsterCount() => spawnedMonsters.Count;

    protected virtual void ClearMonsters()
    {
        foreach (var monster in spawnedMonsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        spawnedMonsters.Clear();
    }

    // 戦闘ログにメッセージを追加するメソッド。メッセージのタイプも引数として与えること。

    protected void Log(string message, BattleLogType type)
    {
        // シングルトンインスタンスであるBattleLogManagerインスタンスに追加したいログを送る
        BattleLogManager.Instance.AddLog(message, type);  
        Debug.Log(message);  // デバッグログとしても表示する
    }
}
