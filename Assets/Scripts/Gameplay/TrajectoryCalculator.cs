using UnityEngine;

public static class TrajectoryCalculator
{
    private const float STEP_SIZE = 0.1f;
    private const int MAX_STEPS = 2000;

    public static (int row, int col) CalculateLanding(
        Vector2 startPos,
        Vector2 dir,
        float leftWall,
        float rightWall,
        BubbleGrid grid)
    {
        Vector2 pos = startPos;
        Vector2 vel = dir.normalized;
        float ceilingY = grid.GetWorldPosition(0, 0).y + BubbleGrid.ROW_HEIGHT;

        for (int i = 0; i < MAX_STEPS; i++)
        {
            pos += vel * STEP_SIZE;

            if (pos.x <= leftWall)  { pos.x = leftWall;  vel.x =  Mathf.Abs(vel.x); }
            else if (pos.x >= rightWall) { pos.x = rightWall; vel.x = -Mathf.Abs(vel.x); }

            if (pos.y >= ceilingY)
                return grid.FindNearestEmpty(pos);

            var (row, col) = grid.GetGridPosition(pos);
            if (grid.IsOccupied(row, col))
                return grid.FindNearestEmptyAdjacentTo(row, col, pos);
        }

        return grid.FindNearestEmpty(pos);
    }
}
