using UnityEngine;

public class UINavigator : MonoBehaviour
{
    [SerializeField] private BaseScreen[] screens;

    public void Show<T>() where T : BaseScreen
    {
        foreach (var screen in screens)
        {
            if (screen is T)
                screen.OnEnter();
            else
                screen.OnExit();
        }
    }

    public void ShowLobby()
    {
        Show<LobbyScreen>();
    }

    public void Initialize(OutGameManager outGameManager)
    {
        foreach (var screen in screens)
            screen.SetOutGameManager(outGameManager);
    }
}
