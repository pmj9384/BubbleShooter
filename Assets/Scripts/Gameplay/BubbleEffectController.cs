using UnityEngine;

public class BubbleEffectController : MonoBehaviour
{
    private IBubbleEffect[] effects;

    private void Awake()
    {
        effects = GetComponents<IBubbleEffect>();
    }

    public void Apply(BubbleType type, BubbleGrid grid, int row, int col)
    {
        foreach (var effect in effects)
        {
            if (effect.TargetType == type)
            {
                effect.Apply(grid, row, col);
                return;
            }
        }

        NormalApply(grid, row, col);
    }

    private void NormalApply(BubbleGrid grid, int row, int col)
    {
        var matches = BubbleMatchProcessor.FindMatches(grid, row, col);
        foreach (var (r, c) in matches) grid.RemoveBubble(r, c);

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
