using System;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeSkill : BubbleSkill
{
    public override BubbleType TargetType => BubbleType.Blackhole;

    public override void OnLand(BubbleGrid grid, int row, int col, Action onComplete)
    {
        var toRemove = new List<(int, int)> { (row, col) };

        foreach (var (dr, dc) in grid.GetNeighborOffsets(row))
        {
            int nr = row + dr, nc = col + dc;
            if (nr >= 0 && nr < BubbleGrid.MAX_ROWS && nc >= 0 && nc <= grid.GetMaxCol(nr))
                toRemove.Add((nr, nc));
        }

        foreach (var (r, c) in toRemove)
            grid.RemoveBubble(r, c);

        RemoveFloating(grid);
        onComplete?.Invoke();
    }
}
