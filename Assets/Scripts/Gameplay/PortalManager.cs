using UnityEngine;

public class PortalManager : InGameManager
{
    public static PortalManager Current { get; private set; }

    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform[] leftSpawnPoints = new Transform[5];
    [SerializeField] private Transform[] rightSpawnPoints = new Transform[5];
    [SerializeField] private int shotsPerSpawn = 10;
    [SerializeField] private int portalDurationShots = 5;
    [SerializeField] private float portalHalfHeight = 1.0f;

    private GameObject activePortal;
    private bool activePortalIsLeft;
    private int shotCount;
    private int shotsSinceSpawn;
    private BubbleShooterController shooter;

    public override void Initialize()
    {
        Current = this;
        shooter = FindAnyObjectByType<BubbleShooterController>();
        shooter.OnFired += OnFired;
        shooter.OnBubbleLanded += OnLanded;
    }

    public override void Clear()
    {
        Current = null;
        if (shooter != null)
        {
            shooter.OnFired -= OnFired;
            shooter.OnBubbleLanded -= OnLanded;
        }
        DestroyPortal();
    }

    private void OnFired()
    {
        shotCount++;
        if (shotCount % shotsPerSpawn == 0)
            SpawnPortal();
    }

    private void OnLanded()
    {
        if (activePortal == null) return;
        shotsSinceSpawn++;
        if (shotsSinceSpawn >= portalDurationShots)
            DestroyPortal();
    }

    // BubbleProjectile.FixedUpdate에서 호출 — 포탈 구간이면 true 반환 후 포탈 소멸
    public bool AbsorbIfPortalZone(float bulletY, bool isLeftWall)
    {
        if (activePortal == null) return false;
        if (activePortalIsLeft != isLeftWall) return false;
        if (Mathf.Abs(activePortal.transform.position.y - bulletY) > portalHalfHeight) return false;

        DestroyPortal();
        return true;
    }

    private void SpawnPortal()
    {
        DestroyPortal();

        bool pickLeft = Random.value < 0.5f;
        var points = pickLeft ? leftSpawnPoints : rightSpawnPoints;
        var point = points[Random.Range(0, points.Length)];

        activePortal = Instantiate(portalPrefab, point.position, point.rotation);
        activePortalIsLeft = pickLeft;
        shotsSinceSpawn = 0;
    }

    private void DestroyPortal()
    {
        if (activePortal != null)
        {
            Destroy(activePortal);
            activePortal = null;
        }
    }
}
