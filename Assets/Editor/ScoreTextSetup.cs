using UnityEditor;
using UnityEngine;
using TMPro;

public static class ScoreTextSetup
{
    [MenuItem("Tools/Setup ScoreText")]
    public static void Run()
    {
        var hudGo = GameObject.Find("HUD");
        if (hudGo == null) { Debug.LogError("HUD not found"); return; }

        var hudPanel = hudGo.GetComponent<HUDPanel>();
        if (hudPanel == null) { Debug.LogError("HUDPanel not found"); return; }

        var scoreGo = GameObject.Find("ScoreText");
        if (scoreGo == null) { Debug.LogError("ScoreText not found"); return; }

        var tmp = scoreGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogError("TextMeshProUGUI not found on ScoreText"); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        if (font != null)
        {
            tmp.font = font;
            EditorUtility.SetDirty(scoreGo);
        }

        var so = new SerializedObject(hudPanel);
        so.FindProperty("scoreText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hudPanel);

        Debug.Log("ScoreText setup complete");
    }
}
