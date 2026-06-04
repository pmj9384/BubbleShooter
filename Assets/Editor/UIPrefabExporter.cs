using UnityEditor;
using UnityEngine;

public class UIPrefabExporter
{
    [MenuItem("Tools/Export UI Prefabs")]
    public static void ExportUIPrefabs()
    {
        string[] targets = new[]
        {
            "UIManager/SafeAreaPanel/GameOverPanel",
            "UIManager/SafeAreaPanel/PausePanel",
            "UIManager/SafeAreaPanel/SettingsPanel",
        };

        string savePath = "Assets/Prefabs/UI";
        if (!System.IO.Directory.Exists(savePath))
            System.IO.Directory.CreateDirectory(savePath);

        foreach (var path in targets)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Debug.LogWarning($"[UIPrefabExporter] 오브젝트를 찾을 수 없습니다: {path}");
                continue;
            }

            string prefabPath = $"{savePath}/{go.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Debug.Log($"[UIPrefabExporter] 프리팹 저장: {prefabPath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("[UIPrefabExporter] 완료");
    }
}
