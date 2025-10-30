using UnityEngine; // Spriteを使うために必要

/// <summary>
/// UIに表示可能なアイテムが、共通して持つべき機能を定義するインターフェース。
/// </summary>
public interface IDisplayable
{
    int GetID();
    Sprite GetIcon();
    string GetName();
    int GetCount();
}