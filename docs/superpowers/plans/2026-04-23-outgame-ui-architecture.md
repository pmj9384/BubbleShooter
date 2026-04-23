# OutGame UI Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 아웃게임 UI를 OutGameManager + UINavigator + PanelManager 구조로 재설계해 SOLID 원칙을 준수하는 확장 가능한 구조를 구축한다.

**Architecture:** LobbyScene 안에서 Screen(전체화면)과 Panel(팝업)을 분리해 관리한다. UINavigator가 Screen 전환을 담당하고 PanelManager가 팝업을 담당한다. OutGameManager가 두 매니저를 들고 씬을 초기화한다.

**Tech Stack:** Unity 6, C#, Unity UI (uGUI), SceneManager

---

## 파일 구조

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/OutGame/OutGameManager.cs` | 씬 코디네이터, UINavigator + PanelManager 초기화 |
| `Assets/Scripts/OutGame/Navigation/UINavigator.cs` | Screen 전환 관리 |
| `Assets/Scripts/OutGame/Navigation/BaseScreen.cs` | Screen 추상 클래스 |
| `Assets/Scripts/OutGame/Screens/LobbyScreen.cs` | 로비 화면 (PlayButton, ShopButton, SkinButton, OptionsButton) |
| `Assets/Scripts/OutGame/Screens/ShopScreen.cs` | 상점 화면 (placeholder) |
| `Assets/Scripts/OutGame/Screens/SkinScreen.cs` | 스킨 화면 (placeholder) |
| `Assets/Scripts/OutGame/Panels/PanelManager.cs` | 팝업 관리 |
| `Assets/Scripts/OutGame/Panels/BasePanel.cs` | Panel 추상 클래스 |
| `Assets/Scripts/OutGame/Panels/OutGameSettingsPanel.cs` | 설정 팝업 (BGM/SFX) |
| `Assets/Scripts/OutGame/HUD/TopBarHUD.cs` | 에너지/재화 상시표시 |

**삭제:**
- `Assets/Scripts/Lobby/LobbyManager.cs`
- `Assets/Scripts/Lobby/LobbySettingsPanel.cs`

---

### Task 1: 기반 추상 클래스 작성

**Files:**
- Create: `Assets/Scripts/OutGame/Navigation/BaseScreen.cs`
- Create: `Assets/Scripts/OutGame/Panels/BasePanel.cs`

- [ ] **Step 1: BaseScreen.cs 작성**

```csharp
using UnityEngine;

public abstract class BaseScreen : MonoBehaviour
{
    protected OutGameManager outGameManager;

    public void SetOutGameManager(OutGameManager outGameManager)
    {
        this.outGameManager = outGameManager;
    }

    public virtual void OnEnter() => gameObject.SetActive(true);
    public virtual void OnExit() => gameObject.SetActive(false);
}
```

- [ ] **Step 2: BasePanel.cs 작성**

```csharp
using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    protected OutGameManager outGameManager;

    public void SetOutGameManager(OutGameManager outGameManager)
    {
        this.outGameManager = outGameManager;
    }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
```

- [ ] **Step 3: Unity 컴파일 확인**

Unity Editor에서 컴파일 에러 없는지 확인.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/OutGame/Navigation/BaseScreen.cs Assets/Scripts/OutGame/Panels/BasePanel.cs
git commit -m "feat: BaseScreen, BasePanel 추상 클래스 추가"
```

---

### Task 2: UINavigator 작성

**Files:**
- Create: `Assets/Scripts/OutGame/Navigation/UINavigator.cs`

- [ ] **Step 1: UINavigator.cs 작성**

```csharp
using UnityEngine;

public class UINavigator : MonoBehaviour
{
    [SerializeField] private BaseScreen[] screens;

    public void Show<T>() where T : BaseScreen
    {
        foreach (var screen in screens)
        {
            if (screen is T)
                screen.OnEnter();
            else
                screen.OnExit();
        }
    }

    public void ShowLobby()
    {
        Show<LobbyScreen>();
    }

    public void Initialize(OutGameManager outGameManager)
    {
        foreach (var screen in screens)
            screen.SetOutGameManager(outGameManager);
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Navigation/UINavigator.cs
git commit -m "feat: UINavigator 추가"
```

---

### Task 3: PanelManager 작성

**Files:**
- Create: `Assets/Scripts/OutGame/Panels/PanelManager.cs`

- [ ] **Step 1: PanelManager.cs 작성**

```csharp
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private BasePanel[] panels;

    public void Show<T>() where T : BasePanel
    {
        foreach (var panel in panels)
        {
            if (panel is T)
                panel.Show();
        }
    }

    public void Hide<T>() where T : BasePanel
    {
        foreach (var panel in panels)
        {
            if (panel is T)
                panel.Hide();
        }
    }

    public void Initialize(OutGameManager outGameManager)
    {
        foreach (var panel in panels)
            panel.SetOutGameManager(outGameManager);
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Panels/PanelManager.cs
git commit -m "feat: PanelManager 추가"
```

---

### Task 4: OutGameManager 작성

**Files:**
- Create: `Assets/Scripts/OutGame/OutGameManager.cs`

- [ ] **Step 1: OutGameManager.cs 작성**

```csharp
using UnityEngine;

public class OutGameManager : MonoBehaviour
{
    [SerializeField] private UINavigator navigator;
    [SerializeField] private PanelManager panelManager;

    public UINavigator Navigator => navigator;
    public PanelManager PanelManager => panelManager;

    private void Start()
    {
        navigator.Initialize(this);
        panelManager.Initialize(this);
        navigator.ShowLobby();
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/OutGameManager.cs
git commit -m "feat: OutGameManager 추가"
```

---

### Task 5: Screen 구현체 작성

**Files:**
- Create: `Assets/Scripts/OutGame/Screens/LobbyScreen.cs`
- Create: `Assets/Scripts/OutGame/Screens/ShopScreen.cs`
- Create: `Assets/Scripts/OutGame/Screens/SkinScreen.cs`

- [ ] **Step 1: LobbyScreen.cs 작성**

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyScreen : BaseScreen
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;

    public override void OnEnter()
    {
        base.OnEnter();
        playButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        skinButton.onClick.RemoveAllListeners();

        playButton.onClick.AddListener(() => SceneManager.LoadScene("SampleScene"));
        optionsButton.onClick.AddListener(() => outGameManager.PanelManager.Show<OutGameSettingsPanel>());
        shopButton.interactable = false;
        skinButton.interactable = false;
    }
}
```

- [ ] **Step 2: ShopScreen.cs 작성 (placeholder)**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : BaseScreen
{
    [SerializeField] private Button homeButton;

    public override void OnEnter()
    {
        base.OnEnter();
        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(() => outGameManager.Navigator.ShowLobby());
    }
}
```

- [ ] **Step 3: SkinScreen.cs 작성 (placeholder)**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class SkinScreen : BaseScreen
{
    [SerializeField] private Button homeButton;

    public override void OnEnter()
    {
        base.OnEnter();
        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(() => outGameManager.Navigator.ShowLobby());
    }
}
```

- [ ] **Step 4: Unity 컴파일 확인**

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/OutGame/Screens/
git commit -m "feat: LobbyScreen, ShopScreen, SkinScreen 추가"
```

---

### Task 6: OutGameSettingsPanel 작성

**Files:**
- Create: `Assets/Scripts/OutGame/Panels/OutGameSettingsPanel.cs`

- [ ] **Step 1: OutGameSettingsPanel.cs 작성**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class OutGameSettingsPanel : BasePanel
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        bgmSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetSfxVolume(v));
    }

    public override void Show()
    {
        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.bgmVolume);
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.sfxVolume);
        base.Show();
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Panels/OutGameSettingsPanel.cs
git commit -m "feat: OutGameSettingsPanel 추가"
```

---

### Task 7: 구 스크립트 제거

**Files:**
- Delete: `Assets/Scripts/Lobby/LobbyManager.cs`
- Delete: `Assets/Scripts/Lobby/LobbySettingsPanel.cs`

- [ ] **Step 1: 구 스크립트 삭제**

```bash
rm Assets/Scripts/Lobby/LobbyManager.cs Assets/Scripts/Lobby/LobbyManager.cs.meta
rm Assets/Scripts/Lobby/LobbySettingsPanel.cs Assets/Scripts/Lobby/LobbySettingsPanel.cs.meta
```

- [ ] **Step 2: Unity Asset Refresh**

Unity Editor에서 `Assets > Refresh` 실행. Missing Script 경고 확인.

- [ ] **Step 3: 커밋**

```bash
git add -A
git commit -m "refactor: LobbyManager, LobbySettingsPanel 제거"
```

---

### Task 8: LobbyScene 씬 재구성

**Files:**
- Modify: `Assets/Scenes/LobbyScene.unity`

- [ ] **Step 1: OutGameManager GameObject 생성**

Unity Hierarchy에서 빈 오브젝트 생성: `OutGameManager`
- `OutGameManager.cs` 컴포넌트 추가

- [ ] **Step 2: UINavigator GameObject 생성**

빈 오브젝트 생성: `UINavigator`
- `UINavigator.cs` 컴포넌트 추가

- [ ] **Step 3: PanelManager GameObject 생성**

빈 오브젝트 생성: `PanelManager`
- `PanelManager.cs` 컴포넌트 추가

- [ ] **Step 4: Screen 오브젝트 구성**

기존 `SafeAreaPanel` 하위에:
- `LobbyScreen` 오브젝트 → `LobbyScreen.cs` 추가, 버튼 Inspector 연결
- `ShopScreen` 오브젝트 생성 → `ShopScreen.cs` 추가 (비활성)
- `SkinScreen` 오브젝트 생성 → `SkinScreen.cs` 추가 (비활성)

- [ ] **Step 5: Panel 오브젝트 구성**

기존 `LobbySettingsPanel` → `OutGameSettingsPanel.cs` 교체, Inspector 연결

- [ ] **Step 6: Inspector 연결**

`OutGameManager`:
- Navigator → UINavigator 오브젝트
- PanelManager → PanelManager 오브젝트

`UINavigator`:
- Screens → [LobbyScreen, ShopScreen, SkinScreen]

`PanelManager`:
- Panels → [OutGameSettingsPanel]

- [ ] **Step 7: LobbyManager 오브젝트 제거**

Hierarchy에서 `LobbyManager` GameObject 삭제.

- [ ] **Step 8: 씬 저장 및 커밋**

```bash
git add Assets/Scenes/LobbyScene.unity
git commit -m "feat: LobbyScene 아웃게임 UI 구조 재구성"
```

---

### Task 9: Build Settings + 플레이테스트

- [ ] **Step 1: Build Settings 조정**

`File > Build Settings`:
- LobbyScene → 인덱스 0
- SampleScene → 인덱스 1

- [ ] **Step 2: 플레이테스트 체크리스트**

- [ ] LobbyScene 진입 시 LobbyScreen 표시
- [ ] PlayButton → SampleScene 로드
- [ ] OptionsButton → OutGameSettingsPanel 팝업
- [ ] BGM/SFX 슬라이더 동작
- [ ] CloseButton → 팝업 닫힘
- [ ] 인게임 → 게임오버 → GoToTitle → LobbyScene 복귀
- [ ] 인게임 → PausePanel → 홈버튼 → LobbyScene 복귀

- [ ] **Step 3: 최종 커밋**

```bash
git add -A
git commit -m "feat: 아웃게임 UI 아키텍처 구현 완료"
```
