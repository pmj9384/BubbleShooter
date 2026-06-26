using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : UIElement
{
    [Header("TopBar")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI scoreText;

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

        var countManager = GameManager.Instance.CountManager;
        countManager.OnScoreChanged -= UpdateScore;
        countManager.OnScoreChanged += UpdateScore;
    }

    public override void Show()
    {
        topBar.SetActive(true);
        bottomBar.SetActive(true);
        gameObject.SetActive(true);
        UpdateScore(GameManager.Instance.CountManager.Score);
        UpdateDisplay();
    }

    public override void Hide()
    {
        topBar.SetActive(false);
        bottomBar.SetActive(false);
        gameObject.SetActive(false);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:N0}";
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
                BubbleType.Blackhole => Bubble.GetBlackholeSprite(),
                BubbleType.Wildcard  => Bubble.GetWildcardSprite(),
                BubbleType.Meteor    => Bubble.GetMeteorSprite(),
                _                   => Bubble.GetNormalSprite(upcoming[i]),
            };
        }
    }
}
