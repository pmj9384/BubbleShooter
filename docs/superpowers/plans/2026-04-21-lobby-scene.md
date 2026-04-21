# Lobby Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** LobbyScene(빌드 인덱스 0)을 만들어 게임 진입점으로 사용하고, 플레이/옵션 버튼을 동작시키며 상점·스킨·에너지·재화는 UI 배치만 한다.

**Architecture:** LobbyScene은 LobbyManager 하나로 관리한다. GameManager 없이 독립 동작하며, SoundManager(PersistentMonoSingleton)는 LobbyScene에서 생성되어 SampleScene까지 유지된다. GoToTitle()은 "LobbyScene"을 로드하도록 수정한다.

**Tech Stack:** Unity 2D, C#, SceneManager, MCP Unity (씬/오브젝트 조작)

---

## 파일 구조

| 파일 | 역할 |
|---|---|
| Create: `Assets/Scripts/Lobby/LobbyManager.cs` | 버튼 연결, 씬 이동 |
| Create: `Assets/Scripts/Lobby/LobbySettingsPanel.cs` | BGM/SFX 슬라이더, SoundManager 직접 연결 |
| Modify: `Assets/Scripts/Core/Managers/GameManager.cs:156-160` | GoToTitle() → LobbyScene 로드 |
| Create: `Assets/Scenes/LobbyScene.unity` | MCP로 씬 생성 |

---

## Task 1: 스크립트 작성 — LobbyManager + LobbySettingsPanel

**Files:**
- Create: `Assets/Scripts/Lobby/LobbyManager.cs`
- Create: `Assets/Scripts/Lobby/LobbySettingsPanel.cs`

- [ ] **Step 1: LobbyManager.cs 작성**

```csharp
// Assets/Scripts/Lobby/LobbyManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;
    [SerializeField] private LobbySettingsPanel settingsPanel;

    private void Start()
    {
        playButton.onClick.AddListener(() => SceneManager.LoadScene("SampleScene"));
        optionsButton.onClick.AddListener(() => settingsPanel.Show());
        shopButton.interactable = false;
        skinButton.interactable = false;
    }
}
```

- [ ] **Step 2: LobbySettingsPanel.cs 작성**

```csharp
// Assets/Scripts/Lobby/LobbySettingsPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class LobbySettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        gameObject.SetActive(false);
        closeButton.onClick.AddListener(Hide);
        bgmSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetSfxVolume(v));
    }

    public void Show()
    {
        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.bgmVolume);
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.sfxVolume);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
```

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Lobby/
git commit -m "feat: LobbyManager, LobbySettingsPanel 스크립트 추가"
```

---

## Task 2: GameManager.GoToTitle() 수정

**Files:**
- Modify: `Assets/Scripts/Core/Managers/GameManager.cs:156-160`

- [ ] **Step 1: GoToTitle() 수정**

`Assets/Scripts/Core/Managers/GameManager.cs`에서:

```csharp
// 변경 전
public void GoToTitle()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

// 변경 후
public void GoToTitle()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("LobbyScene");
}
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/Core/Managers/GameManager.cs
git commit -m "fix: GoToTitle()을 LobbyScene 로드로 수정"
```

---

## Task 3: LobbyScene 생성 + 기본 오브젝트

**MCP Unity 사용**

- [ ] **Step 1: LobbyScene 생성**

MCP `create_scene` 호출:
```json
{ "sceneName": "LobbyScene", "scenePath": "Assets/Scenes/LobbyScene.unity" }
```

- [ ] **Step 2: Canvas 생성**

MCP `update_gameobject` — "Canvas" 생성 후 `Canvas` + `CanvasScaler` + `GraphicRaycaster` 컴포넌트 설정:
```json
{ "objectPath": "Canvas", "gameObjectData": { "name": "Canvas" } }
```
`Canvas` 컴포넌트:
```json
{ "renderMode": 0 }
```
`CanvasScaler` 컴포넌트:
```json
{ "uiScaleMode": 1, "referenceResolution": { "x": 1080, "y": 1920 }, "matchWidthOrHeight": 0.5 }
```

- [ ] **Step 3: SafeAreaPanel 생성**

MCP `update_gameobject` — Canvas 하위에 "SafeAreaPanel":
```json
{ "objectPath": "Canvas/SafeAreaPanel", "gameObjectData": { "name": "SafeAreaPanel" } }
```
`RectTransform` — 전체 스트레치:
```json
{
  "anchorMin": { "x": 0, "y": 0 },
  "anchorMax": { "x": 1, "y": 1 },
  "offsetMin": { "x": 0, "y": 0 },
  "offsetMax": { "x": 0, "y": 0 }
}
```

- [ ] **Step 4: LobbyManager 오브젝트 생성**

```json
{ "objectPath": "LobbyManager", "gameObjectData": { "name": "LobbyManager" } }
```
`LobbyManager` 컴포넌트 추가:
```json
{ "objectPath": "LobbyManager", "componentName": "LobbyManager" }
```

- [ ] **Step 5: EventSystem 생성**

```json
{ "objectPath": "EventSystem", "gameObjectData": { "name": "EventSystem" } }
```
`EventSystem` + `StandaloneInputModule` 컴포넌트 추가.

- [ ] **Step 6: SoundManager 오브젝트 생성**

SampleScene의 SoundManager를 참고해서 동일하게 LobbyScene에도 생성.
(PersistentMonoSingleton이므로 LobbyScene에서 한 번 생성되면 SampleScene까지 유지됨)

```json
{ "objectPath": "SoundManager", "gameObjectData": { "name": "SoundManager" } }
```
`SoundManager` 컴포넌트 추가.

---

## Task 4: UI 계층 구성 — TopBar / Center / BottomBar

**Files:**
- Modify: `Assets/Scenes/LobbyScene.unity`

- [ ] **Step 1: TopBar 생성 (상단 고정, h=90)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/TopBar", "gameObjectData": { "name": "TopBar" } }
```
RectTransform — top-stretch:
```json
{
  "anchorMin": { "x": 0, "y": 1 },
  "anchorMax": { "x": 1, "y": 1 },
  "pivot": { "x": 0.5, "y": 1 },
  "offsetMin": { "x": 0, "y": -90 },
  "offsetMax": { "x": 0, "y": 0 }
}
```

- [ ] **Step 2: TopBar — OptionsButton (우상단)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/TopBar/OptionsButton", "gameObjectData": { "name": "OptionsButton" } }
```
RectTransform — 우상단 앵커, 60x60:
```json
{
  "anchorMin": { "x": 1, "y": 0.5 },
  "anchorMax": { "x": 1, "y": 0.5 },
  "pivot": { "x": 1, "y": 0.5 },
  "anchoredPosition": { "x": -16, "y": 0 },
  "sizeDelta": { "x": 60, "y": 60 }
}
```
`Button` + `Image` 컴포넌트 추가. TMP 자식 텍스트 "⚙" 추가.

- [ ] **Step 3: TopBar — EnergyDisplay (중앙좌)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/TopBar/EnergyDisplay", "gameObjectData": { "name": "EnergyDisplay" } }
```
RectTransform — 좌앵커, 120x40:
```json
{
  "anchorMin": { "x": 0, "y": 0.5 },
  "anchorMax": { "x": 0, "y": 0.5 },
  "pivot": { "x": 0, "y": 0.5 },
  "anchoredPosition": { "x": 16, "y": 0 },
  "sizeDelta": { "x": 120, "y": 40 }
}
```
`TextMeshProUGUI` 컴포넌트 — text: "⚡ 5", color: gray (비활성 느낌).

- [ ] **Step 4: TopBar — CurrencyDisplay**

```json
{ "objectPath": "Canvas/SafeAreaPanel/TopBar/CurrencyDisplay", "gameObjectData": { "name": "CurrencyDisplay" } }
```
RectTransform — 중앙 좌측 (EnergyDisplay 오른쪽):
```json
{
  "anchorMin": { "x": 0, "y": 0.5 },
  "anchorMax": { "x": 0, "y": 0.5 },
  "pivot": { "x": 0, "y": 0.5 },
  "anchoredPosition": { "x": 148, "y": 0 },
  "sizeDelta": { "x": 120, "y": 40 }
}
```
`TextMeshProUGUI` — text: "💰 0", color: gray.

- [ ] **Step 5: Center — TitleText**

```json
{ "objectPath": "Canvas/SafeAreaPanel/TitleText", "gameObjectData": { "name": "TitleText" } }
```
RectTransform — 중앙, 전체 스트레치에서 수직 중앙:
```json
{
  "anchorMin": { "x": 0.1, "y": 0.4 },
  "anchorMax": { "x": 0.9, "y": 0.7 },
  "offsetMin": { "x": 0, "y": 0 },
  "offsetMax": { "x": 0, "y": 0 }
}
```
`TextMeshProUGUI` — text: "BUBBLE SHOOTER", fontSize: 72, alignment: Center.

- [ ] **Step 6: BottomBar 생성 (하단 고정, h=150)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/BottomBar", "gameObjectData": { "name": "BottomBar" } }
```
RectTransform — bottom-stretch:
```json
{
  "anchorMin": { "x": 0, "y": 0 },
  "anchorMax": { "x": 1, "y": 0 },
  "pivot": { "x": 0.5, "y": 0 },
  "offsetMin": { "x": 0, "y": 0 },
  "offsetMax": { "x": 0, "y": 150 }
}
```

- [ ] **Step 7: BottomBar — ShopButton (좌)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/BottomBar/ShopButton", "gameObjectData": { "name": "ShopButton" } }
```
RectTransform:
```json
{
  "anchorMin": { "x": 0, "y": 0.5 },
  "anchorMax": { "x": 0, "y": 0.5 },
  "pivot": { "x": 0, "y": 0.5 },
  "anchoredPosition": { "x": 30, "y": 0 },
  "sizeDelta": { "x": 100, "y": 100 }
}
```
`Button` + `Image`. TMP 자식 "🛒".

- [ ] **Step 8: BottomBar — PlayButton (중앙, 크게)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/BottomBar/PlayButton", "gameObjectData": { "name": "PlayButton" } }
```
RectTransform:
```json
{
  "anchorMin": { "x": 0.5, "y": 0.5 },
  "anchorMax": { "x": 0.5, "y": 0.5 },
  "pivot": { "x": 0.5, "y": 0.5 },
  "anchoredPosition": { "x": 0, "y": 0 },
  "sizeDelta": { "x": 140, "y": 140 }
}
```
`Button` + `Image`. TMP 자식 "▶ PLAY".

- [ ] **Step 9: BottomBar — SkinButton (우)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/BottomBar/SkinButton", "gameObjectData": { "name": "SkinButton" } }
```
RectTransform:
```json
{
  "anchorMin": { "x": 1, "y": 0.5 },
  "anchorMax": { "x": 1, "y": 0.5 },
  "pivot": { "x": 1, "y": 0.5 },
  "anchoredPosition": { "x": -30, "y": 0 },
  "sizeDelta": { "x": 100, "y": 100 }
}
```
`Button` + `Image`. TMP 자식 "🎨".

---

## Task 5: LobbySettingsPanel UI 구성

**Files:**
- Modify: `Assets/Scenes/LobbyScene.unity`

- [ ] **Step 1: LobbySettingsPanel 오브젝트 생성 (기본 비활성)**

```json
{ "objectPath": "Canvas/SafeAreaPanel/LobbySettingsPanel", "gameObjectData": { "name": "LobbySettingsPanel", "activeSelf": false } }
```
RectTransform — 중앙 카드, 600x500:
```json
{
  "anchorMin": { "x": 0.5, "y": 0.5 },
  "anchorMax": { "x": 0.5, "y": 0.5 },
  "pivot": { "x": 0.5, "y": 0.5 },
  "anchoredPosition": { "x": 0, "y": 0 },
  "sizeDelta": { "x": 600, "y": 500 }
}
```
`Image` (반투명 배경) + `LobbySettingsPanel` 컴포넌트 추가.

- [ ] **Step 2: BgmSlider 생성**

Unity 에디터에서 직접: GameObject > UI > Slider
- 경로: `Canvas/SafeAreaPanel/LobbySettingsPanel/BgmSlider`
- 위치: anchoredPosition (0, 80), sizeDelta (400, 50)

- [ ] **Step 3: SfxSlider 생성**

Unity 에디터에서 직접: GameObject > UI > Slider
- 경로: `Canvas/SafeAreaPanel/LobbySettingsPanel/SfxSlider`
- 위치: anchoredPosition (0, 0), sizeDelta (400, 50)

- [ ] **Step 4: CloseButton 생성**

```json
{ "objectPath": "Canvas/SafeAreaPanel/LobbySettingsPanel/CloseButton", "gameObjectData": { "name": "CloseButton" } }
```
RectTransform:
```json
{
  "anchorMin": { "x": 0.5, "y": 0 },
  "anchorMax": { "x": 0.5, "y": 0 },
  "pivot": { "x": 0.5, "y": 0 },
  "anchoredPosition": { "x": 0, "y": 40 },
  "sizeDelta": { "x": 200, "y": 60 }
}
```
`Button` + `Image`. TMP 자식 "닫기".

---

## Task 6: Inspector 연결 + 빌드 세팅

- [ ] **Step 1: LobbyManager Inspector 연결**

씬에서 LobbyManager 오브젝트 선택 → Inspector:
- `playButton` → BottomBar/PlayButton
- `optionsButton` → TopBar/OptionsButton
- `shopButton` → BottomBar/ShopButton
- `skinButton` → BottomBar/SkinButton
- `settingsPanel` → LobbySettingsPanel

- [ ] **Step 2: LobbySettingsPanel Inspector 연결**

LobbySettingsPanel 오브젝트 선택 → Inspector:
- `bgmSlider` → BgmSlider
- `sfxSlider` → SfxSlider
- `closeButton` → CloseButton

- [ ] **Step 3: 빌드 세팅 등록**

File > Build Settings:
1. "Add Open Scenes"로 LobbyScene 추가 → 인덱스 0
2. SampleScene 인덱스 1 확인

- [ ] **Step 4: LobbyScene 저장**

MCP `save_scene` 호출.

- [ ] **Step 5: 플레이 테스트**
  - [ ] LobbyScene 실행 → TopBar/BottomBar UI 표시 확인
  - [ ] PlayButton → SampleScene 로드 확인
  - [ ] OptionsButton → LobbySettingsPanel 표시 확인
  - [ ] BGM/SFX 슬라이더 동작 확인
  - [ ] CloseButton → 패널 숨김 확인
  - [ ] ShopButton/SkinButton 비활성화 표시 확인
  - [ ] SampleScene에서 Pause → HomeButton → LobbyScene 복귀 확인

- [ ] **Step 6: 최종 커밋**

```bash
git add Assets/Scenes/LobbyScene.unity Assets/Scripts/Lobby/
git commit -m "feat: LobbyScene 구현 - 로비 UI, 플레이/옵션 버튼 연결"
git push origin proto
```
