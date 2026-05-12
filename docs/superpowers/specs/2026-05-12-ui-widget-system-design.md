# UIWidget 시스템 설계

Date: 2026-05-12

## 목표

코인·에너지 같은 리소스 표시 UI를 독립 컴포넌트(Widget)로 만들어, 씬에 배치만 하면 알아서 동작하는 모듈형 시스템 구축. 새 위젯 추가 시 기존 코드를 수정하지 않는 구조(OCP).

---

## 레이아웃

- TopBar를 모든 아웃게임 스크린 상단에 항상 고정 (클래시 로얄 스타일)
- TopBarPanel은 위젯들의 위치를 잡아주는 컨테이너 역할만 수행
- BottomMenuPanel과 동일하게 SafeAreaPanel 안에 배치

---

## 클래스 구조

### UIWidget (신규, UIFramework/Core/)

```csharp
public abstract class UIWidget : MonoBehaviour
{
    protected virtual void OnEnable()  => Subscribe();
    protected virtual void OnDisable() => Unsubscribe();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
    public abstract void Refresh();
}
```

- `OnEnable`에서 이벤트 구독, `OnDisable`에서 해제 → 씬에서 켜고 끌 때 자동 처리
- `Refresh()`는 구독 전 초기값을 표시할 때 사용

### CoinWidget (OutGame/Widgets/)

```csharp
public class CoinWidget : UIWidget
{
    [SerializeField] private TMP_Text coinText;

    protected override void Subscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged += UpdateUI;
        Refresh();
    }

    protected override void Unsubscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged -= UpdateUI;
    }

    public override void Refresh()
        => UpdateUI(GameDataManager.Instance.PlayerAccountData.Coins);

    private void UpdateUI(int coins) => coinText.text = coins.ToString("N0");
}
```

### EnergyWidget (OutGame/Widgets/)

CoinWidget과 동일한 구조. `OnEnergyChanged` 이벤트 구독, 에너지/최대 에너지 형식(`5/5`)으로 표시.

### TopBarPanel (OutGame/Panels/)

```csharp
public class TopBarPanel : UIPanel { }
```

별도 로직 없음. HorizontalLayoutGroup으로 위젯 정렬만 담당.

---

## 데이터 흐름

```
PlayerAccountData.Coins setter 호출
    → OnCoinsChanged?.Invoke(value)
        → CoinWidget.UpdateUI(value)
            → coinText.text 갱신
```

TopBarPanel은 이 흐름에 관여하지 않음.

---

## PlayerAccountData 변경

```csharp
// 추가할 이벤트 및 프로퍼티
public event Action<int> OnCoinsChanged;
public event Action<int> OnEnergyChanged;

private int coins;
public int Coins
{
    get => coins;
    private set { coins = value; OnCoinsChanged?.Invoke(coins); }
}

private int energy;
public int Energy
{
    get => energy;
    private set { energy = value; OnEnergyChanged?.Invoke(energy); }
}

public int MaxEnergy { get; private set; } = 5;

public void AddCoins(int amount) => Coins += amount;
public bool TryConsumeEnergy()
{
    if (Energy <= 0) return false;
    Energy--;
    return true;
}
```

---

## PlayerAccountDataSave 변경

```csharp
public int coins;
public int energy;
public long lastEnergyRecoveryTime;
```

---

## 씬 계층 구조

```
UIManager (OutGameUIManager)
SafeAreaPanel
  TopBar (TopBarPanel)             ← UIPanel, 상단 고정
    CoinWidget (CoinWidget)        ← UIWidget
    EnergyWidget (EnergyWidget)    ← UIWidget
  ScreenCanvas
    LobbyScreen
    ShopScreen
    SkinScreen
  PopupCanvas
    OutGameSettingsPanel
```

---

## 새 위젯 추가 방법

1. `UIWidget` 상속 클래스 작성
2. 씬의 TopBar(또는 원하는 위치)에 게임오브젝트 배치
3. Inspector에서 TMP_Text 연결

기존 코드 수정 불필요.

---

## 범위 외 (이번 구현 제외)

- 에너지 시간 기반 자동 회복 타이머
- 에너지 부족 시 UI 팝업
- 인게임 → 아웃게임 코인 지급 연동
