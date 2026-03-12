using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class EditorUtils
{

    public static List<T> LoadAllAssets<T>() where T : ScriptableObject
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        // 名前順でソートして返す
        return assets.OrderBy(a => a.name).ToList();
    }

    // assetを削除する
    public static void DeleteAsset(ScriptableObject asset)
    {
        if (asset == null) return;
        string path = AssetDatabase.GetAssetPath(asset);
        AssetDatabase.DeleteAsset(path);
        // Unityエディタに変更を認織させる
        AssetDatabase.Refresh();
    }
}
