using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleShooterController : MonoBehaviour
{
    private const float LEFT_WALL  = -4.0f;
    private const float RIGHT_WALL =  4.0f;
    private const float SHOOT_SPEED = 12f;
    private const float MIN_ANGLE_FROM_HORIZONTAL = 10f;
    private const float REFLECT_LINE_LENGTH = 20f;
    private const int UPCOMING_COUNT = 6; // [0]=next, [1~5]=preview

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private SpriteRenderer currentBubbleRenderer;

    [SerializeField] private int shotsPerRow = 5;
    [SerializeField] private float gameOverY = -5f;

    private LineRenderer lineRenderer;
    private BubbleGrid bubbleGrid;
    private BubbleColor currentColor;
    private readonly List<BubbleColor> upcomingColors = new();
    private bool isDragging;
    private int shotCount;

    public event Action OnFired;

    public int ShotCount => shotCount;
    public int ShotsPerRow => shotsPerRow;
    public int ShotsUntilNextRow => shotsPerRow - (shotCount % shotsPerRow);
    public BubbleColor CurrentColor => currentColor;
    public BubbleColor NextColor => upcomingColors[0];
    public IReadOnlyList<BubbleColor> UpcomingColors => upcomingColors;

    private void Awake()
    {
        for (int i = 0; i < UPCOMING_COUNT; i++)
            upcomingColors.Add(RandomColor());
        currentColor = RandomColor();
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        bubbleGrid = GameManager.Instance.BubbleGrid;
        RefreshShooterDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var gm = GameManager.Instance;
            if (gm.CurrentState == GameManager.GameState.GamePlay)
                gm.SetGameState(GameManager.GameState.GameStop);
            else if (gm.CurrentState == GameManager.GameState.GameStop)
                gm.SetGameState(GameManager.GameState.GamePlay);
        }

        if (Input.GetMouseButtonDown(0)) isDragging = true;

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            lineRenderer.enabled = false;

            Vector2 dir = GetAimDirection();
            if (dir != Vector2.zero) Fire(dir);
            return;
        }

        if (isDragging)
        {
            Vector2 dir = GetAimDirection();
            if (dir != Vector2.zero) UpdateAimLine(dir);
            else lineRenderer.enabled = false;
        }
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)transform.position - (Vector2)mouseWorld).normalized;

        if (dir.y <= 0) return Vector2.zero;

        float minY = Mathf.Sin(MIN_ANGLE_FROM_HORIZONTAL * Mathf.Deg2Rad);
        if (dir.y < minY)
        {
            dir.y = minY;
            dir = dir.normalized;
        }
        return dir;
    }

    private void UpdateAimLine(Vector2 dir)
    {
        lineRenderer.enabled = true;
        Vector2 p0 = transform.position;

        Vector2 p1, p2;
        if (Mathf.Abs(dir.x) < 0.001f)
        {
            p1 = p0 + dir * (REFLECT_LINE_LENGTH * 0.5f);
            p2 = p0 + dir * REFLECT_LINE_LENGTH;
        }
        else
        {
            float wallX = dir.x > 0 ? RIGHT_WALL : LEFT_WALL;
            float t = (wallX - p0.x) / dir.x;
            p1 = p0 + dir * t;
            Vector2 reflectedDir = new Vector2(-dir.x, dir.y).normalized;
            p2 = p1 + reflectedDir * REFLECT_LINE_LENGTH;
        }

        lineRenderer.SetPosition(0, p0);
        lineRenderer.SetPosition(1, p1);
        lineRenderer.SetPosition(2, p2);
    }

    private void Fire(Vector2 dir)
    {
        GameObject go = Instantiate(bubblePrefab, transform.position, Quaternion.identity);

        var bubble = go.GetComponent<Bubble>();
        bubble.SetColor(currentColor);

        var col = go.GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        shotCount++;
        bool shouldAddRow = shotCount % shotsPerRow == 0;

        var proj = go.AddComponent<BubbleProjectile>();
        proj.Launch(currentColor, dir, SHOOT_SPEED, LEFT_WALL, RIGHT_WALL, bubbleGrid, shouldAddRow ? (Action)OnProjectileLanded : null);

        currentColor = upcomingColors[0];
        upcomingColors.RemoveAt(0);
        upcomingColors.Add(RandomColor());
        RefreshShooterDisplay();

        OnFired?.Invoke();
    }

    private void RefreshShooterDisplay()
    {
        if (currentBubbleRenderer != null)
            currentBubbleRenderer.color = Bubble.GetColorMap()[(int)currentColor];
    }

    private void OnProjectileLanded()
    {
        bubbleGrid.AddRowAtTop(bubbleGrid.GenerateRandomRow(0));
        if (bubbleGrid.HasBubbleBelowY(gameOverY))
            GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
    }

    private BubbleColor RandomColor() =>
        (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.Count);
}
