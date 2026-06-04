# BubbleShooter Task 5 — 조준 & 발사 설계

**날짜:** 2026-04-08

## 개요

플레이어가 드래그로 조준하고 손을 떼면 버블을 발사하는 시스템.
벽 반사 1회 포함한 꺾인 조준선(LineRenderer)과 착지 예측 동그라미를 제공.

## 오브젝트 구성

```
BubbleShooter (0, -4)
├── BubbleShooterController (스크립트)
├── LineRenderer (조준선)
├── CurrentBubble (발사 대기 버블, 슈터 위치)
└── NextBubble (+1.5, 0 오프셋, 다음 버블 미리보기)
```

## BubbleShooterController 동작

| 이벤트 | 동작 |
|--------|------|
| PointerDown | 조준선 활성화 |
| 드래그 중 | 방향 계산 → 각도 클램프 → LineRenderer 업데이트 |
| PointerUp | BubbleProjectile 생성 후 Launch(), 다음 버블로 교체 |

## 각도 제한

- 발사 방향 y < 0 (아래쪽) 불가
- 수평에서 최소 10도 위 (좌우 각 80도, 총 160도 범위)
- `Vector2.SignedAngle` 또는 방향벡터 y 클램프로 구현

## LineRenderer 경로 (3 포인트)

```
P0: 슈터 위치
P1: 벽 충돌 지점 (반사점)
    - 발사 방향이 벽에 닿을 때까지 레이캐스트 or 수학적 계산
P2: 반사 후 그리드 방향 끝점 (고정 길이)
```

반사점 계산: 발사 방향 x 성분이 있으면 좌/우 벽(x = leftWall or rightWall)까지 거리 계산.
x 성분이 없으면 직선(P0 → P2 2포인트).

## 착지 예측 동그라미

- P2 위치에서 `BubbleGrid.FindNearestEmpty()` 호출
- 해당 셀 WorldPosition에 반투명 동그라미 스프라이트 표시

## 참조 관계

```
BubbleShooterController
  → BubbleGrid.FindNearestEmpty()   (착지 예측)
  → BubbleGrid (leftWall, rightWall 상수 계산용)
  → BubbleProjectile.Launch()       (발사)
```

벽 좌표는 `BubbleGrid.gridOriginX`와 `COLS_EVEN * BUBBLE_DIAMETER` 기준으로 계산.

## 버블 교체 흐름

1. 발사 시 `CurrentBubble` 색 → Projectile에 적용
2. `NextBubble` 색 → `CurrentBubble`로 이동
3. 새 랜덤 색 → `NextBubble`에 할당

## Task 6 연동 고려

- `BubbleShooterController`는 발사 이벤트를 `Action OnFired`로 노출
- Task 6의 `BubbleGameManager`가 이 이벤트 구독해서 발사 카운터 관리
