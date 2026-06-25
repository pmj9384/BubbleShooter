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
        int popped = GameManager.Instance.CountManager.PoppedCount;
        scoreText.text = $"버블 {popped}개 제거";
        gameObject.SetActive(true);
    }

    public override void Hide() => gameObject.SetActive(false);
}
