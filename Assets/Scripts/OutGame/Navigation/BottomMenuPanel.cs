using UnityEngine;
using UnityEngine.UI;

public class BottomMenuPanel : MonoBehaviour
{
    [SerializeField] private OutGameUIManager uiManager;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;

    private void Awake()
    {
        homeButton.onClick.AddListener(() => uiManager.OpenScreen<LobbyScreen>());
        shopButton.onClick.AddListener(() => uiManager.OpenScreen<ShopScreen>());
        skinButton.onClick.AddListener(() => uiManager.OpenScreen<SkinScreen>());
    }
}
