using System;
using UnityEngine;

public interface IBubbleSkill
{
    BubbleType TargetType { get; }
    bool UsesCustomFire { get; }
    void CustomFire(Vector2 screenPos, BubbleGrid grid, Action onComplete);
    void OnLand(BubbleGrid grid, int row, int col);
}
