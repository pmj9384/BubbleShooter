using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : UIElement
{
    [SerializeField] private Button restartButton;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        restartButton.onClick.AddListener(() => gameUIManager.RestartGame());
    }

    public override void Show() => gameObject.SetActive(true);

    public override void Hide() => gameObject.SetActive(false);
}
