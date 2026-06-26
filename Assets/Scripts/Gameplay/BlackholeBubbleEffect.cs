using System.Collections.Generic;
using UnityEngine;

public class BlackholeBubbleEffect : MonoBehaviour, IBubbleEffect
{
    public BubbleType TargetType => BubbleType.Blackhole;

    public void Apply(BubbleGrid grid, int row, int col)
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

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
