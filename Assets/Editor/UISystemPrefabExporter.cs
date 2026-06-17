using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class UISystemPrefabExporter
{
    [MenuItem("Tools/UISystem/Export Prefabs from Scene")]
    public static void ExportPrefabs()
    {
        string baseFolder = "Assets/Prefabs/UISystem";
        string screensFolder = baseFolder + "/Screens";
        string panelsFolder = baseFolder + "/Panels";

        if (!AssetDatabase.IsValidFolder(baseFolder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UISystem");
        if (!AssetDatabase.IsValidFolder(screensFolder))
            AssetDatabase.CreateFolder(baseFolder, "Screens");
        if (!AssetDatabase.IsValidFolder(panelsFolder))
            AssetDatabase.CreateFolder(baseFolder, "Panels");

        SavePrefab("UIManager", baseFolder + "/UIManager.prefab");
        SavePrefab("EventSystem", baseFolder + "/EventSystem.prefab");
        SavePrefab("Canvas/SafeAreaPanel/TopBar", panelsFolder + "/TopBar.prefab");
        SavePrefab("Canvas/SafeAreaPanel/ContentArea/LobbyScreen", screensFolder + "/LobbyScreen.prefab");
        SavePrefab("Canvas/SafeAreaPanel/BottomBar", panelsFolder + "/BottomBar.prefab");
        SavePrefab("Canvas", baseFolder + "/Canvas.prefab");

        // 공통 패널: Assets/Prefabs/UI 에서 복사
        CopyExistingPrefab("Assets/Prefabs/UI/SettingsPanel.prefab", screensFolder + "/SettingsPanel.prefab");
        CopyExistingPrefab("Assets/Prefabs/UI/PausePanel.prefab", screensFolder + "/PausePanel.prefab");
        CopyExistingPrefab("Assets/Prefabs/UI/GameOverPanel.prefab", screensFolder + "/GameOverPanel.prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UISystem 프리팹 export 완료 → Assets/Prefabs/UISystem/");
    }

    private static void SavePrefab(string goPath, string prefabPath)
    {
        GameObject go = FindByPath(goPath);
        if (go == null)
        {
            Debug.LogWarning($"오브젝트를 찾을 수 없음: {goPath}");
            return;
        }

        // 저장 전 이 오브젝트가 속한 최상위 프리팹 인스턴스를 언팩
        var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
        if (outermost != null)
            PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        bool success;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath, out success);
        Debug.Log(success ? $"저장됨: {prefabPath}" : $"실패: {prefabPath}");
    }

    private static void CopyExistingPrefab(string srcPath, string dstPath)
    {
        if (!System.IO.File.Exists(srcPath.Replace("Assets/", Application.dataPath + "/")))
        {
            Debug.LogWarning($"원본 없음: {srcPath}");
            return;
        }
        AssetDatabase.CopyAsset(srcPath, dstPath);
        Debug.Log($"복사됨: {dstPath}");
    }

    private static GameObject FindByPath(string path)
    {
        string[] parts = path.Split('/');
        GameObject root = null;
        foreach (var r in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (r.name == parts[0]) { root = r; break; }
        }
        if (root == null) return null;
        if (parts.Length == 1) return root;

        Transform t = root.transform;
        for (int i = 1; i < parts.Length; i++)
        {
            t = t.Find(parts[i]);
            if (t == null) return null;
        }
        return t.gameObject;
    }
}
