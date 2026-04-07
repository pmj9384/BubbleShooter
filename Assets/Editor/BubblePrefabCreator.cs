using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class BubblePrefabCreator
{
    static BubblePrefabCreator()
    {
        if (!AssetDatabase.AssetPathExists("Assets/Prefabs/Bubble.prefab"))
            CreateBubblePrefab();
    }

    [MenuItem("Tools/Create Bubble Prefab")]
    public static void CreateBubblePrefab()
    {
        // 버블 오브젝트 생성
        var go = new GameObject("Bubble");

        // SpriteRenderer - 기본 원형 스프라이트
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.white;
        go.transform.localScale = Vector3.one;

        // CircleCollider2D
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.48f;

        // Bubble 스크립트
        go.AddComponent<Bubble>();

        // 태그
        go.tag = "Bubble";

        // 프리팹 저장
        string path = "Assets/Prefabs/Bubble.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        AssetDatabase.Refresh();
        Debug.Log("Bubble 프리팹 생성 완료: " + path);
    }
}
