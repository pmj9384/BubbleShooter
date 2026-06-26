using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bubble : MonoBehaviour
{
    public BubbleColor Color { get; private set; }
    public int Row { get; set; }
    public int Col { get; set; }

    private SpriteRenderer spriteRenderer;

    private static Sprite[] normalSprites;
    private static Sprite blackholeSprite;
    private static Sprite wildcardSprite;

    private static void LoadSprites()
    {
        if (normalSprites != null) return;
        normalSprites = new Sprite[(int)BubbleColor.Count];
        normalSprites[(int)BubbleColor.Red]    = Resources.Load<Sprite>("Sprites/Bubbles/bubble_red");
        normalSprites[(int)BubbleColor.Blue]   = Resources.Load<Sprite>("Sprites/Bubbles/bubble_blue");
        normalSprites[(int)BubbleColor.Green]  = Resources.Load<Sprite>("Sprites/Bubbles/bubble_green");
        normalSprites[(int)BubbleColor.Yellow] = Resources.Load<Sprite>("Sprites/Bubbles/bubble_yellow");
        normalSprites[(int)BubbleColor.Purple] = Resources.Load<Sprite>("Sprites/Bubbles/bubble_purple");
        blackholeSprite     = Resources.Load<Sprite>("Sprites/Bubbles/bubble_blackhole");
        wildcardSprite = Resources.Load<Sprite>("Sprites/Bubbles/bubble_wildcard");
    }

    public static Sprite GetNormalSprite(BubbleColor color) { LoadSprites(); return normalSprites[(int)color]; }
    public static Sprite GetBlackholeSprite()     { LoadSprites(); return blackholeSprite; }
    public static Sprite GetWildcardSprite() { LoadSprites(); return wildcardSprite; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.48f;
    }

    public void SetColor(BubbleColor color)
    {
        SetVisual(color, BubbleType.Normal);
    }

    public void SetVisual(BubbleColor color, BubbleType type)
    {
        Color = color;
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        LoadSprites();
        spriteRenderer.color = UnityEngine.Color.white;
        spriteRenderer.sprite = type switch
        {
            BubbleType.Blackhole     => blackholeSprite,
            BubbleType.Wildcard => wildcardSprite,
            _                   => normalSprites[(int)color],
        };
    }
}
