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
