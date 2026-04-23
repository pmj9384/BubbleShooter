using UnityEngine;
using UnityEngine.UI;

public class SkinScreen : BaseScreen
{
    [SerializeField] private Button homeButton;

    public override void OnEnter()
    {
        base.OnEnter();
        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(() => outGameManager.Navigator.ShowLobby());
    }
}
