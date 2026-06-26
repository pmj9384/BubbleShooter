using System;
using System.Collections;
using UnityEngine;

public class MeteorSkill : BubbleSkill
{
    public override BubbleType TargetType => BubbleType.Meteor;

    public override void OnLand(BubbleGrid grid, int row, int col, Action onComplete)
    {
        StartCoroutine(FallCoroutine(row, col, grid, onComplete));
    }

    private IEnumerator FallCoroutine(int landRow, int col, BubbleGrid grid, Action onComplete)
    {
        Vector2 startPos = (Vector2)grid.GetWorldPosition(0, col) + Vector2.up * 3f;
        Vector2 endPos = (Vector2)grid.GetWorldPosition(landRow, col);

        var visual = new GameObject("MeteorVisual");
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = Bubble.GetMeteorFallSprite();
        sr.sortingOrder = 10;
        visual.transform.position = startPos;

        float distance = Vector2.Distance(startPos, endPos);
        float duration = distance / 15f;
        float elapsed = 0f;
        int lastRemovedRow = -1;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, elapsed / duration);
            visual.transform.position = currentPos;

            var (currentRow, _) = grid.GetGridPosition(currentPos);
            for (int r = lastRemovedRow + 1; r <= currentRow && r < BubbleGrid.MAX_ROWS; r++)
            {
                if (col <= grid.GetMaxCol(r))
                    grid.RemoveBubble(r, col);
            }
            lastRemovedRow = Mathf.Max(lastRemovedRow, currentRow);

            yield return null;
        }

        for (int r = lastRemovedRow + 1; r <= landRow && r < BubbleGrid.MAX_ROWS; r++)
            if (col <= grid.GetMaxCol(r))
                grid.RemoveBubble(r, col);

        Destroy(visual);
        RemoveFloating(grid);
        onComplete?.Invoke();
    }
}
