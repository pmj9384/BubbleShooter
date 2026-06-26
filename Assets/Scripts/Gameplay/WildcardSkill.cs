using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WildcardSkill : MonoBehaviour, IBubbleSkill
{
    public BubbleType TargetType => BubbleType.Wildcard;
    public bool UsesCustomFire => false;

    public void CustomFire(Vector2 screenPos, BubbleGrid grid, Action onComplete) { }

    public void OnLand(BubbleGrid grid, int row, int col)
    {
        var colorCounts = new Dictionary<BubbleColor, int>();

        foreach (var (dr, dc) in grid.GetNeighborOffsets(row))
        {
            int nr = row + dr, nc = col + dc;
            if (nr < 0 || nr >= BubbleGrid.MAX_ROWS || nc < 0 || nc >= BubbleGrid.COLS_EVEN) continue;
            var neighbor = grid.Grid[nr, nc];
            if (neighbor == null) continue;

            if (!colorCounts.ContainsKey(neighbor.Color)) colorCounts[neighbor.Color] = 0;
            colorCounts[neighbor.Color]++;
        }

        BubbleColor targetColor = colorCounts.Count > 0
            ? colorCounts.OrderByDescending(kvp => kvp.Value).First().Key
            : (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.Count);

        grid.Grid[row, col].SetColor(targetColor);

        var matches = BubbleMatchProcessor.FindMatches(grid, row, col);
        foreach (var (r, c) in matches) grid.RemoveBubble(r, c);

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
