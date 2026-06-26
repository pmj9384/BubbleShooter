using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : UIElement
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private TMP_Text scoreText;

    public override void Initialize()
    {
        gameObject.SetActive(false);
        restartButton.onClick.AddListener(() => gameUIManager.RestartGame());
        homeButton.onClick.AddListener(() => gameUIManager.GoToTitle());
    }

    public override void Show()
    {
        int score = GameManager.Instance.CountManager.Score;
        int earned = score / 10;
        scoreText.text = $"{score:N0}점\n+{earned} 코인 획득";
        gameObject.SetActive(true);
    }

    public override void Hide() => gameObject.SetActive(false);
}
