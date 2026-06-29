using System.Collections.Generic;

public static class BubbleMatchProcessor
{
    private const int MIN_MATCH = 3;

    // 착지한 버블 (row, col)에서 같은 색 연결 BFS
    // 3개 미만이면 빈 리스트 반환
    public static List<(int row, int col)> FindMatches(BubbleGrid bubbleGrid, int startRow, int startCol)
    {
        var grid = bubbleGrid.Grid;
        var target = grid[startRow, startCol];
        if (target == null) return new List<(int, int)>();

        var visited = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();
        var result = new List<(int, int)>();

        queue.Enqueue((startRow, startCol));
        visited.Add((startRow, startCol));

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            result.Add((r, c));

            foreach (var (dr, dc) in bubbleGrid.GetNeighborOffsets(r))
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= BubbleGrid.MAX_ROWS || nc < 0 || nc >= BubbleGrid.COLS_EVEN) continue;
                if (visited.Contains((nr, nc))) continue;
                if (grid[nr, nc] == null) continue;
                if (grid[nr, nc].Type == BubbleType.Asteroid) continue;
                if (grid[nr, nc].Color != target.Color) continue;

                visited.Add((nr, nc));
                queue.Enqueue((nr, nc));
            }
        }

        return result.Count >= MIN_MATCH ? result : new List<(int, int)>();
    }

    // 0행과 연결되지 않은 버블 전체 탐색
    public static List<(int row, int col)> FindFloating(BubbleGrid bubbleGrid)
    {
        var grid = bubbleGrid.Grid;
        var connected = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();

        // 0행 버블 + 모든 소행성을 시작점으로 (소행성은 그리드 고정 장애물)
        for (int c = 0; c < BubbleGrid.COLS_EVEN; c++)
        {
            if (grid[0, c] != null)
            {
                queue.Enqueue((0, c));
                connected.Add((0, c));
            }
        }

        for (int r = 1; r < BubbleGrid.MAX_ROWS; r++)
            for (int c = 0; c < BubbleGrid.COLS_EVEN; c++)
                if (grid[r, c] != null && grid[r, c].Type == BubbleType.Asteroid && !connected.Contains((r, c)))
                {
                    connected.Add((r, c));
                    queue.Enqueue((r, c));
                }

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            foreach (var (dr, dc) in bubbleGrid.GetNeighborOffsets(r))
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= BubbleGrid.MAX_ROWS || nc < 0 || nc >= BubbleGrid.COLS_EVEN) continue;
                if (connected.Contains((nr, nc))) continue;
                if (grid[nr, nc] == null) continue;

                connected.Add((nr, nc));
                queue.Enqueue((nr, nc));
            }
        }

        // connected에 없는 버블 = 부유
        var floating = new List<(int, int)>();
        for (int r = 0; r < BubbleGrid.MAX_ROWS; r++)
            for (int c = 0; c < BubbleGrid.COLS_EVEN; c++)
                if (grid[r, c] != null && !connected.Contains((r, c)))
                    floating.Add((r, c));

        return floating;
    }
}
