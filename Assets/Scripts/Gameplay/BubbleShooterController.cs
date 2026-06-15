using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleShooterController : MonoBehaviour
{
    private const float LEFT_WALL = -4.0f;
    private const float RIGHT_WALL = 4.0f;
    private const float SHOOT_SPEED = 12f;
    private const float MIN_ANGLE_FROM_HORIZONTAL = 10f;
    private const float REFLECT_LINE_LENGTH = 20f;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private SpriteRenderer currentBubbleRenderer;

    [SerializeField] private int shotsPerRow = 5;
    [SerializeField] private float gameOverY = -5f;

    private LineRenderer lineRenderer;
    private BubbleGrid bubbleGrid;
    private BubbleQueue bubbleQueue;
    private bool isDragging;
    private bool canShoot = true;
    private int shotCount;

    public event Action OnFired;
    public event Action<Vector2> OnAimDirectionChanged;
    public int ShotCount => shotCount;
    public int ShotsPerRow => shotsPerRow;
    public int ShotsUntilNextRow => shotsPerRow - (shotCount % shotsPerRow);
    public BubbleColor CurrentColor => bubbleQueue.CurrentColor;
    public BubbleColor NextColor => bubbleQueue.NextColor;
    public IReadOnlyList<BubbleColor> UpcomingColors => bubbleQueue.UpcomingColors;

    private void Awake()
    {
        bubbleQueue = GetComponent<BubbleQueue>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
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

        if (GameManager.Instance.CurrentState != GameManager.GameState.GamePlay) return;
        if (!canShoot) return;

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
            OnAimDirectionChanged?.Invoke(dir);
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
        canShoot = false;

        GameObject go = Instantiate(bubblePrefab, transform.position, Quaternion.identity);

        var bubble = go.GetComponent<Bubble>();
        bubble.SetColor(bubbleQueue.CurrentColor);

        var col = go.GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        shotCount++;
        bool shouldAddRow = shotCount % shotsPerRow == 0;

        Action onLanded = () =>
        {
            if (shouldAddRow) OnProjectileLanded();
            canShoot = true;
        };

        var proj = go.AddComponent<BubbleProjectile>();
        proj.Launch(bubbleQueue.CurrentColor, dir, SHOOT_SPEED, LEFT_WALL, RIGHT_WALL, bubbleGrid, onLanded);

        bubbleQueue.Consume();
        RefreshShooterDisplay();

        OnFired?.Invoke();
    }

    private void RefreshShooterDisplay()
    {
        if (currentBubbleRenderer != null)
            currentBubbleRenderer.color = Bubble.GetColorMap()[(int)bubbleQueue.CurrentColor];
    }

    private void OnProjectileLanded()
    {
        bubbleGrid.AddRowAtTop(bubbleGrid.GenerateRandomRow(0));
        if (bubbleGrid.HasBubbleBelowY(gameOverY))
            GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
    }
}
