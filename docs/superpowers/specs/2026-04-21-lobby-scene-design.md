# Lobby Scene Design

**목표:** 게임 진입 전 로비 화면 구성. 플레이 버튼으로 게임 씬으로 이동하고, 상점/스킨/에너지/재화는 나중에 기능을 붙일 수 있도록 UI 구조만 먼저 잡는다.

---

## 씬 구성

| 씬 | 빌드 인덱스 | 역할 |
|---|---|---|
| LobbyScene | 0 | 진입점, 로비 UI |
| SampleScene | 1 | 게임 플레이 |

---

## UI 구조

```
[LobbyScene]
└── Canvas (Screen Space - Overlay)
    └── SafeAreaPanel
        ├── TopBar (anchor: top-stretch, h=90)
        │   ├── OptionsButton (우상단, 60x60) → LobbySettingsPanel Show
        │   ├── EnergyDisplay (텍스트 "⚡ 5", 비활성)
        │   └── CurrencyDisplay (텍스트 "💰 0", 비활성)
        ├── Center (anchor: stretch)
        │   └── TitleText ("BUBBLE SHOOTER", 큰 폰트)
        └── BottomBar (anchor: bottom-stretch, h=150)
            ├── ShopButton (좌, interactable=false)
            ├── PlayButton (중앙, 크게) → SampleScene 로드
            └── SkinButton (우, interactable=false)

└── LobbySettingsPanel (기본 비활성)
    ├── BGM Slider → SoundManager.SetBgmVolume
    ├── SFX Slider → SoundManager.SetSfxVolume
    └── CloseButton → Hide
```

---

## 스크립트

### LobbyManager.cs
- `Assets/Scripts/Lobby/LobbyManager.cs`
- MonoBehaviour, 씬에 하나
- SerializeField: playButton, optionsButton, shopButton, skinButton, settingsPanel
- Start():
  - `playButton` → `SceneManager.LoadScene("SampleScene")`
  - `optionsButton` → `settingsPanel.Show()`
  - `shopButton.interactable = false`
  - `skinButton.interactable = false`

### LobbySettingsPanel.cs
- `Assets/Scripts/Lobby/LobbySettingsPanel.cs`
- MonoBehaviour, SoundManager에 직접 접근 (GameManager 없이)
- Show() / Hide() 메서드
- Show(): 슬라이더를 현재 SoundManager 볼륨값으로 초기화
- 슬라이더 onValueChanged → SoundManager.Instance.SetBgmVolume / SetSfxVolume

---

## GameManager 수정

`GoToTitle()` 수정:
```csharp
public void GoToTitle()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("LobbyScene");
}
```

빌드 세팅: LobbyScene(0) → SampleScene(1) 순서로 등록

---

## 비활성 항목 (나중에 기능 추가)

| 항목 | 현재 상태 | 나중에 필요한 것 |
|---|---|---|
| ShopButton | interactable=false | 상점 씬/패널 |
| SkinButton | interactable=false | 스킨 선택 패널 |
| EnergyDisplay | 텍스트만 ("⚡ 5") | 에너지 시스템 |
| CurrencyDisplay | 텍스트만 ("💰 0") | 재화 시스템 |

---

## 완료 기준

- [ ] LobbyScene 빌드 인덱스 0, SampleScene 인덱스 1
- [ ] 플레이 버튼 → SampleScene 로드
- [ ] 옵션 버튼 → BGM/SFX 슬라이더 패널 동작
- [ ] 상점/스킨 버튼 비활성화 표시
- [ ] 게임 씬 홈 버튼 → LobbyScene 복귀
- [ ] 게임 재시작 버튼 → SampleScene 재로드 (기존 유지)
