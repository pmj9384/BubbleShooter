using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyScreen : BaseScreen
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;

    public override void OnEnter()
    {
        base.OnEnter();
        playButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        skinButton.onClick.RemoveAllListeners();

        playButton.onClick.AddListener(() => SceneManager.LoadScene("SampleScene"));
        optionsButton.onClick.AddListener(() => outGameManager.PanelManager.Show<OutGameSettingsPanel>());
        shopButton.interactable = false;
        skinButton.interactable = false;
    }
}
