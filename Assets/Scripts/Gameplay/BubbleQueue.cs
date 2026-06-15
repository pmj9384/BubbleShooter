using System.Collections.Generic;
using UnityEngine;

public class BubbleQueue : MonoBehaviour
{
    private const int UPCOMING_COUNT = 6;

    private BubbleColor currentColor;
    private readonly List<BubbleColor> upcomingColors = new();

    public BubbleColor CurrentColor => currentColor;
    public BubbleColor NextColor => upcomingColors[0];
    public IReadOnlyList<BubbleColor> UpcomingColors => upcomingColors;

    private void Awake()
    {
        for (int i = 0; i < UPCOMING_COUNT; i++)
            upcomingColors.Add(RandomColor());
        currentColor = RandomColor();
    }

    public void Consume()
    {
        currentColor = upcomingColors[0];
        upcomingColors.RemoveAt(0);
        upcomingColors.Add(RandomColor());

    }

    private BubbleColor RandomColor()
    {
        return (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.Count);
    }
}
