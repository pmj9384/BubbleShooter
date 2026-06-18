public interface IBubbleEffect
{
    BubbleType TargetType { get; }
    void Apply(BubbleGrid grid, int row, int col);
}
