using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUIElement : UIElement
{
    [Header("TopBar")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private Button pauseButton;

    [Header("BottomBar")]
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Image[] previewBubbleImages; // 5개 (upcoming[0]=next, [1~4]=그 다음)

    private BubbleShooterController shooter;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        shooter = FindFirstObjectByType<BubbleShooterController>();
        if (shooter != null)
            shooter.OnFired += UpdateDisplay;

        pauseButton.onClick.AddListener(() =>
            gameManager.SetGameState(GameManager.GameState.GameStop));
    }

    public override void Show()
    {
        topBar.SetActive(true);
        bottomBar.SetActive(true);
        gameObject.SetActive(true);
        UpdateDisplay();
    }

    public override void Hide()
    {
        topBar.SetActive(false);
        bottomBar.SetActive(false);
        gameObject.SetActive(false);
    }

    private void UpdateDisplay()
    {
        if (shooter == null) return;

        if (countdownText != null)
            countdownText.text = $"다음 줄까지: {shooter.ShotsUntilNextRow}발";

        var colorMap = Bubble.GetColorMap();
        var upcoming = shooter.UpcomingColors;

        for (int i = 0; i < previewBubbleImages.Length; i++)
        {
            if (previewBubbleImages[i] == null) continue;
            previewBubbleImages[i].color = i < upcoming.Count
                ? colorMap[(int)upcoming[i]]
                : Color.gray;
        }
    }
}
