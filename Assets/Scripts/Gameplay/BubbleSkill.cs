using System;
using UnityEngine;

public abstract class BubbleSkill : MonoBehaviour, IBubbleSkill
{
    public abstract BubbleType TargetType { get; }
    public virtual bool UsesCustomFire => false;

    public virtual void CustomFire(Vector2 screenPos, BubbleGrid grid, Action onComplete) { }

    public virtual void OnLand(BubbleGrid grid, int row, int col, Action onComplete)
    {
        var matches = BubbleMatchProcessor.FindMatches(grid, row, col);
        foreach (var (r, c) in matches) grid.RemoveBubble(r, c);

        RemoveFloating(grid);
        onComplete?.Invoke();
    }

    protected void RemoveFloating(BubbleGrid grid)
    {
        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
