using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Image[] previewBubbleImages; // 5개 (upcoming[0]=next, [1~4]=그 다음)

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
        StopAllCoroutines();

        if (countdownText != null)
            countdownText.text = $"다음 줄까지: {shooter.ShotsUntilNextRow}발";

        var colorMap = Bubble.GetColorMap();
        var upcoming = shooter.UpcomingColors;
        var upcomingTypes = shooter.UpcomingTypes;
        var wildcardImages = new List<Image>();

        int count = previewBubbleImages.Length;
        for (int i = 0; i < count; i++)
        {
            int imgIdx = count - 1 - i; // 배열 오른쪽→왼쪽, upcoming은 왼쪽=다음
            if (previewBubbleImages[imgIdx] == null) continue;
            if (i >= upcoming.Count) { previewBubbleImages[imgIdx].color = Color.gray; continue; }

            var type = i < upcomingTypes.Count ? upcomingTypes[i] : BubbleType.Normal;
            if (type == BubbleType.Bomb)
                previewBubbleImages[imgIdx].color = Color.black;
            else if (type == BubbleType.Wildcard)
                wildcardImages.Add(previewBubbleImages[imgIdx]);
            else
                previewBubbleImages[imgIdx].color = colorMap[(int)upcoming[i]];
        }

        if (wildcardImages.Count > 0)
            StartCoroutine(RainbowLoop(wildcardImages.ToArray()));
    }

    private IEnumerator RainbowLoop(Image[] images)
    {
        float hue = 0f;
        while (true)
        {
            var color = Color.HSVToRGB(hue, 1f, 1f);
            foreach (var img in images) img.color = color;
            hue = (hue + Time.deltaTime * 1.5f) % 1f;
            yield return null;
        }
    }
}
