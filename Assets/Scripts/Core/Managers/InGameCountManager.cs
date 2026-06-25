public class InGameCountManager : InGameManager
{
    public int PoppedCount { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        PoppedCount = 0;

        var grid = GameManager.BubbleGrid;
        grid.OnBubblePopped -= HandleBubblePopped;
        grid.OnBubblePopped += HandleBubblePopped;
    }

    private void HandleBubblePopped() => PoppedCount++;
}
