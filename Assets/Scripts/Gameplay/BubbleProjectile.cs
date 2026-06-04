using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BubbleProjectile : MonoBehaviour
{
    private float leftWall;
    private float rightWall;
    private BubbleGrid grid;
    private bool landed;
    private Rigidbody2D rb;
    private Action onLanded;

    public BubbleColor Color { get; private set; }

    public void Launch(BubbleColor color, Vector2 direction, float speed, float leftWall, float rightWall, BubbleGrid grid, Action onLanded = null)
    {
        Color = color;
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
        if (bubble != null) bubble.SetColor(color);
    }

    private void FixedUpdate()
    {
        if (landed) return;

        float x = rb.position.x;
        if (x <= leftWall)
        {
            rb.position = new Vector2(leftWall, rb.position.y);
            rb.linearVelocity = new Vector2(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
        }
        else if (x >= rightWall)
        {
            rb.position = new Vector2(rightWall, rb.position.y);
            rb.linearVelocity = new Vector2(-Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (landed) return;
        if (other.GetComponent<Bubble>() == null) return;

        Land();
    }

    // 최상단 벽에 닿았을 때 (천장)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (landed) return;
        Land();
    }

    private void Land()
    {
        landed = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var (row, col) = grid.FindNearestEmpty(transform.position);
        var bubble = GetComponent<Bubble>();
        if (bubble != null)
        {
            grid.PlaceBubble(bubble, row, col);

            var matches = BubbleMatchProcessor.FindMatches(grid, row, col);
            foreach (var (r, c) in matches) grid.RemoveBubble(r, c);

            var floating = BubbleMatchProcessor.FindFloating(grid);
            foreach (var (r, c) in floating) grid.RemoveBubble(r, c);

            onLanded?.Invoke();

            // BubbleProjectile 역할 끝 — Bubble로 전환
            Destroy(this);
        }
    }
}
