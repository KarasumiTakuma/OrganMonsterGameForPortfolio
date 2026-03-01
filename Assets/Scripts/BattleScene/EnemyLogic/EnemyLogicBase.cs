using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の行動ロジックの基底クラス。
/// ScriptableObjectとしてアセット化し、敵データにアタッチして使用する。
/// ストラテジーパターンで、異なる行動パターンを持つ複数のEnemyLogicクラスを作成できるようにする。
/// </summary>
public abstract class EnemyLogicBase : ScriptableObject
{
    /// <summary>
    /// 敵のターンに行う行動を定義するメソッド。
    /// BattleManagerからコルーチンとして実行される。
    /// </summary>
    /// <param name="self">行動する敵自身のコントローラー</param>
    /// <param name="allyManager">プレイヤー（味方）側の管理マネージャー（攻撃対象）</param>
    /// <param name="enemyManager">敵側の管理マネージャー（味方へのバフ/回復用）</param>
    public abstract IEnumerator ExecuteTurn(Enemy self, AllyAreaManager allyManager, EnemyAreaManager enemyManager);
}