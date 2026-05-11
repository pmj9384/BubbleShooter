using UnityEngine;
using UnityEngine.UI;

public class BottomMenuPanel : UIPanel
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;

    public new void SetUIManager(IUIManager uiManager)
    {
        base.SetUIManager(uiManager);
        homeButton.onClick.AddListener(() => uiManager.OpenScreen<LobbyScreen>());
        shopButton.onClick.AddListener(() => uiManager.OpenScreen<ShopScreen>());
        skinButton.onClick.AddListener(() => uiManager.OpenScreen<SkinScreen>());
    }
}
