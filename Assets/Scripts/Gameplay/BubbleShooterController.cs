using System;
using System.Collections;
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
    private float gameOverY;

    private BubbleGrid bubbleGrid;
    private BubbleQueue bubbleQueue;
    private BubbleEffectController effectController;
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
    public BubbleType CurrentType => bubbleQueue.CurrentType;
    public BubbleColor NextColor => bubbleQueue.NextColor;
    public BubbleType NextType => bubbleQueue.NextType;
    public IReadOnlyList<BubbleColor> UpcomingColors => bubbleQueue.UpcomingColors;
    public IReadOnlyList<BubbleType> UpcomingTypes => bubbleQueue.UpcomingTypes;

    private void Awake()
    {
        bubbleQueue = GetComponent<BubbleQueue>();
        effectController = GetComponent<BubbleEffectController>();
        inputHandler = GetComponent<ShooterInputHandler>();
        aimer = GetComponent<ShooterAimer>();
    }

    private void Start()
    {
        bubbleGrid = GameManager.Instance.BubbleGrid;
        RefreshShooterDisplay();
        StartCoroutine(SetupPositionAfterLayout());
    }

    private IEnumerator SetupPositionAfterLayout()
    {
        yield return new WaitForEndOfFrame(); // 렌더링 후 bounds 확정 대기
        SetupPosition();
    }

    private void SetupPosition()
    {
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);

        float bottomBarTopScreenY = GameManager.Instance.UIManager.GetPlayAreaBottomScreenY();
        Vector3 bottomBarTopWorld = cam.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, bottomBarTopScreenY, camZ));

        var shooterSprite = transform.Find("ShooterSprite")?.GetComponent<SpriteRenderer>();
        float spriteHalfH = (shooterSprite != null)
            ? shooterSprite.bounds.extents.y
            : BubbleGrid.BUBBLE_DIAMETER;

        float shooterY = bottomBarTopWorld.y + spriteHalfH;
        transform.position = new Vector3(transform.position.x, shooterY, transform.position.z);

        gameOverY = shooterY + BubbleGrid.ROW_HEIGHT * 2f;
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
        bubble.SetVisual(bubbleQueue.CurrentColor, bubbleQueue.CurrentType);

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
        proj.Launch(bubbleQueue.CurrentColor, bubbleQueue.CurrentType, dir, SHOOT_SPEED, LEFT_WALL, RIGHT_WALL, bubbleGrid, effectController, onLanded);

        bubbleQueue.Consume();
        RefreshShooterDisplay();

        OnFired?.Invoke();
    }

    private void RefreshShooterDisplay()
    {
        if (currentBubbleRenderer == null) return;
        currentBubbleRenderer.color = UnityEngine.Color.white;
        currentBubbleRenderer.sprite = bubbleQueue.CurrentType switch
        {
            BubbleType.Bomb     => Bubble.GetBombSprite(),
            BubbleType.Wildcard => Bubble.GetWildcardSprite(),
            _                   => Bubble.GetNormalSprite(bubbleQueue.CurrentColor),
        };
    }

    private void OnProjectileLanded()
    {
        bubbleGrid.AddRowAtTop(bubbleGrid.GenerateRandomRow(0));
        if (bubbleGrid.HasBubbleBelowY(gameOverY))
            GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
    }
}
