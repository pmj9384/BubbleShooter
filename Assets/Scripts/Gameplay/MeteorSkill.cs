using System;
using System.Collections;
using UnityEngine;

public class MeteorSkill : MonoBehaviour, IBubbleSkill
{
    public BubbleType TargetType => BubbleType.Meteor;
    public bool UsesCustomFire => true;

    public void CustomFire(Vector2 screenPos, BubbleGrid grid, Action onComplete)
    {
        Camera cam = Camera.main;
        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        float topY = grid.GetWorldPosition(0, 0).y;
        var (_, col) = grid.GetGridPosition(new Vector2(worldPos.x, topY));

        StartCoroutine(FallCoroutine(col, grid, onComplete));
    }

    public void OnLand(BubbleGrid grid, int row, int col) { }

    private IEnumerator FallCoroutine(int col, BubbleGrid grid, Action onComplete)
    {
        Vector2 startPos = (Vector2)grid.GetWorldPosition(0, col) + Vector2.up * 4f;
        Vector2 endPos = grid.GetWorldPosition(0, col);

        var visual = new GameObject("MeteorVisual");
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = Bubble.GetMeteorSprite();
        sr.sortingOrder = 10;
        visual.transform.position = startPos;

        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            visual.transform.position = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        Destroy(visual);

        for (int r = 0; r < BubbleGrid.MAX_ROWS; r++)
            if (col <= grid.GetMaxCol(r))
                grid.RemoveBubble(r, col);

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);

        onComplete?.Invoke();
    }
}
