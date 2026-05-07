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
        homeButton.onClick.AddListener(() => uiManager.OpenPage<LobbyPage>());
        shopButton.onClick.AddListener(() => uiManager.OpenPage<ShopPage>());
        skinButton.onClick.AddListener(() => uiManager.OpenPage<SkinPage>());
    }
}
