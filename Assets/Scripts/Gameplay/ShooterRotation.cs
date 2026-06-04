using UnityEngine;

public class ShooterRotation : MonoBehaviour
{
    [SerializeField] private BubbleShooterController controller;
    [SerializeField] private Transform shooterTransform;

    private void OnEnable()
    {
        controller.OnAimDirectionChanged += Rotate;
    }

    private void OnDisable()
    {
        controller.OnAimDirectionChanged -= Rotate;
    }

    private void Rotate(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        shooterTransform.rotation = Quaternion.Euler(0, 0, -angle);
    }
}
