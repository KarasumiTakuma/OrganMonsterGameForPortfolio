using UnityEngine;
using TMPro;

/// <summary>
/// マナの管理を行うクラス。
/// ターン制カードゲームを想定し、
/// ・ターンごとの最大マナ増加
/// ・現在使用可能なマナの管理
/// ・マナUIの更新
/// を担当する。
/// </summary>
public class ManaManager : MonoBehaviour
{
    [Header("Mana Settings")]

    /// <summary>到達可能な最大マナ量。ターン数が増えても、この値を上限として制限される</summary>
    [SerializeField] private int maxMana = 10;

    /// <summary>マナ残量を表示するUIテキスト。「現在マナ / 今ターンの最大マナ」の形式で表示する</summary>
    [SerializeField] private TMP_Text manaText;


    /// <summary>現在使用可能なマナ量。カード使用などで消費される</summary>
    private int currentMana;

    /// <summary>今ターンにおける最大マナ量。 ターン数に応じて増加し、maxMana を超えない</summary>
    private int turnMaxMana;

    /// <summary>現在のターン数。ターン開始時にインクリメントされ、マナ計算に使用される</summary>
    private int turnCount = 0;

    /// <summary>
    /// ターン開始時に呼ばれる処理。
    /// ターン数を進め、最大マナを更新し、
    /// 今ターンのマナを全回復する。
    /// </summary>
    public void StartTurn()
    {
        turnCount++;

        // ターン数に応じて最大マナを増やす
        turnMaxMana = Mathf.Min(turnCount, maxMana);

        // 今ターンのマナを全回復
        currentMana = turnMaxMana;

        UpdateManaUI();
    }

    /// <summary>
    /// 指定量のマナを消費する。
    /// マナが足りない場合は消費せず、falseを返す。
    /// </summary>
    /// <param name="amount">消費したいマナ量</param>
    /// <returns>
    /// マナを消費できた場合は true、足りない場合は false
    /// </returns>
    public bool UseMana(int amount)
    {
        if (currentMana < amount) return false;

        currentMana -= amount;
        UpdateManaUI();
        return true;
    }

    /// <summary>
    /// マナ関連の状態を初期化する。
    /// バトル開始時やフェーズ切り替え時に使用される想定。
    /// </summary>
    public void ResetMana()
    {
        turnCount = 0;
        currentMana = 0;
        turnMaxMana = 0;
        UpdateManaUI();
    }

    /// <summary>
    /// マナUIを更新する。
    /// 現在のマナと今ターンの最大マナを表示する。
    /// </summary>
    private void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = this.GetCurrentMana().ToString() + " / " + turnMaxMana;
        }
    }

    /// <summary>
    /// 現在使用可能なマナ量を取得する。
    /// </summary>
    /// <returns>現在のマナ量</returns>
    public int GetCurrentMana() => currentMana;
}