# BubbleShooter Task 5 — 조준 & 발사 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 드래그 조준 + 벽 반사 LineRenderer + 착지 예측 + 다음 버블 미리보기가 포함된 BubbleShooterController 구현

**Architecture:** `BubbleShooterController` MonoBehaviour가 마우스/터치 입력을 받아 조준선을 업데이트하고, 손을 떼면 BubbleProjectile을 생성해 Launch()를 호출한다. BubbleGrid는 `GameManager.Instance.BubbleGrid`로 참조.

**Tech Stack:** Unity 2D, LineRenderer, CircleCollider2D, BubbleProjectile (기존), BubbleGrid (기존)

---

## 파일 구성

| 파일 | 역할 |
|------|------|
| `Assets/Scripts/Gameplay/BubbleShooterController.cs` | 신규 — 조준/발사 전체 제어 |
| `Assets/Scripts/Gameplay/BubbleProjectile.cs` | 기존 — 수정 없음 |
| `Assets/Scripts/Core/Managers/GameManager.cs` | 기존 — `BubbleShooter` 등록 추가 |

---

## Task 1: BubbleShooter 오브젝트 씬 배치 (MCP)

**Files:**
- Modify: `SampleScene` (MCP로 오브젝트 추가)

- [ ] **Step 1: BubbleShooter 루트 오브젝트 생성**

MCP `add_asset_to_scene` 또는 `update_gameobject`로 씬에 빈 GameObject 추가.
위치 (0, -4, 0), 이름 `BubbleShooter`, 태그 `Untagged`.

- [ ] **Step 2: LineRenderer 컴포넌트 추가**

`BubbleShooter`에 LineRenderer 컴포넌트 추가.
- `positionCount`: 3
- `startWidth` / `endWidth`: 0.05
- `useWorldSpace`: true
- Material: Default-Line (또는 Sprites/Default)
- 초기 `enabled = false`

- [ ] **Step 3: LandingIndicator 자식 오브젝트 생성**

`BubbleShooter` 자식으로 `LandingIndicator` 빈 GameObject 생성.
SpriteRenderer 추가, `Sprites/Circle` 스프라이트 할당, 색 = 흰색 반투명(alpha 0.5), scale (0.9, 0.9, 1).
초기 `enabled = false`.

- [ ] **Step 4: NextBubble 자식 오브젝트 생성**

`BubbleShooter` 자식으로 `NextBubble` 빈 GameObject 생성.
위치 `(1.5, 0, 0)`, scale `(0.7, 0.7, 1)`.
SpriteRenderer 추가 (Bubble과 같은 스프라이트).

- [ ] **Step 5: 씬 저장**

MCP `save_scene` 호출.

---

## Task 2: BubbleShooterController 기본 구조

**Files:**
- Create: `Assets/Scripts/Gameplay/BubbleShooterController.cs`

- [ ] **Step 1: 스크립트 생성**

```csharp
using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleShooterController : MonoBehaviour
{
    // 벽 X 좌표 (BubbleGrid.gridOriginX -3.5, 좌우 반 버블 여백)
    private const float LEFT_WALL  = -4.0f;
    private const float RIGHT_WALL =  4.0f;
    private const float SHOOT_SPEED = 12f;
    private const float MIN_ANGLE_FROM_HORIZONTAL = 10f; // 도(degree)
    private const float REFLECT_LINE_LENGTH = 20f;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private SpriteRenderer landingIndicator;
    [SerializeField] private SpriteRenderer nextBubbleRenderer;

    private LineRenderer lineRenderer;
    private BubbleGrid bubbleGrid;
    private BubbleColor currentColor;
    private BubbleColor nextColor;
    private bool isDragging;

    public event Action OnFired;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        bubbleGrid = GameManager.Instance.BubbleGrid;

        currentColor = RandomColor();
        nextColor = RandomColor();
        RefreshNextBubbleDisplay();
    }

    private BubbleColor RandomColor() =>
        (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.Count);

    private void RefreshNextBubbleDisplay()
    {
        if (nextBubbleRenderer != null)
        {
            var colorMap = Bubble.GetColorMap();
            nextBubbleRenderer.color = colorMap[(int)nextColor];
        }
    }
}
```

> `Bubble.GetColorMap()`은 Task 3에서 추가한다.

- [ ] **Step 2: BubbleShooterController를 BubbleShooter 오브젝트에 추가 (MCP)**

MCP `update_component`로 `BubbleShooter` 오브젝트에 `BubbleShooterController` 스크립트 추가.
`bubblePrefab`, `landingIndicator`, `nextBubbleRenderer` 필드는 Inspector에서 연결.

- [ ] **Step 3: Bubble.cs에 ColorMap public 접근자 추가**

`Assets/Scripts/Gameplay/Bubble.cs` 수정:

```csharp
// 기존 private static readonly 를 internal로 변경하고 접근자 추가
public static UnityEngine.Color[] GetColorMap() => ColorMap;
```

- [ ] **Step 4: 컴파일 확인**

MCP `recompile_scripts` 호출 후 `get_console_logs`로 에러 없는지 확인.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooterController.cs \
        Assets/Scripts/Gameplay/Bubble.cs
git commit -m "feat: add BubbleShooterController skeleton and expose Bubble.GetColorMap"
```

---

## Task 3: 드래그 입력 + 각도 클램프

**Files:**
- Modify: `Assets/Scripts/Gameplay/BubbleShooterController.cs`

- [ ] **Step 1: 입력 처리 및 방향 계산 메서드 추가**

`BubbleShooterController`에 추가:

```csharp
private void Update()
{
    if (Input.GetMouseButtonDown(0)) isDragging = true;
    if (Input.GetMouseButtonUp(0) && isDragging)
    {
        isDragging = false;
        lineRenderer.enabled = false;
        if (landingIndicator != null) landingIndicator.enabled = false;

        Vector2 dir = GetAimDirection();
        if (dir != Vector2.zero) Fire(dir);
        return;
    }

    if (isDragging)
    {
        Vector2 dir = GetAimDirection();
        if (dir != Vector2.zero) UpdateAimLine(dir);
    }
}

private Vector2 GetAimDirection()
{
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

    // 아래쪽 방향 차단 (y <= 0)
    if (dir.y <= 0) return Vector2.zero;

    // 수평에서 최소 MIN_ANGLE_FROM_HORIZONTAL도 위쪽 강제
    float minY = Mathf.Sin(MIN_ANGLE_FROM_HORIZONTAL * Mathf.Deg2Rad);
    if (dir.y < minY)
    {
        dir.y = minY;
        dir = dir.normalized;
    }
    return dir;
}
```

- [ ] **Step 2: 플레이 모드에서 입력 확인**

Unity Editor에서 Play. 화면 아래 드래그 시 `isDragging` 작동 여부 확인 (`Debug.Log` 임시 추가해도 됨).

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooterController.cs
git commit -m "feat: add drag input and angle clamp to BubbleShooterController"
```

---

## Task 4: LineRenderer 조준선 (반사 포함)

**Files:**
- Modify: `Assets/Scripts/Gameplay/BubbleShooterController.cs`

- [ ] **Step 1: UpdateAimLine 메서드 구현**

```csharp
private void UpdateAimLine(Vector2 dir)
{
    lineRenderer.enabled = true;
    Vector2 p0 = transform.position;

    Vector2 p1, p2;
    if (Mathf.Abs(dir.x) < 0.001f)
    {
        // 수직 발사 — 반사 없음
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

    UpdateLandingIndicator(p2);
}
```

- [ ] **Step 2: 플레이 모드에서 조준선 확인**

Play 후 드래그 시 꺾인 흰색 선이 그려지는지 확인.
벽을 향해 드래그할 때 반사점에서 꺾이는지 확인.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooterController.cs
git commit -m "feat: add reflected LineRenderer aim line"
```

---

## Task 5: 착지 예측 동그라미

**Files:**
- Modify: `Assets/Scripts/Gameplay/BubbleShooterController.cs`

- [ ] **Step 1: UpdateLandingIndicator 메서드 구현**

```csharp
private void UpdateLandingIndicator(Vector2 endPoint)
{
    if (landingIndicator == null || bubbleGrid == null) return;

    var (row, col) = bubbleGrid.FindNearestEmpty(endPoint);
    Vector2 worldPos = bubbleGrid.GetWorldPosition(row, col);
    landingIndicator.transform.position = worldPos;
    landingIndicator.enabled = true;
}
```

- [ ] **Step 2: 플레이 모드에서 착지 예측 확인**

드래그 시 그리드 셀 위에 반투명 동그라미가 표시되는지 확인.
조준 방향을 바꿀 때 동그라미가 따라 이동하는지 확인.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooterController.cs
git commit -m "feat: add landing prediction indicator"
```

---

## Task 6: 발사 + 버블 교체

**Files:**
- Modify: `Assets/Scripts/Gameplay/BubbleShooterController.cs`

- [ ] **Step 1: Fire 메서드 구현**

```csharp
private void Fire(Vector2 dir)
{
    // BubblePrefab 인스턴스 생성
    GameObject go = Instantiate(bubblePrefab, transform.position, Quaternion.identity);

    var bubble = go.GetComponent<Bubble>();
    bubble.SetColor(currentColor);

    // Collider를 isTrigger로 설정 (BubbleProjectile이 OnTriggerEnter2D 사용)
    var col = go.GetComponent<CircleCollider2D>();
    col.isTrigger = true;

    var proj = go.AddComponent<BubbleProjectile>();
    proj.Launch(currentColor, dir, SHOOT_SPEED, LEFT_WALL, RIGHT_WALL, bubbleGrid);

    // 버블 교체
    currentColor = nextColor;
    nextColor = RandomColor();
    RefreshNextBubbleDisplay();

    OnFired?.Invoke();
}
```

- [ ] **Step 2: 플레이 모드에서 발사 확인**

Play 후 드래그하고 손 떼면 버블이 발사되는지 확인.
벽 반사가 일어나는지 확인.
그리드 버블에 착지되는지 확인.

- [ ] **Step 3: 다음 버블 디스플레이 색 확인**

발사할 때마다 nextBubble 색이 바뀌는지 확인.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Gameplay/BubbleShooterController.cs
git commit -m "feat: implement fire and bubble color cycling in BubbleShooterController"
```

---

## Task 7: GameManager에 BubbleShooterController 등록

**Files:**
- Modify: `Assets/Scripts/Core/Managers/GameManager.cs`

- [ ] **Step 1: BubbleShooter 프로퍼티 및 등록 추가**

`GameManager.cs`의 `#region 핵심 매니저 시스템` 블록에 추가:

```csharp
public BubbleShooterController BubbleShooter { get; private set; }
```

`InitializeCoreManagers()`의 RegisterManager 호출 다음에 추가:

```csharp
BubbleShooter = RegisterManager<BubbleShooterController>(managerObjects);
```

> **주의:** `BubbleShooterController`는 `InGameManager`를 상속하지 않는 일반 MonoBehaviour이므로 `RegisterManager<T>`를 사용하려면 `InGameManager`를 상속하거나 별도 방법으로 등록해야 한다.
>
> **대안 (권장):** GameManager 수정 없이 `BubbleShooterController.Start()`에서 `GameManager.Instance.BubbleGrid`를 직접 참조. 이미 Task 2에서 이렇게 구현했으므로 이 Task는 스킵 가능.

- [ ] **Step 2: 최종 플레이 테스트**

전체 흐름 확인:
1. Play 시작
2. 드래그 → 꺾인 조준선 + 착지 예측 동그라미 표시
3. 손 뗌 → 버블 발사, 벽 반사, 그리드 착지
4. 매칭 3개 이상 → BubbleMatchProcessor가 제거
5. nextBubble 색 교체 확인

- [ ] **Step 3: 최종 커밋**

```bash
git add Assets/Scripts/Core/Managers/GameManager.cs
git commit -m "feat: complete BubbleShooter Task 5 - aim, reflect, fire"
```
