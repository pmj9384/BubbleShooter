using System.Collections.Generic;
using UnityEngine;

public class BubbleQueue : MonoBehaviour
{
    private const int UPCOMING_COUNT = 6;

    private BubbleColor currentColor;
    private BubbleType currentType;
    private readonly List<BubbleColor> upcomingColors = new();
    private readonly List<BubbleType> upcomingTypes = new();

    public BubbleColor CurrentColor => currentColor;
    public BubbleType CurrentType => currentType;
    public BubbleColor NextColor => upcomingColors[0];
    public BubbleType NextType => upcomingTypes[0];
    public IReadOnlyList<BubbleColor> UpcomingColors => upcomingColors;
    public IReadOnlyList<BubbleType> UpcomingTypes => upcomingTypes;

    private void Awake()
    {
        for (int i = 0; i < UPCOMING_COUNT; i++)
        {
            upcomingColors.Add(RandomColor());
            upcomingTypes.Add(RandomType());
        }
        currentColor = RandomColor();
        currentType = RandomType();
    }

    public void Consume()
    {
        currentColor = upcomingColors[0];
        currentType = upcomingTypes[0];
        upcomingColors.RemoveAt(0);
        upcomingTypes.RemoveAt(0);
        upcomingColors.Add(RandomColor());
        upcomingTypes.Add(RandomType());
    }

    private BubbleColor RandomColor()
    {
        return (BubbleColor)UnityEngine.Random.Range(0, (int)BubbleColor.Count);
    }

    private BubbleType RandomType()
    {
        float r = Random.value;
        if (r < 0.05f) return BubbleType.Bomb;
        if (r < 0.10f) return BubbleType.Wildcard;
        return BubbleType.Normal;
    }
}
