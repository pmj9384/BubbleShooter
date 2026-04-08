using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bubble : MonoBehaviour
{
    public BubbleColor Color { get; private set; }
    public int Row { get; set; }
    public int Col { get; set; }

    private SpriteRenderer spriteRenderer;

    public static UnityEngine.Color[] GetColorMap() => ColorMap;

    private static readonly UnityEngine.Color[] ColorMap = new UnityEngine.Color[]
    {
        UnityEngine.Color.red,
        UnityEngine.Color.blue,
        UnityEngine.Color.green,
        UnityEngine.Color.yellow,
        new UnityEngine.Color(0.6f, 0.2f, 0.8f), // Purple
    };

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.48f;
    }

    public void SetColor(BubbleColor color)
    {
        Color = color;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = ColorMap[(int)color];
    }
}
