using UnityEngine;
using UnityEngine.UI;

public class PausePanel : UIElement
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button homeButton;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        resumeButton.onClick.AddListener(() => gameUIManager.ResumeGame());
        settingsButton.onClick.AddListener(() => gameUIManager.ShowUIElement(UIElementEnums.SettingsPanel));
        homeButton.onClick.AddListener(() => gameUIManager.GoToTitle());
    }

    public override void Show() => gameObject.SetActive(true);
    public override void Hide() => gameObject.SetActive(false);
}
