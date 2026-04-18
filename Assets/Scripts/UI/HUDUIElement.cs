using TMPro;
using UnityEngine;

public class HUDUIElement : UIElement
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private BubbleShooterController shooter;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        shooter = FindFirstObjectByType<BubbleShooterController>();
        if (shooter != null)
            shooter.OnFired += UpdateDisplay;
    }

    public override void Show()
    {
        gameObject.SetActive(true);
        UpdateDisplay();
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateDisplay()
    {
        if (shooter == null || countdownText == null) return;
        countdownText.text = $"\ub2e4\uc74c \uc904\uae4c\uc9c0: {shooter.ShotsUntilNextRow}\ubc1c";
    }
}
