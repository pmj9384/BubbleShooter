# Skin Gacha System Design

**Date**: 2026-05-27  
**Scope**: 스킨 소유 시스템 + 가챠 ShopScreen 구현

---

## 목표

`SkinUserData.Load()`의 전체 소유 임시처리를 제거하고, 코인으로 가챠를 뽑아 스킨을 해금하는 시스템을 구현한다. SkinScreen은 소유 스킨만 장착 가능, 미소유 스킨은 잠금 표시.

---

## 변경 파일 / 신규 파일

| 파일 | 변경 유형 |
|------|-----------|
| `SkinUserData.cs` | 수정 |
| `SkinItemUI.cs` | 수정 |
| `ShopScreen.cs` | 수정 (빈 껍데기 → 가챠 UI) |
| `GachaService.cs` | 신규 |
| `GachaResultPopup.cs` | 신규 (UIPopup 상속) |

---

## Part 1. SkinUserData

### Load() 변경

새 유저(saveData 없음) 기본값: `Common_GreenNature` 1개만 소유 및 장착.

```csharp
public void Load()
{
    ownedSkinIds.Clear();
    var defaultSkin = DataTableManager.SkinDataTable.Get("Common_GreenNature");
    if (defaultSkin != null) ownedSkinIds.Add(defaultSkin.SkinId);
    equippedSkinId = defaultSkin?.SkinId;
}
```

### Unlock() 추가

```csharp
public bool Unlock(string skinId)
{
    if (IsOwned(skinId)) return false;
    ownedSkinIds.Add(skinId);
    return true;
}
```

> `Save()`는 ShopScreen에서 가챠 완료 후 명시적으로 호출한다.

---

## Part 2. GachaService

SRP에 따라 가챠 추첨 로직을 별도 클래스로 분리. 코인 차감 및 Unlock 호출은 ShopScreen이 담당.

```csharp
public class GachaService
{
    public const int SingleCost = 100;
    public const int TenCost = 900;

    // 가중치 랜덤으로 스킨 1개 반환
    public SkinDataTable.SkinRawData DrawOne()

    // DrawOne() 10회 반복 (중복 허용)
    public List<SkinDataTable.SkinRawData> DrawTen()
}
```

### 가중치 알고리즘

`SkinDataTable.All`의 `GachaRate` 값으로 누적합 방식:

```
Random.value → [0, 1) 범위
누적합 순서로 첫 번째로 초과하는 스킨 반환
```

GachaRate 총합 검증: Common 0.60 + Rare 0.30 + Epic 0.10 = **1.00** ✅

### 예외 처리

- 총합이 1.0이 아닐 경우: 마지막 스킨 반환 (폴백)
- 테이블이 비어있을 경우: null 반환, ShopScreen에서 처리

---

## Part 3. ShopScreen

빈 껍데기를 가챠 UI로 교체.

### Inspector 슬롯

```csharp
[SerializeField] private Button singleDrawButton;
[SerializeField] private Button tenDrawButton;
[SerializeField] private TMP_Text coinText;
[SerializeField] private GachaResultPopup resultPopup;
```

### 동작 흐름

1. `Open()` 시 코인 잔액 표시, 버튼 활성화 갱신
2. 1회 뽑기 클릭 → 코인 100 확인 → `GachaService.DrawOne()` → `SkinUserData.Unlock()` → `resultPopup.Show(results)` → Save
3. 10회 뽑기 클릭 → 코인 900 확인 → `GachaService.DrawTen()` → 각각 `Unlock()` → `resultPopup.Show(results)` → Save
4. 코인 부족 시 버튼 비활성화

### 코인 이벤트

`CoinUserData.OnCoinsChanged` 구독 → 코인 변경 시 버튼 상태 자동 갱신

---

## Part 4. GachaResultPopup

`UIPopup` 상속. 1회/10회 결과를 같은 팝업으로 처리.

```csharp
public void Show(List<SkinDataTable.SkinRawData> results)
```

### UI 구성

```
┌─────────────────────────┐
│   🎉 뽑기 결과!         │
│                         │
│  [ScrollView - Grid]    │
│  각 슬롯:               │
│    스킨 이미지          │
│    스킨 이름            │
│    등급 (Common/Rare/…) │
│    NEW! 뱃지 (신규 해금)│
│                         │
│       [  확인  ]        │
└─────────────────────────┘
```

- 1회 결과: 그리드에 1개 슬롯
- 10회 결과: 그리드에 10개 슬롯 (스크롤)
- NEW! 뱃지: `Unlock()` 반환값이 `true`인 항목에만 표시

---

## Part 5. SkinItemUI — 잠금 상태 추가

### Refresh() 3-상태 로직

```
IsOwned? → No  → button.interactable = false, text = "잠금"
          → Yes → IsEquipped?
                  → Yes → button.interactable = false, text = "장착중"
                  → No  → button.interactable = true,  text = "장착"
```

코드 추가 없음 — `Refresh()` 수정만.

---

## 미포함 (다음 태스크)

- 가챠 연출 애니메이션 (박스 열기 등)
- 광고 뽑기 (애니멀 브레이크아웃의 `GachaSingleAdsButton` 상당)
- 스킨별 가격 차등 (현재는 동일 확률표로 통일)
