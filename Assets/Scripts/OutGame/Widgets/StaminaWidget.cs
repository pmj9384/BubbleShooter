using TMPro;
using UnityEngine;

public class StaminaWidget : UIWidget
{
    [SerializeField] private TMP_Text staminaText;

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
