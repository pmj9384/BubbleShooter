using UnityEngine;

public class MeteorBubbleEffect : MonoBehaviour, IBubbleEffect
{
    public BubbleType TargetType => BubbleType.Meteor;

    public void Apply(BubbleGrid grid, int row, int col)
    {
        for (int r = 0; r < BubbleGrid.MAX_ROWS; r++)
        {
            if (col <= grid.GetMaxCol(r))
                grid.RemoveBubble(r, col);
        }

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
