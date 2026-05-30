using UnityEngine;

public class ShooterSkin : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.sprite = GameDataManager.Instance.SkinUserData.GetEquippedSprite();

    }
}
