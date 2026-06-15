# BubbleShooter 개발 규칙

## MCP 씬 작업 검증

MCP로 씬 오브젝트를 조작(reparent, 위치/크기 설정 등)한 뒤에는 반드시 마지막에 `get_gameobject`로 결과를 재확인할 것.

- `worldPositionStays: false`로 reparent하면 자식들이 (0,0)으로 몰림
- Panel 크기가 자식보다 작으면 넘침
- 확인 후 이상 있으면 수정 → `save_scene`

## 커밋 메시지

항상 한국어로 작성.

## SRP

코딩 중 한 클래스에 책임이 섞이는 경우 구현 전에 먼저 지적할 것.
