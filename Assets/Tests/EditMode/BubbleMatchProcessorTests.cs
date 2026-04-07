using NUnit.Framework;
using UnityEngine;

public class BubbleMatchProcessorTests
{
    // 3개 이상 같은 색 연결 → 매칭 목록 반환
    [Test]
    public void FindMatches_ThreeSameColor_ReturnsAll()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        grid[0, 0] = MakeBubble(BubbleColor.Red, 0, 0);
        grid[0, 1] = MakeBubble(BubbleColor.Red, 0, 1);
        grid[0, 2] = MakeBubble(BubbleColor.Red, 0, 2);

        var matches = BubbleMatchProcessor.FindMatches(grid, 0, 1);

        Assert.AreEqual(3, matches.Count);
        Object.DestroyImmediate(grid[0, 0].gameObject);
        Object.DestroyImmediate(grid[0, 1].gameObject);
        Object.DestroyImmediate(grid[0, 2].gameObject);
    }

    // 2개만 연결 → 빈 목록 반환
    [Test]
    public void FindMatches_TwoSameColor_ReturnsEmpty()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        grid[0, 0] = MakeBubble(BubbleColor.Red, 0, 0);
        grid[0, 1] = MakeBubble(BubbleColor.Red, 0, 1);

        var matches = BubbleMatchProcessor.FindMatches(grid, 0, 0);

        Assert.AreEqual(0, matches.Count);
        Object.DestroyImmediate(grid[0, 0].gameObject);
        Object.DestroyImmediate(grid[0, 1].gameObject);
    }

    // 0행과 연결되지 않은 버블 → 부유로 판정
    [Test]
    public void FindFloating_DisconnectedBubble_ReturnsIt()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        grid[1, 0] = MakeBubble(BubbleColor.Blue, 1, 0);

        var floating = BubbleMatchProcessor.FindFloating(grid);

        Assert.AreEqual(1, floating.Count);
        Object.DestroyImmediate(grid[1, 0].gameObject);
    }

    // 0행 버블 → 부유 아님
    [Test]
    public void FindFloating_TopRowBubble_ReturnsEmpty()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        grid[0, 0] = MakeBubble(BubbleColor.Blue, 0, 0);

        var floating = BubbleMatchProcessor.FindFloating(grid);

        Assert.AreEqual(0, floating.Count);
        Object.DestroyImmediate(grid[0, 0].gameObject);
    }

    private Bubble MakeBubble(BubbleColor color, int row, int col)
    {
        var go = new GameObject();
        go.AddComponent<SpriteRenderer>();
        var col2d = go.AddComponent<CircleCollider2D>();
        col2d.radius = 0.48f;
        var b = go.AddComponent<Bubble>();
        b.SetColor(color);
        b.Row = row;
        b.Col = col;
        return b;
    }
}
