using System;
using System.Collections.Generic;
using UnityEngine;

public class BubbleShooterController : MonoBehaviour
{
    private const float LEFT_WALL = -4.0f;
    private const float RIGHT_WALL = 4.0f;
    private const float SHOOT_SPEED = 12f;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private SpriteRenderer currentBubbleRenderer;

    [SerializeField] private int shotsPerRow = 5;
    [SerializeField] private float gameOverY = -5f;

    private BubbleGrid bubbleGrid;
    private BubbleQueue bubbleQueue;
    private ShooterInputHandler inputHandler;
    private ShooterAimer aimer;
    private Vector2 lastAimDir;
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
        inputHandler = GetComponent<ShooterInputHandler>();
        aimer = GetComponent<ShooterAimer>();
    }

    private void Start()
    {
        bubbleGrid = GameManager.Instance.BubbleGrid;
        RefreshShooterDisplay();
    }

    private void OnEnable()
    {
        inputHandler.OnReleased += HandleReleased;
        inputHandler.OnEscapePressed += HandleEscape;
        aimer.OnAimDirectionChanged += HandleAimDirectionChanged;
    }

    private void OnDisable()
    {
        inputHandler.OnReleased -= HandleReleased;
        inputHandler.OnEscapePressed -= HandleEscape;
        aimer.OnAimDirectionChanged -= HandleAimDirectionChanged;
    }

    private void HandleAimDirectionChanged(Vector2 dir)
    {
        lastAimDir = dir;
        OnAimDirectionChanged?.Invoke(dir);
    }

    private void HandleReleased(Vector2 screenPos)
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.GamePlay) return;
        if (!canShoot) return;
        if (lastAimDir != Vector2.zero) Fire(lastAimDir);
    }

    private void HandleEscape()
    {
        var gm = GameManager.Instance;
        if (gm.CurrentState == GameManager.GameState.GamePlay)
            gm.SetGameState(GameManager.GameState.GameStop);
        else if (gm.CurrentState == GameManager.GameState.GameStop)
            gm.SetGameState(GameManager.GameState.GamePlay);
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
