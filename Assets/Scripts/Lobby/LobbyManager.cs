using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button skinButton;
    [SerializeField] private LobbySettingsPanel settingsPanel;

    private void Start()
    {
        playButton.onClick.AddListener(() => SceneManager.LoadScene("SampleScene"));
        optionsButton.onClick.AddListener(() => settingsPanel.Show());
        shopButton.interactable = false;
        skinButton.interactable = false;
    }
}
