using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : UIElement
{
    [Header("TopBar")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private Button pauseButton;

    [Header("BottomBar")]
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Image[] previewBubbleImages;

    private BubbleShooterController shooter;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        shooter = FindFirstObjectByType<BubbleShooterController>();
        if (shooter != null)
            shooter.OnFired += UpdateDisplay;

        pauseButton.onClick.AddListener(() => gameUIManager.PauseGame());
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

        var upcoming = shooter.UpcomingColors;
        var upcomingTypes = shooter.UpcomingTypes;
        int count = previewBubbleImages.Length;

        for (int i = 0; i < count; i++)
        {
            int imgIdx = count - 1 - i;
            if (previewBubbleImages[imgIdx] == null) continue;

            var image = previewBubbleImages[imgIdx];
            image.color = Color.white;

            if (i >= upcoming.Count)
            {
                image.sprite = null;
                image.color = Color.gray;
                continue;
            }

            var type = i < upcomingTypes.Count ? upcomingTypes[i] : BubbleType.Normal;
            image.sprite = type switch
            {
                BubbleType.Bomb     => Bubble.GetBombSprite(),
                BubbleType.Wildcard => Bubble.GetWildcardSprite(),
                _                   => Bubble.GetNormalSprite(upcoming[i]),
            };
        }
    }
}
