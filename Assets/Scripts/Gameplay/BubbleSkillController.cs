using System;
using UnityEngine;

public class BubbleSkillController : MonoBehaviour
{
    private IBubbleSkill[] skills;

    private void Awake()
    {
        skills = GetComponents<IBubbleSkill>();
    }

    public bool UsesCustomFire(BubbleType type)
    {
        foreach (var skill in skills)
            if (skill.TargetType == type) return skill.UsesCustomFire;
        return false;
    }

    public void CustomFire(BubbleType type, Vector2 screenPos, BubbleGrid grid, Action onComplete)
    {
        foreach (var skill in skills)
        {
            if (skill.TargetType == type)
            {
                skill.CustomFire(screenPos, grid, onComplete);
                return;
            }
        }
    }

    public void OnLand(BubbleType type, BubbleGrid grid, int row, int col)
    {
        foreach (var skill in skills)
        {
            if (skill.TargetType == type)
            {
                skill.OnLand(grid, row, col);
                return;
            }
        }
        NormalOnLand(grid, row, col);
    }

    private void NormalOnLand(BubbleGrid grid, int row, int col)
    {
        var matches = BubbleMatchProcessor.FindMatches(grid, row, col);
        foreach (var (r, c) in matches) grid.RemoveBubble(r, c);

        var floating = BubbleMatchProcessor.FindFloating(grid);
        foreach (var (r, c) in floating) grid.RemoveBubble(r, c);
    }
}
