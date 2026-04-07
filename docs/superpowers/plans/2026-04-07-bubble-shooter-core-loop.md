# Bubble Shooter Core Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 세로 화면에서 버블을 드래그로 발사하고, 3개 이상 같은 색 매칭 시 제거, 5발마다 한 줄 내려오며, 하단 라인 초과 시 게임오버가 되는 무한 생존 버블슈터 핵심 루프 구현

**Architecture:** 헥사고날 오프셋 그리드로 버블을 배치하고, 순수 C# BFS 로직(BubbleMatchProcessor)으로 매칭/부유 버블을 처리한다. BubbleGameManager(InGameManager)가 발사 카운터, 줄 추가, 게임오버 판정을 담당하고 기존 GameManager 상태 머신에 연결된다. 프로젝타일은 Kinematic Rigidbody2D로 수동 이동하며 CircleCollider2D 트리거로 착지를 감지한다.

**Tech Stack:** Unity 2D, C#, Physics2D (Kinematic Rigidbody2D + CircleCollider2D), LineRenderer (조준선)

---

## 파일 구조

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/Defines/BubbleColor.cs` | BubbleColor enum 정의 |
| `Assets/Scripts/Defines/Enums.cs` | BgmClipId/SfxClipId 버블슈터용 항목 추가 |
| `Assets/Scripts/Defines/UIElementEnums.cs` | HUDPanel, GameOverPanel 인덱스 |
| `Assets/Scripts/Gameplay/Bubble.cs` | 개별 버블 MonoBehaviour (색, 그리드 좌표, 비주얼) |
| `Assets/Scripts/Gameplay/BubbleGrid.cs` | 그리드 배열 관리, 좌표 변환, 줄 생성/추가 |
| `Assets/Scripts/Gameplay/BubbleMatchProcessor.cs` | 순수 C# BFS 매칭 + 부유 버블 탐지 (Edit Mode 테스트 가능) |
| `Assets/Scripts/Gameplay/BubbleProjectile.cs` | 비행 버블 이동, 벽 반사, 착지 이벤트 |
| `Assets/Scripts/Gameplay/BubbleShooter.cs` | 드래그 입력, LineRenderer 조준선, 발사 |
| `Assets/Scripts/Gameplay/BubbleGameManager.cs` | InGameManager: 발사 카운터, 줄 추가, 게임오버 판정 |
| `Assets/Scripts/UI/Elements/HUDPanel.cs` | 다음 줄 추가까지 남은 발 수 표시 |
| `Assets/Scripts/UI/Elements/GameOverPanel.cs` | 게임오버 패널, 재시작 버튼 |
| `Assets/Scripts/Core/Managers/GameManager.cs` | BubbleGameManager 등록 추가 |
| `Assets/Scripts/UI/Core/GameUIManager.cs` | 상태별 UI 액션 연결 |

---

## 그리드 상수 (공통 참고)

```
BUBBLE_DIAMETER = 1.0f
ROW_HEIGHT = 0.866f         // diameter * sin(60°)
COLS_EVEN = 8               // 짝수 행 버블 수
COLS_ODD = 7                // 홀수 행 버블 수 (오프셋 +0.5)
GRID_ORIGIN_X = -3.5f       // 짝수 행 첫 버블 X (화면 중앙 기준)
GRID_ORIGIN_Y = 7.0f        // 최상단 행 Y (카메라 top 근처)
INITIAL_ROWS = 5
SHOTS_PER_DROP = 5
```

---

## Task 1: BubbleColor enum + Bubble 스크립트

**Files:**
- Create: `Assets/Scripts/Defines/BubbleColor.cs`
- Create: `Assets/Scripts/Gameplay/Bubble.cs`

- [ ] **Step 1: BubbleColor.cs 작성**

```csharp
// Assets/Scripts/Defines/BubbleColor.cs
public enum BubbleColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Count  // 색 종류 수 (랜덤 생성 시 사용)
}
```

- [ ] **Step 2: Bubble.cs 작성**

```csharp
// Assets/Scripts/Gameplay/Bubble.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bubble : MonoBehaviour
{
    public BubbleColor Color { get; private set; }
    public int Row { get; set; }
    public int Col { get; set; }

    private SpriteRenderer spriteRenderer;

    private static readonly UnityEngine.Color[] ColorMap = new UnityEngine.Color[]
    {
        UnityEngine.Color.red,
        UnityEngine.Color.blue,
        UnityEngine.Color.green,
        UnityEngine.Color.yellow,
        new UnityEngine.Color(0.6f, 0.2f, 0.8f), // Purple
    };

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.48f; // 약간 작게 해서 틈 허용
    }

    public void SetColor(BubbleColor color)
    {
        Color = color;
        spriteRenderer.color = ColorMap[(int)color];
    }
}
```

- [ ] **Step 3: Bubble 프리팹 생성 (Unity 에디터)**
  - Hierarchy에서 빈 오브젝트 생성 → 이름 "Bubble"
  - `Bubble.cs` 컴포넌트 추가 (SpriteRenderer, CircleCollider2D 자동 추가됨)
  - SpriteRenderer > Sprite: Unity 기본 Circle 스프라이트 (Sprites/Circle) 또는 흰 원
  - Scale: (1, 1, 1) (diameter = 1.0)
  - `Assets/Prefabs/` 폴더 생성 후 프리팹으로 저장

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Defines/BubbleColor.cs Assets/Scripts/Gameplay/Bubble.cs Assets/Prefabs/
git commit -m "feat: add BubbleColor enum and Bubble prefab"
```

---

## Task 2: BubbleGrid - 그리드 관리 + 초기 버블 생성

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleGrid.cs`

- [ ] **Step 1: BubbleGrid.cs 작성**

```csharp
// Assets/Scripts/Gameplay/BubbleGrid.cs
using UnityEngine;

public class BubbleGrid : MonoBehaviour
{
    public const float BUBBLE_DIAMETER = 1.0f;
    public const float ROW_HEIGHT = 0.866f;
    public const int COLS_EVEN = 8;
    public const int COLS_ODD = 7;
    public const int MAX_ROWS = 20;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float gridOriginX = -3.5f;  // 짝수 행 첫 버블 X
    [SerializeField] private float gridOriginY = 7.0f;   // 0번 행 Y

    public Bubble[,] Grid { get; private set; } = new Bubble[MAX_ROWS, COLS_EVEN];

    private void Awake()
    {
        SpawnInitialRows(5);
    }

    // 그리드 (row, col) → 월드 좌표
    public Vector2 GetWorldPosition(int row, int col)
    {
        float x = gridOriginX + col * BUBBLE_DIAMETER;
        if (row % 2 == 1) x += BUBBLE_DIAMETER * 0.5f; // 홀수 행 오프셋
        float y = gridOriginY - row * ROW_HEIGHT;
        return new Vector2(x, y);
    }

    // 월드 좌표 → 가장 가까운 그리드 (row, col)
    public (int row, int col) GetGridPosition(Vector2 worldPos)
    {
        int row = Mathf.RoundToInt((gridOriginY - worldPos.y) / ROW_HEIGHT);
        row = Mathf.Clamp(row, 0, MAX_ROWS - 1);

        float offsetX = (row % 2 == 1) ? BUBBLE_DIAMETER * 0.5f : 0f;
        int col = Mathf.RoundToInt((worldPos.x - gridOriginX - offsetX) / BUBBLE_DIAMETER);
        int maxCol = (row % 2 == 0) ? COLS_EVEN - 1 : COLS_ODD - 1;
        col = Mathf.Clamp(col, 0, maxCol);

        return (row, col);
    }

    public bool IsOccupied(int row, int col) => Grid[row, col] != null;

    public int GetMaxCol(int row) => (row % 2 == 0) ? COLS_EVEN - 1 : COLS_ODD - 1;

    // 비어 있는 가장 가까운 셀 찾기 (착지 시 사용)
    public (int row, int col) FindNearestEmpty(Vector2 worldPos)
    {
        var (row, col) = GetGridPosition(worldPos);

        // 이미 비어 있으면 그대로
        if (!IsOccupied(row, col)) return (row, col);

        // 인접 셀 순서대로 시도
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
            Destroy(Grid[row, col].gameObject);
            Grid[row, col] = null;
        }
    }

    // 새 행을 맨 위(row 0)에 추가 — 기존 행을 아래로 한 칸씩 밀어냄
    public void AddRowAtTop(BubbleColor[] colors)
    {
        // 기존 버블 한 줄 아래로 이동
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

        // 새 행 생성
        for (int c = 0; c < COLS_EVEN; c++)
        {
            Grid[0, c] = null;
        }
        SpawnRow(0, colors);
    }

    public void SpawnInitialRows(int count)
    {
        for (int r = 0; r < count; r++)
        {
            var colors = GenerateRandomRow(r);
            SpawnRow(r, colors);
        }
    }

    private void SpawnRow(int row, BubbleColor[] colors)
    {
        int maxCol = GetMaxCol(row);
        for (int c = 0; c <= maxCol; c++)
        {
            if (c < colors.Length)
            {
                var go = Instantiate(bubblePrefab, GetWorldPosition(row, c), Quaternion.identity, transform);
                var bubble = go.GetComponent<Bubble>();
                bubble.SetColor(colors[c]);
                bubble.Row = row;
                bubble.Col = c;
                Grid[row, c] = bubble;
            }
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

    // 헥사고날 인접 오프셋 (row 홀/짝에 따라 다름)
    public static (int dr, int dc)[] GetNeighborOffsets(int row)
    {
        if (row % 2 == 0)
            return new[] { (0,-1),(0,1),(-1,-1),(-1,0),(1,-1),(1,0) };
        else
            return new[] { (0,-1),(0,1),(-1,0),(-1,1),(1,0),(1,1) };
    }

    // 특정 Y 이하에 버블이 있는지 (게임오버 판정)
    public bool HasBubbleBelowY(float y)
    {
        for (int r = 0; r < MAX_ROWS; r++)
        {
            for (int c = 0; c < COLS_EVEN; c++)
            {
                if (Grid[r, c] != null && Grid[r, c].transform.position.y < y)
                    return true;
            }
        }
        return false;
    }
}
```

- [ ] **Step 2: Unity 에디터에서 BubbleGrid 오브젝트 설정**
  - Hierarchy에 빈 오브젝트 "BubbleGrid" 추가
  - `BubbleGrid.cs` 컴포넌트 추가
  - `bubblePrefab` 필드에 Bubble.prefab 연결
  - `gridOriginX`, `gridOriginY` 값 조정 (카메라 범위에 맞게)

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleGrid.cs
git commit -m "feat: add BubbleGrid with hex offset layout and row management"
```

---

## Task 3: BubbleMatchProcessor - BFS 매칭 + 부유 버블 탐지

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleMatchProcessor.cs`
- Create: `Assets/Tests/EditMode/BubbleMatchProcessorTests.cs`

- [ ] **Step 1: BubbleMatchProcessorTests.cs 작성 (실패 테스트)**

```csharp
// Assets/Tests/EditMode/BubbleMatchProcessorTests.cs
using NUnit.Framework;
using UnityEngine;

public class BubbleMatchProcessorTests
{
    // 3개 이상 같은 색 연결 → 매칭 목록 반환
    [Test]
    public void FindMatches_ThreeSameColor_ReturnsAll()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        // row 0: Red Red Red ...
        grid[0, 0] = MakeBubble(BubbleColor.Red, 0, 0);
        grid[0, 1] = MakeBubble(BubbleColor.Red, 0, 1);
        grid[0, 2] = MakeBubble(BubbleColor.Red, 0, 2);

        var matches = BubbleMatchProcessor.FindMatches(grid, 0, 1);

        Assert.AreEqual(3, matches.Count);
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
    }

    // 0행과 연결되지 않은 버블 → 부유로 판정
    [Test]
    public void FindFloating_DisconnectedBubble_ReturnsIt()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        // row 1에만 버블 (row 0과 연결 없음)
        grid[1, 0] = MakeBubble(BubbleColor.Blue, 1, 0);

        var floating = BubbleMatchProcessor.FindFloating(grid);

        Assert.AreEqual(1, floating.Count);
    }

    // 0행 버블 → 부유 아님
    [Test]
    public void FindFloating_TopRowBubble_ReturnsEmpty()
    {
        var grid = new Bubble[BubbleGrid.MAX_ROWS, BubbleGrid.COLS_EVEN];
        grid[0, 0] = MakeBubble(BubbleColor.Blue, 0, 0);

        var floating = BubbleMatchProcessor.FindFloating(grid);

        Assert.AreEqual(0, floating.Count);
    }

    private Bubble MakeBubble(BubbleColor color, int row, int col)
    {
        // Edit Mode에서 MonoBehaviour 직접 생성 불가 → ScriptableObject 아님, 임시 Go 사용
        var go = new GameObject();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<CircleCollider2D>();
        var b = go.AddComponent<Bubble>();
        b.SetColor(color);
        b.Row = row;
        b.Col = col;
        return b;
    }
}
```

- [ ] **Step 2: Unity Test Runner에서 실행 → FAIL 확인**
  - Window > General > Test Runner > Edit Mode > Run All
  - `BubbleMatchProcessorTests` 전부 FAIL (클래스 없음) 확인

- [ ] **Step 3: BubbleMatchProcessor.cs 작성**

```csharp
// Assets/Scripts/Gameplay/BubbleMatchProcessor.cs
using System.Collections.Generic;

public static class BubbleMatchProcessor
{
    private const int MIN_MATCH = 3;

    // 착지한 버블 (row, col)에서 같은 색 연결 BFS
    // 3개 미만이면 빈 리스트 반환
    public static List<(int row, int col)> FindMatches(Bubble[,] grid, int startRow, int startCol)
    {
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

            foreach (var (dr, dc) in BubbleGrid.GetNeighborOffsets(r))
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= BubbleGrid.MAX_ROWS || nc < 0 || nc >= BubbleGrid.COLS_EVEN) continue;
                if (visited.Contains((nr, nc))) continue;
                if (grid[nr, nc] == null) continue;
                if (grid[nr, nc].Color != target.Color) continue;

                visited.Add((nr, nc));
                queue.Enqueue((nr, nc));
            }
        }

        return result.Count >= MIN_MATCH ? result : new List<(int, int)>();
    }

    // 0행과 연결되지 않은 버블 전체 탐색 (BFS from top row)
    public static List<(int row, int col)> FindFloating(Bubble[,] grid)
    {
        var connected = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();

        // 0행의 모든 버블을 시작점으로
        for (int c = 0; c < BubbleGrid.COLS_EVEN; c++)
        {
            if (grid[0, c] != null)
            {
                queue.Enqueue((0, c));
                connected.Add((0, c));
            }
        }

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            foreach (var (dr, dc) in BubbleGrid.GetNeighborOffsets(r))
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= BubbleGrid.MAX_ROWS || nc < 0 || nc >= BubbleGrid.COLS_EVEN) continue;
                if (connected.Contains((nr, nc))) continue;
                if (grid[nr, nc] == null) continue;

                connected.Add((nr, nc));
                queue.Enqueue((nr, nc));
            }
        }

        // 그리드 전체 버블 중 connected에 없는 것 = 부유
        var floating = new List<(int, int)>();
        for (int r = 0; r < BubbleGrid.MAX_ROWS; r++)
            for (int c = 0; c < BubbleGrid.COLS_EVEN; c++)
                if (grid[r, c] != null && !connected.Contains((r, c)))
                    floating.Add((r, c));

        return floating;
    }
}
```

- [ ] **Step 4: Test Runner에서 재실행 → 전부 PASS 확인**

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleMatchProcessor.cs Assets/Tests/
git commit -m "feat: add BubbleMatchProcessor with BFS match and floating detection"
```

---

## Task 4: BubbleProjectile - 비행 버블

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleProjectile.cs`

- [ ] **Step 1: BubbleProjectile.cs 작성**

```csharp
// Assets/Scripts/Gameplay/BubbleProjectile.cs
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BubbleProjectile : MonoBehaviour
{
    public BubbleColor Color { get; private set; }
    public event Action<Vector2> OnLanded;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float leftWallX = -4.0f;
    [SerializeField] private float rightWallX = 4.0f;

    private Vector2 velocity;
    private bool isFlying;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.48f;
        col.isTrigger = true;
    }

    public void Launch(BubbleColor color, Vector2 direction, SpriteRenderer visual)
    {
        Color = color;
        GetComponent<SpriteRenderer>().color = visual.color;
        velocity = direction.normalized * speed;
        isFlying = true;
    }

    private void Update()
    {
        if (!isFlying) return;

        Vector2 pos = transform.position;
        pos += velocity * Time.deltaTime;

        // 좌우 벽 반사
        if (pos.x <= leftWallX)
        {
            pos.x = leftWallX;
            velocity.x = Mathf.Abs(velocity.x);
        }
        else if (pos.x >= rightWallX)
        {
            pos.x = rightWallX;
            velocity.x = -Mathf.Abs(velocity.x);
        }

        transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFlying) return;
        if (other.CompareTag("Bubble") || other.CompareTag("TopWall"))
        {
            isFlying = false;
            OnLanded?.Invoke(transform.position);
        }
    }
}
```

- [ ] **Step 2: Bubble 태그 + TopWall 태그 Unity 에디터 등록**
  - Edit > Project Settings > Tags and Layers
  - "Bubble" 태그 추가
  - "TopWall" 태그 추가
  - Bubble 프리팹의 태그를 "Bubble"로 설정
  - 씬에 빈 오브젝트 "TopWall" 추가, BoxCollider2D 추가, 태그 "TopWall" 설정
    - Position: (0, gridOriginY + 0.5), Scale: (10, 0.1)

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleProjectile.cs
git commit -m "feat: add BubbleProjectile with wall reflection and landing trigger"
```

---

## Task 5: BubbleShooter - 입력, 조준선, 발사

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleShooter.cs`

- [ ] **Step 1: BubbleShooter.cs 작성**

```csharp
// Assets/Scripts/Gameplay/BubbleShooter.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleShooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform currentBubbleSlot;   // 현재 버블 표시 위치
    [SerializeField] private Transform nextBubbleSlot;      // 다음 버블 표시 위치
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private float leftWallX = -4.0f;
    [SerializeField] private float rightWallX = 4.0f;
    [SerializeField] private int trajectorySteps = 60;
    [SerializeField] private float minLaunchAngle = 10f;    // 수평에서 최소 각도

    public event System.Action<BubbleProjectile> OnFired;

    private BubbleColor currentColor;
    private BubbleColor nextColor;
    private bool canShoot = true;
    private LineRenderer lineRenderer;
    private bool isDragging;
    private Vector2 dragDirection;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    private void Start()
    {
        currentColor = RandomColor();
        nextColor = RandomColor();
        UpdateVisuals();
    }

    private void Update()
    {
        if (!canShoot) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shooterPos = transform.position;
            Vector2 dir = (mouseWorld - shooterPos).normalized;

            // 너무 수평이거나 아래로 향하면 최소 각도 고정
            float angle = Vector2.SignedAngle(Vector2.up, dir);
            if (Mathf.Abs(angle) > 90f - minLaunchAngle)
            {
                angle = Mathf.Sign(angle) * (90f - minLaunchAngle);
                dir = Quaternion.Euler(0, 0, -angle) * Vector2.up;
            }

            dragDirection = dir;
            DrawTrajectory(shooterPos, dir);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            lineRenderer.positionCount = 0;
            Fire(dragDirection);
        }
    }

    private void DrawTrajectory(Vector2 start, Vector2 dir)
    {
        var points = new Vector3[trajectorySteps];
        Vector2 pos = start;
        Vector2 vel = dir.normalized;
        float stepSize = 0.3f;

        for (int i = 0; i < trajectorySteps; i++)
        {
            points[i] = pos;
            pos += vel * stepSize;

            if (pos.x <= leftWallX) { pos.x = leftWallX; vel.x = Mathf.Abs(vel.x); }
            else if (pos.x >= rightWallX) { pos.x = rightWallX; vel.x = -Mathf.Abs(vel.x); }
        }

        lineRenderer.positionCount = trajectorySteps;
        lineRenderer.SetPositions(points);
    }

    public void Fire(Vector2 direction)
    {
        if (!canShoot) return;
        canShoot = false;

        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<BubbleProjectile>();
        proj.Launch(currentColor, direction, GetVisualForColor(currentColor));
        OnFired?.Invoke(proj);

        // 다음 버블로 교체
        currentColor = nextColor;
        nextColor = RandomColor();
        UpdateVisuals();
    }

    public void EnableShoot() => canShoot = true;

    private BubbleColor RandomColor() => (BubbleColor)Random.Range(0, (int)BubbleColor.Count);

    private void UpdateVisuals()
    {
        if (currentBubbleSlot != null)
            currentBubbleSlot.GetComponent<SpriteRenderer>().color = GetColorValue(currentColor);
        if (nextBubbleSlot != null)
            nextBubbleSlot.GetComponent<SpriteRenderer>().color = GetColorValue(nextColor);
    }

    private SpriteRenderer GetVisualForColor(BubbleColor color)
    {
        var sr = currentBubbleSlot.GetComponent<SpriteRenderer>();
        sr.color = GetColorValue(color);
        return sr;
    }

    private static UnityEngine.Color GetColorValue(BubbleColor color)
    {
        return color switch
        {
            BubbleColor.Red => UnityEngine.Color.red,
            BubbleColor.Blue => UnityEngine.Color.blue,
            BubbleColor.Green => UnityEngine.Color.green,
            BubbleColor.Yellow => UnityEngine.Color.yellow,
            BubbleColor.Purple => new UnityEngine.Color(0.6f, 0.2f, 0.8f),
            _ => UnityEngine.Color.white,
        };
    }
}
```

- [ ] **Step 2: LineRenderer 설정 (Unity 에디터)**
  - BubbleShooter 오브젝트 선택
  - LineRenderer > Width: 0.05
  - LineRenderer > Material: Sprites-Default
  - LineRenderer > Color: 흰색 반투명

- [ ] **Step 3: BubbleShooter 씬 오브젝트 세팅**
  - Hierarchy에 "BubbleShooter" 빈 오브젝트 추가, 화면 하단에 배치 (Y ≈ -8)
  - `BubbleShooter.cs` 컴포넌트 추가
  - 자식으로 "CurrentBubble" 오브젝트 (SpriteRenderer, 원 스프라이트) 생성
  - 자식으로 "NextBubble" 오브젝트 (SpriteRenderer, 원 스프라이트, Scale 0.7) 생성
  - 인스펙터에서 각 슬롯 연결

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooter.cs
git commit -m "feat: add BubbleShooter with drag aim and trajectory preview"
```

---

## Task 6: BubbleGameManager - 발사 카운터, 줄 추가, 게임오버

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleGameManager.cs`
- Modify: `Assets/Scripts/Core/Managers/GameManager.cs:36-116`

- [ ] **Step 1: BubbleGameManager.cs 작성**

```csharp
// Assets/Scripts/Gameplay/BubbleGameManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BubbleGameManager : InGameManager
{
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private float deathLineY = -7f;
    [SerializeField] private int shotsPerDrop = 5;

    private int shotCount;
    public int ShotsUntilDrop => shotsPerDrop - (shotCount % shotsPerDrop);

    public event System.Action<int> OnShotsUntilDropChanged;

    public override void Initialize()
    {
        base.Initialize();
        bubbleShooter.OnFired += OnBubbleFired;
    }

    private void OnBubbleFired(BubbleProjectile proj)
    {
        proj.OnLanded += OnBubbleLanded;
    }

    private void OnBubbleLanded(Vector2 worldPos)
    {
        // 착지한 프로젝타일 찾기 (이 콜백을 보낸 proj)
        // BubbleProjectile에서 self 전달하도록 시그니처 변경
    }

    public void OnBubbleLandedAt(BubbleProjectile proj, Vector2 worldPos)
    {
        // 1. 그리드에 배치
        var (row, col) = bubbleGrid.FindNearestEmpty(worldPos);
        var bubble = proj.GetComponent<Bubble>();
        if (bubble == null)
        {
            bubble = proj.gameObject.AddComponent<Bubble>();
            bubble.SetColor(proj.Color);
        }
        Destroy(proj.GetComponent<BubbleProjectile>());
        bubbleGrid.PlaceBubble(bubble, row, col);

        // 2. 매칭 검사
        var matches = BubbleMatchProcessor.FindMatches(bubbleGrid.Grid, row, col);
        if (matches.Count >= 3)
        {
            foreach (var (r, c) in matches)
                bubbleGrid.RemoveBubble(r, c);

            // 부유 버블 제거
            var floating = BubbleMatchProcessor.FindFloating(bubbleGrid.Grid);
            foreach (var (r, c) in floating)
                bubbleGrid.RemoveBubble(r, c);
        }

        // 3. 발사 카운터
        shotCount++;
        OnShotsUntilDropChanged?.Invoke(ShotsUntilDrop);

        // 4. 5발마다 줄 추가
        if (shotCount % shotsPerDrop == 0)
        {
            var newRow = bubbleGrid.GenerateRandomRow(0);
            bubbleGrid.AddRowAtTop(newRow);

            // 게임오버 판정
            if (bubbleGrid.HasBubbleBelowY(deathLineY))
            {
                GameManager.SetGameState(GameManager.GameState.GameOver);
                return;
            }
        }

        // 5. 다음 발사 허용
        bubbleShooter.EnableShoot();
    }

    public override void Clear()
    {
        base.Clear();
        if (bubbleShooter != null)
            bubbleShooter.OnFired -= OnBubbleFired;
    }
}
```

- [ ] **Step 2: BubbleProjectile OnLanded 시그니처 수정**

`Assets/Scripts/Gameplay/BubbleProjectile.cs`에서:
```csharp
// 변경 전
public event Action<Vector2> OnLanded;

// 변경 후
public event Action<BubbleProjectile, Vector2> OnLanded;

// OnTriggerEnter2D 내부도 변경
OnLanded?.Invoke(this, transform.position);
```

- [ ] **Step 3: BubbleShooter OnFired 연결 수정**

`Assets/Scripts/Gameplay/BubbleShooter.cs`의 Fire():
```csharp
proj.OnLanded += (p, pos) => { /* BubbleGameManager에서 처리 */ };
OnFired?.Invoke(proj);
```

- [ ] **Step 4: BubbleGameManager OnBubbleFired 수정**

```csharp
private void OnBubbleFired(BubbleProjectile proj)
{
    proj.OnLanded += OnBubbleLandedAt;
}
```

- [ ] **Step 5: GameManager.cs에 BubbleGameManager 등록**

`GameManager.cs`의 `InitializeCoreManagers()` 수정:
```csharp
// 기존 TODO 주석 교체
public BubbleGameManager BubbleManager { get; private set; }

// InitializeCoreManagers() 내부에 추가:
BubbleManager = RegisterManager<BubbleGameManager>(managerObjects);
```

`BubbleGameManager` 오브젝트에 "Manager" 태그 설정 (씬에서).

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleGameManager.cs Assets/Scripts/Gameplay/BubbleProjectile.cs Assets/Scripts/Core/Managers/GameManager.cs
git commit -m "feat: add BubbleGameManager with shot counter, row drop, and game over"
```

---

## Task 7: UI - HUDPanel + GameOverPanel

**Files:**
- Create: `Assets/Scripts/UI/Elements/HUDPanel.cs`
- Create: `Assets/Scripts/UI/Elements/GameOverPanel.cs`
- Modify: `Assets/Scripts/Defines/UIElementEnums.cs`
- Modify: `Assets/Scripts/Defines/Enums.cs`
- Modify: `Assets/Scripts/UI/Core/GameUIManager.cs`

- [ ] **Step 1: UIElementEnums.cs 업데이트**

```csharp
// Assets/Scripts/Defines/UIElementEnums.cs
public enum UIElementEnums
{
    HUDPanel = 0,
    GameOverPanel = 1,
}
```

- [ ] **Step 2: Enums.cs 업데이트**

```csharp
// Assets/Scripts/Defines/Enums.cs
public enum BgmClipId
{
    InGame,
}

public enum SfxClipId
{
    None,
    BubbleShoot,
    BubblePop,
    BubbleLand,
}
```

- [ ] **Step 3: HUDPanel.cs 작성**

```csharp
// Assets/Scripts/UI/Elements/HUDPanel.cs
using UnityEngine;
using TMPro;

public class HUDPanel : UIElement
{
    [SerializeField] private TextMeshProUGUI shotsUntilDropText;

    public override void Initialize()
    {
        var bubbleManager = GameManager.GetComponent<BubbleGameManager>();
        if (bubbleManager == null)
            bubbleManager = FindObjectOfType<BubbleGameManager>();

        if (bubbleManager != null)
            bubbleManager.OnShotsUntilDropChanged += UpdateDisplay;

        UpdateDisplay(5);
    }

    private void UpdateDisplay(int shotsUntilDrop)
    {
        if (shotsUntilDropText != null)
            shotsUntilDropText.text = $"다음 줄: {shotsUntilDrop}발";
    }
}
```

- [ ] **Step 4: GameOverPanel.cs 작성**

```csharp
// Assets/Scripts/UI/Elements/GameOverPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : UIElement
{
    [SerializeField] private Button restartButton;

    public override void Initialize()
    {
        restartButton?.onClick.AddListener(() => GameManager.RestartGame());
        Hide();
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }
}
```

- [ ] **Step 5: GameUIManager.cs 업데이트**

```csharp
// GameUIManager.cs Initialize() 내 TODO 주석 교체:
GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, () =>
{
    ShowUIElement(UIElementEnums.HUDPanel);
    HideUIElement(UIElementEnums.GameOverPanel);
});

GameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, () =>
{
    StartCoroutine(ShowDelayed(UIElementEnums.GameOverPanel, 1.0f));
});
```

- [ ] **Step 6: Unity 에디터에서 UI 프리팹 연결**
  - UIManager > SafeAreaPanel 하위에 HUDPanel, GameOverPanel Canvas 오브젝트 생성
  - 각 패널에 스크립트 추가 및 텍스트/버튼 연결
  - `GameUIManager`의 `uiElements` 리스트에 순서대로 연결 (0: HUDPanel, 1: GameOverPanel)

- [ ] **Step 7: 커밋**

```bash
git add Assets/Scripts/UI/Elements/ Assets/Scripts/Defines/UIElementEnums.cs Assets/Scripts/Defines/Enums.cs Assets/Scripts/UI/Core/GameUIManager.cs
git commit -m "feat: add HUD and GameOver UI panels"
```

---

## Task 8: 씬 마무리 세팅 + 통합 테스트

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity`

- [ ] **Step 1: 씬 최종 오브젝트 구성 확인**

```
SampleScene
├── GameManager          [GameManager, Manager 태그]
├── GameDataManager      [GameDataManager]
├── SoundManager         [SoundManager]
│   └── AudioSourcePlayer
├── AdsManager           [AdsManager]
├── BubbleGrid           [BubbleGrid] ← bubblePrefab 연결
├── BubbleShooter        [BubbleShooter, LineRenderer]
│   ├── CurrentBubble    [SpriteRenderer]
│   └── NextBubble       [SpriteRenderer]
├── TopWall              [BoxCollider2D, TopWall 태그]
├── DeathLine            [없어도 됨, BubbleGameManager의 deathLineY로 판정]
├── UIManager            [Canvas, GameUIManager, Manager 태그]
│   └── SafeAreaPanel
│       ├── HUDPanel
│       └── GameOverPanel
└── EventSystem
```

- [ ] **Step 2: 카메라 설정**
  - Main Camera > Projection: Orthographic
  - Size: 9 (세로 18 유닛)
  - 화면 비율 9:16 → 가로 약 10.1 유닛
  - BubbleGrid gridOriginX = -3.5, 8 cols → X 범위 -3.5 ~ 3.5 ✓
  - BubbleShooter Y ≈ -7.5, TopWall Y ≈ 7.5

- [ ] **Step 3: 플레이 테스트 체크리스트**
  - [ ] 버블 5행 초기 생성 확인
  - [ ] 드래그로 조준선 표시 확인
  - [ ] 발사 시 버블이 직선 + 벽 반사 이동 확인
  - [ ] 착지 시 그리드에 스냅 확인
  - [ ] 같은 색 3개 이상 → 제거 확인
  - [ ] 부유 버블 → 함께 제거 확인
  - [ ] 5발마다 줄 추가 확인
  - [ ] HUD 카운터 업데이트 확인
  - [ ] 버블이 데스라인 아래로 → GameOver 패널 표시 확인
  - [ ] 재시작 버튼 동작 확인

- [ ] **Step 4: 최종 커밋**

```bash
git add Assets/Scenes/SampleScene.unity
git commit -m "feat: complete bubble shooter core loop scene setup"
git push origin master
```

---

## 스펙 커버리지 확인

| 요구사항 | 구현 Task |
|---|---|
| 세로 화면 | Task 8 (카메라 설정) |
| 드래그 조준 + 발사 | Task 5 (BubbleShooter) |
| 좌우 벽 반사 | Task 4 (BubbleProjectile) + Task 5 (조준선) |
| 3개 이상 같은 색 제거 | Task 3 (BubbleMatchProcessor) |
| 부유 버블 제거 | Task 3 (FindFloating) |
| 5발마다 한 줄 내려옴 | Task 6 (BubbleGameManager) |
| 다음 버블 미리보기 | Task 5 (nextBubbleSlot) |
| 하단 라인 게임오버 | Task 6 (HasBubbleBelowY) |
| 무한 생존 | Task 2 (GenerateRandomRow 무한 생성) |
| 재시작 | Task 7 (GameOverPanel + GameManager.RestartGame) |
