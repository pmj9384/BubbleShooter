public class InGameCountManager : InGameManager
{
    public int PoppedCount { get; private set; }
    public int Score { get; private set; }

    public event System.Action<int> OnScoreChanged;

    public override void Initialize()
    {
        base.Initialize();
        PoppedCount = 0;
        Score = 0;

        var grid = GameManager.BubbleGrid;
        grid.OnBubblePopped -= HandleBubblePopped;
        grid.OnBubblePopped += HandleBubblePopped;
    }

    private void HandleBubblePopped()
    {
        PoppedCount++;
        Score += 100;
        OnScoreChanged?.Invoke(Score);
    }
}
