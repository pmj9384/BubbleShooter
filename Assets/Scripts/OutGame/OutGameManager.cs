using UnityEngine;

public class OutGameManager : MonoBehaviour
{
    [SerializeField] private UINavigator navigator;
    [SerializeField] private PanelManager panelManager;

    public UINavigator Navigator => navigator;
    public PanelManager PanelManager => panelManager;

    private void Start()
    {
        navigator.Initialize(this);
        panelManager.Initialize(this);
        navigator.ShowLobby();
    }
}
