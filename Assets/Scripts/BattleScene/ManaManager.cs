using UnityEngine;
using TMPro;

public class ManaManager : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private int maxMana = 10;   // 到達できる最大マナ
    [SerializeField] private int currentMana;    // 現在のマナ(残り使用可能なマナ数)
    [SerializeField] private TMP_Text manaText;  // マナを表示するUI

    private int currentMaxMana; // 今のターンの最大マナ

    private int turnCount = 0; // 現在のターン数

    // ターン開始時
    public void StartTurn()
    {
        turnCount++;

        // ターン数に応じて最大マナを増やす
        currentMaxMana = Mathf.Min(turnCount, maxMana);

        // 今ターンのマナを全回復
        currentMana = currentMaxMana;

        UpdateManaUI();
    }

    // マナを使用
    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    // マナUI更新
    private void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = this.GetCurrentMana().ToString() + " / " + currentMaxMana;
        }
    }

    // 現在のマナ取得
    public int GetCurrentMana() => currentMana;
}
