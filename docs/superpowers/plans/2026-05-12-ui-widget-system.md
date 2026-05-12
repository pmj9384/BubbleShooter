# UIWidget 시스템 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 코인·에너지를 독립 UIWidget 컴포넌트로 만들어, 씬에 배치만 하면 PlayerAccountData 이벤트를 스스로 구독하고 표시하는 모듈형 UI 시스템 구축

**Architecture:** `UIWidget : MonoBehaviour` 베이스가 `OnEnable`/`OnDisable`에서 이벤트 구독·해제를 처리한다. `CoinWidget`, `EnergyWidget`이 이를 상속해 `PlayerAccountData`의 C# 이벤트를 직접 구독한다. `TopBarPanel : UIPanel`은 위젯들의 레이아웃 컨테이너 역할만 한다.

**Tech Stack:** Unity 2D, C#, TextMeshPro, UIFramework (기존 UIPanel/UIScreen 구조)

---

## 파일 구조

| 동작 | 경로 | 역할 |
|------|------|------|
| **생성** | `Assets/Scripts/UIFramework/Core/UIWidget.cs` | 이벤트 구독 생명주기 베이스 |
| **생성** | `Assets/Scripts/OutGame/Widgets/CoinWidget.cs` | 코인 표시 위젯 |
| **생성** | `Assets/Scripts/OutGame/Widgets/EnergyWidget.cs` | 에너지 표시 위젯 |
| **생성** | `Assets/Scripts/OutGame/Panels/TopBarPanel.cs` | 레이아웃 컨테이너 |
| **수정** | `Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountData.cs` | 코인·에너지 프로퍼티 + 이벤트 추가 |
| **수정** | `Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountDataSave.cs` | coins, energy 직렬화 필드 추가 |

---

## Task 1: 데이터 레이어 — PlayerAccountData 코인·에너지 추가

**Files:**
- Modify: `Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountData.cs`
- Modify: `Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountDataSave.cs`

- [ ] **Step 1: PlayerAccountDataSave에 직렬화 필드 추가**

`Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountDataSave.cs` 전체를 교체:

```csharp
using System;

[Serializable]
public class PlayerAccountDataSave
{
    public float bgmVolume;
    public float sfxVolume;
    public int bestScore;
    public int coins;
    public int energy;
}
```

- [ ] **Step 2: PlayerAccountData에 이벤트·프로퍼티·메서드 추가**

`Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountData.cs` 전체를 교체:

```csharp
using System;
using UnityEngine;

public class PlayerAccountData : ISaveLoad
{
    public DataSourceType SaveDataSouceType => DataSourceType.Local;

    public int BestScore { get; private set; }

    public bool TryUpdateBestScore(int score)
    {
        if (score <= BestScore) return false;
        BestScore = score;
        return true;
    }

    private float bgmVolume;
    public float BgmVolume
    {
        get => bgmVolume;
        set { bgmVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    private float sfxVolume;
    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnEnergyChanged;

    private int coins;
    public int Coins
    {
        get => coins;
        private set { coins = value; OnCoinsChanged?.Invoke(coins); }
    }

    public int MaxEnergy { get; } = 5;

    private int energy;
    public int Energy
    {
        get => energy;
        private set { energy = Mathf.Clamp(value, 0, MaxEnergy); OnEnergyChanged?.Invoke(energy); }
    }

    public void AddCoins(int amount) => Coins += amount;

    public bool TryConsumeEnergy()
    {
        if (Energy <= 0) return false;
        Energy--;
        return true;
    }

    public PlayerAccountData()
    {
        SaveLoadSystem.Instance.RegisterOnSaveAction(this);
    }

    public void Save()
    {
        var saveData = SaveLoadSystem.Instance.CurrentSaveData.playerAccountDataSave = new();
        saveData.bgmVolume = SoundManager.Instance.bgmVolume;
        saveData.sfxVolume = SoundManager.Instance.sfxVolume;
        saveData.bestScore = BestScore;
        saveData.coins = Coins;
        saveData.energy = Energy;
    }

    public void Load()
    {
        BgmVolume = 1f;
        SfxVolume = 1f;
        BestScore = 0;
        Coins = 0;
        Energy = MaxEnergy;
    }

    public void Load(PlayerAccountDataSave saveData)
    {
        if (saveData == null) { Load(); return; }
        BgmVolume = saveData.bgmVolume;
        SfxVolume = saveData.sfxVolume;
        BestScore = saveData.bestScore;
        Coins = saveData.coins;
        Energy = saveData.energy;
    }
}
```

- [ ] **Step 3: 컴파일 확인**

MCP 툴로 재컴파일:
```
mcp__mcp-unity__recompile_scripts
```

이어서 콘솔 로그 확인:
```
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개. `SoundManager` 관련 기존 경고 1개는 무시.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountData.cs
git add Assets/Scripts/Core/Managers/SaveLoad/PlayerAccountDataSave.cs
git commit -m "feat: PlayerAccountData에 코인·에너지 프로퍼티 및 이벤트 추가"
```

---

## Task 2: UIWidget 베이스 클래스

**Files:**
- Create: `Assets/Scripts/UIFramework/Core/UIWidget.cs`

- [ ] **Step 1: UIWidget.cs 생성**

```csharp
using UnityEngine;

public abstract class UIWidget : MonoBehaviour
{
    protected virtual void OnEnable() => Subscribe();
    protected virtual void OnDisable() => Unsubscribe();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
    public abstract void Refresh();
}
```

- [ ] **Step 2: 컴파일 확인**

```
mcp__mcp-unity__recompile_scripts
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/UIFramework/Core/UIWidget.cs
git commit -m "feat: UIWidget 베이스 클래스 추가 (UIFramework/Core)"
```

---

## Task 3: CoinWidget

**Files:**
- Create: `Assets/Scripts/OutGame/Widgets/CoinWidget.cs`

- [ ] **Step 1: Widgets 폴더 생성 확인 후 CoinWidget.cs 생성**

`Assets/Scripts/OutGame/Widgets/` 폴더가 없으면 생성 후:

```csharp
using TMPro;
using UnityEngine;

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

- [ ] **Step 2: 컴파일 확인**

```
mcp__mcp-unity__recompile_scripts
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Widgets/CoinWidget.cs
git commit -m "feat: CoinWidget 추가 — PlayerAccountData.OnCoinsChanged 구독"
```

---

## Task 4: EnergyWidget

**Files:**
- Create: `Assets/Scripts/OutGame/Widgets/EnergyWidget.cs`

- [ ] **Step 1: EnergyWidget.cs 생성**

```csharp
using TMPro;
using UnityEngine;

public class EnergyWidget : UIWidget
{
    [SerializeField] private TMP_Text energyText;

    protected override void Subscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnEnergyChanged += UpdateUI;
        Refresh();
    }

    protected override void Unsubscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnEnergyChanged -= UpdateUI;
    }

    public override void Refresh()
        => UpdateUI(GameDataManager.Instance.PlayerAccountData.Energy);

    private void UpdateUI(int energy)
        => energyText.text = $"{energy}/{GameDataManager.Instance.PlayerAccountData.MaxEnergy}";
}
```

- [ ] **Step 2: 컴파일 확인**

```
mcp__mcp-unity__recompile_scripts
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Widgets/EnergyWidget.cs
git commit -m "feat: EnergyWidget 추가 — PlayerAccountData.OnEnergyChanged 구독"
```

---

## Task 5: TopBarPanel

**Files:**
- Create: `Assets/Scripts/OutGame/Panels/TopBarPanel.cs`

- [ ] **Step 1: TopBarPanel.cs 생성**

```csharp
public class TopBarPanel : UIPanel { }
```

- [ ] **Step 2: 컴파일 확인**

```
mcp__mcp-unity__recompile_scripts
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/OutGame/Panels/TopBarPanel.cs
git commit -m "feat: TopBarPanel 추가 — 위젯 레이아웃 컨테이너"
```

---

## Task 6: 씬 구성 및 통합 검증

**LobbyScene 계층 목표:**
```
UIManager (OutGameUIManager, tag: UIManager)
SafeAreaPanel
  TopBar              ← TopBarPanel 컴포넌트
    CoinDisplay       ← CoinWidget 컴포넌트 + TMP_Text "0"
    EnergyDisplay     ← EnergyWidget 컴포넌트 + TMP_Text "5/5"
  ScreenCanvas
    LobbyScreen / ShopScreen / SkinScreen
  PopupCanvas
    OutGameSettingsPanel
```

- [ ] **Step 1: LobbyScene 열기**

```
mcp__mcp-unity__load_scene  { "scenePath": "Assets/Scenes/LobbyScene.unity" }
```

- [ ] **Step 2: 현재 씬 계층 확인**

```
mcp__mcp-unity__get_scene_info
```

SafeAreaPanel 하위에 TopBar가 없으면 Step 3으로. 이미 있으면 Step 4로.

- [ ] **Step 3: TopBar GameObject 생성 및 배치**

Unity Editor에서:
1. Hierarchy > SafeAreaPanel 우클릭 > Create Empty → 이름 `TopBar`
2. `TopBar`에 `TopBarPanel` 컴포넌트 추가
3. `TopBar`의 RectTransform: Anchor = Top Stretch, Pos Y = 0, Height = 80

또는 MCP:
```
mcp__mcp-unity__update_gameobject {
  "name": "TopBar",
  "parentName": "SafeAreaPanel",
  "components": ["TopBarPanel"]
}
```

- [ ] **Step 4: CoinDisplay GameObject 생성**

Unity Editor에서 `TopBar` 하위에:
1. UI > TextMeshPro 오브젝트 생성 → 이름 `CoinDisplay`
2. `CoinWidget` 컴포넌트 추가
3. Inspector에서 `Coin Text` 슬롯에 TextMeshPro 컴포넌트 연결

- [ ] **Step 5: EnergyDisplay GameObject 생성**

Unity Editor에서 `TopBar` 하위에:
1. UI > TextMeshPro 오브젝트 생성 → 이름 `EnergyDisplay`
2. `EnergyWidget` 컴포넌트 추가
3. Inspector에서 `Energy Text` 슬롯에 TextMeshPro 컴포넌트 연결

- [ ] **Step 6: GameDataManager 씬 존재 확인**

```
mcp__mcp-unity__get_scene_info
```

LobbyScene 루트에 `GameDataManager` 오브젝트가 있어야 한다. 없으면 Hierarchy에 빈 오브젝트 생성 후 `GameDataManager` 컴포넌트 추가. (`PersistentMonoSingleton`이므로 이후 씬 전환 시 유지됨)

- [ ] **Step 7: 플레이 모드 검증 — 초기값 표시**

Unity Editor에서 Play 진입 후:
- `CoinDisplay`가 "0" 표시하는지 확인
- `EnergyDisplay`가 "5/5" 표시하는지 확인
- Console에 NullReferenceException 없는지 확인

```
mcp__mcp-unity__get_console_logs
```
Expected: 에러 0개.

- [ ] **Step 8: 플레이 모드 검증 — 이벤트 반응**

CoinWidget.cs에 임시 테스트 메서드 추가:

```csharp
#if UNITY_EDITOR
[UnityEngine.ContextMenu("Test: Add 100 Coins")]
private void TestAddCoins()
    => GameDataManager.Instance.PlayerAccountData.AddCoins(100);
#endif
```

Unity Editor에서:
1. Play 모드 진입
2. Hierarchy에서 `CoinDisplay` 선택
3. Inspector 우상단 점 세 개 → `Test: Add 100 Coins` 클릭

Expected: `CoinDisplay` 텍스트가 "100"으로 즉시 갱신됨.

검증 후 `#if UNITY_EDITOR` 블록 제거.

- [ ] **Step 9: 씬 저장 및 커밋**

```
mcp__mcp-unity__save_scene
```

```bash
git add Assets/Scenes/LobbyScene.unity
git commit -m "feat: LobbyScene에 TopBar 위젯 시스템 구성 (CoinWidget, EnergyWidget)"
```

---

## 완료 기준

- [ ] LobbyScene 플레이 시 TopBar에 코인 "0", 에너지 "5/5" 표시
- [ ] `AddCoins()` 호출 즉시 CoinWidget 텍스트 갱신
- [ ] `TryConsumeEnergy()` 호출 즉시 EnergyWidget 텍스트 갱신
- [ ] 화면 전환(Lobby → Shop → Skin) 시 TopBar 유지, 위젯 정상 표시
- [ ] Console 에러 0개

---

## 범위 외 (이번 구현 제외)

- 에너지 시간 기반 자동 회복 타이머
- 에너지 부족 시 팝업
- 인게임 GameOver 시 코인 지급 연동
- 위젯 아이콘 디자인 (아이콘 없이 텍스트만으로 구현)
