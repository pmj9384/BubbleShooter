using UnityEngine;
using UnityEngine.Pool;

public class BubbleGrid : MonoBehaviour
{
    public const float BUBBLE_DIAMETER = 1.0f;
    public const float ROW_HEIGHT = 0.866f;
    public const int COLS_EVEN = 8;
    public const int COLS_ODD = 7;
    public const int MAX_ROWS = 20;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float gridOriginX = -3.5f;
    [SerializeField] private float gridOriginY = 7.0f;

    public Bubble[,] Grid { get; private set; } = new Bubble[MAX_ROWS, COLS_EVEN];

    private ObjectPool<GameObject> bubblePool;

    private void Awake()
    {
        bubblePool = GameManager.Instance.ObjectPool.CreateObjectPool(
            bubblePrefab,
            () => Instantiate(bubblePrefab, transform),
            go => go.SetActive(true),
            go => go.SetActive(false)
        );

        SpawnInitialRows(5);
    }

    public Vector2 GetWorldPosition(int row, int col)
    {
        float x = gridOriginX + col * BUBBLE_DIAMETER;
        if (row % 2 == 1) x += BUBBLE_DIAMETER * 0.5f;
        float y = gridOriginY - row * ROW_HEIGHT;
        return new Vector2(x, y);
    }

    public (int row, int col) GetGridPosition(Vector2 worldPos)
    {
        int row = Mathf.RoundToInt((gridOriginY - worldPos.y) / ROW_HEIGHT);
        row = Mathf.Clamp(row, 0, MAX_ROWS - 1);

        float offsetX = (row % 2 == 1) ? BUBBLE_DIAMETER * 0.5f : 0f;
        int col = Mathf.RoundToInt((worldPos.x - gridOriginX - offsetX) / BUBBLE_DIAMETER);
        col = Mathf.Clamp(col, 0, GetMaxCol(row));

        return (row, col);
    }

    public bool IsOccupied(int row, int col) => Grid[row, col] != null;

    public int GetMaxCol(int row) => (row % 2 == 0) ? COLS_EVEN - 1 : COLS_ODD - 1;

    public (int row, int col) FindNearestEmpty(Vector2 worldPos)
    {
        var (row, col) = GetGridPosition(worldPos);

        if (!IsOccupied(row, col)) return (row, col);

        foreach (var (dr, dc) in GetNeighborOffsets(row))
        {
            int nr = row + dr, nc = col + dc;
            if (nr >= 0 && nr < MAX_ROWS && nc >= 0 && nc <= GetMaxCol(nr) && !IsOccupied(nr, nc))
                return (nr, nc);
        }
        return (row, col);
    }

    public void PlaceBubble(Bubble bubble, int row, int col)
    {
        bubble.Row = row;
        bubble.Col = col;
        Grid[row, col] = bubble;
        bubble.transform.position = GetWorldPosition(row, col);
        bubble.transform.SetParent(transform);
    }

    public void RemoveBubble(int row, int col)
    {
        if (Grid[row, col] != null)
        {
            bubblePool.Release(Grid[row, col].gameObject);
            Grid[row, col] = null;
        }
    }

    public void AddRowAtTop(BubbleColor[] colors)
    {
        for (int r = MAX_ROWS - 1; r > 0; r--)
        {
            for (int c = 0; c < COLS_EVEN; c++)
            {
                Grid[r, c] = Grid[r - 1, c];
                if (Grid[r, c] != null)
                {
                    Grid[r, c].Row = r;
                    Grid[r, c].transform.position = GetWorldPosition(r, Grid[r, c].Col);
                }
            }
        }

        for (int c = 0; c < COLS_EVEN; c++)
            Grid[0, c] = null;

        SpawnRow(0, colors);
    }

    public void SpawnInitialRows(int count)
    {
        for (int r = 0; r < count; r++)
            SpawnRow(r, GenerateRandomRow(r));
    }

    private void SpawnRow(int row, BubbleColor[] colors)
    {
        int maxCol = GetMaxCol(row);
        for (int c = 0; c <= maxCol && c < colors.Length; c++)
        {
            var go = bubblePool.Get();
            go.transform.position = GetWorldPosition(row, c);
            go.transform.SetParent(transform);
            var bubble = go.GetComponent<Bubble>();
            bubble.SetColor(colors[c]);
            bubble.Row = row;
            bubble.Col = c;
            Grid[row, c] = bubble;
        }
    }

    public BubbleColor[] GenerateRandomRow(int row)
    {
        int count = (row % 2 == 0) ? COLS_EVEN : COLS_ODD;
        var colors = new BubbleColor[count];
        for (int i = 0; i < count; i++)
            colors[i] = (BubbleColor)Random.Range(0, (int)BubbleColor.Count);
        return colors;
    }

    public static (int dr, int dc)[] GetNeighborOffsets(int row)
    {
        if (row % 2 == 0)
            return new[] { (0, -1), (0, 1), (-1, -1), (-1, 0), (1, -1), (1, 0) };
        else
            return new[] { (0, -1), (0, 1), (-1, 0), (-1, 1), (1, 0), (1, 1) };
    }

    public bool HasBubbleBelowY(float y)
    {
        for (int r = 0; r < MAX_ROWS; r++)
            for (int c = 0; c < COLS_EVEN; c++)
                if (Grid[r, c] != null && Grid[r, c].transform.position.y < y)
                    return true;
        return false;
    }
}
