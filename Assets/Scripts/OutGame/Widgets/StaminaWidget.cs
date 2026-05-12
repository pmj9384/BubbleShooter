using TMPro;

public class StaminaWidget : UIWidget
{
    private TMP_Text staminaText;

    private void Awake() => staminaText = GetComponent<TMP_Text>();

    protected override void Subscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnStaminaChanged += UpdateUI;
        Refresh();
    }

    protected override void Unsubscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnStaminaChanged -= UpdateUI;
    }

    public override void Refresh()
        => UpdateUI(GameDataManager.Instance.PlayerAccountData.Stamina);

    private void UpdateUI(int stamina)
        => staminaText.text = $"{stamina}/{GameDataManager.Instance.PlayerAccountData.MaxStamina}";
}
