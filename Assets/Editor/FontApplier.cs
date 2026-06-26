using TMPro;
using UnityEditor;
using UnityEngine;

public class FontApplier
{
    [MenuItem("Tools/Apply Kostar Font to All TMP")]
    public static void ApplyKostarFont()
    {
        string[] guids = AssetDatabase.FindAssets("Kostar SDF 2", new[] { "Assets/Font" });
        if (guids.Length == 0)
        {
            Debug.LogError("Kostar SDF 2 폰트를 찾을 수 없습니다.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);

        var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var tmp in allTMP)
        {
            Undo.RecordObject(tmp, "Apply Kostar Font");
            tmp.font = font;
            tmp.enabled = true;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        Debug.Log($"Kostar SDF 2 폰트를 {count}개 TMP 컴포넌트에 적용했습니다.");
    }

    [MenuItem("Tools/Setup ScoreText")]
    public static void SetupScoreText()
    {
        var hudGo = GameObject.Find("HUD");
        if (hudGo == null) { Debug.LogError("HUD not found"); return; }

        var hudPanel = hudGo.GetComponent<HUDPanel>();
        if (hudPanel == null) { Debug.LogError("HUDPanel not found"); return; }

        var scoreGo = GameObject.Find("ScoreText");
        if (scoreGo == null) { Debug.LogError("ScoreText not found"); return; }

        var tmp = scoreGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogError("TextMeshProUGUI not found on ScoreText"); return; }

        var so = new SerializedObject(hudPanel);
        so.FindProperty("scoreText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hudPanel);

        Debug.Log("HUDPanel.scoreText → ScoreText 연결 완료");
    }
}
