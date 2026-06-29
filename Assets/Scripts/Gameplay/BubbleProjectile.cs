using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BubbleProjectile : MonoBehaviour
{
    private float leftWall;
    private float rightWall;
    private BubbleGrid grid;
    private BubbleSkillController skillController;
    private bool landed;
    private Rigidbody2D rb;
    private Action onLanded;
    private Vector2 prevPosition;

    public BubbleColor Color { get; private set; }
    public BubbleType Type { get; private set; }

    public void Launch(BubbleColor color, BubbleType type, Vector2 direction, float speed, float leftWall, float rightWall, BubbleGrid grid, BubbleSkillController skillController, Action onLanded = null)
    {
        Color = color;
        Type = type;
        this.skillController = skillController;
        this.leftWall = leftWall;
        this.rightWall = rightWall;
        this.grid = grid;
        this.onLanded = onLanded;
        landed = false;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        var bubble = GetComponent<Bubble>();
        if (bubble != null) bubble.SetVisual(color, type);
    }

    private void FixedUpdate()
    {
        if (landed) return;

        prevPosition = rb.position;

        float x = rb.position.x;
        if (x <= leftWall)
        {
            if (PortalManager.Current != null && PortalManager.Current.AbsorbIfPortalZone(rb.position.y, true))
            {
                onLanded?.Invoke();
                Destroy(gameObject);
                return;
            }
            rb.position = new Vector2(leftWall, rb.position.y);
            rb.linearVelocity = new Vector2(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
        }
        else if (x >= rightWall)
        {
            if (PortalManager.Current != null && PortalManager.Current.AbsorbIfPortalZone(rb.position.y, false))
            {
                onLanded?.Invoke();
                Destroy(gameObject);
                return;
            }
            rb.position = new Vector2(rightWall, rb.position.y);
            rb.linearVelocity = new Vector2(-Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (landed) return;
        var hitBubble = other.GetComponent<Bubble>();
        if (hitBubble == null) return;

        Land(hitBubble.Row, hitBubble.Col);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (landed) return;
        Land();
    }

    private void Land(int hitRow = -1, int hitCol = -1)
    {
        landed = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var (row, col) = hitRow >= 0
            ? grid.FindNearestEmptyAdjacentTo(hitRow, hitCol, prevPosition)
            : grid.FindNearestEmpty(transform.position);

        var bubble = GetComponent<Bubble>();
        if (bubble != null)
        {
            grid.PlaceBubble(bubble, row, col);
            skillController.OnLand(Type, grid, row, col, () =>
            {
                onLanded?.Invoke();
                Destroy(this);
            });
        }
    }
}
