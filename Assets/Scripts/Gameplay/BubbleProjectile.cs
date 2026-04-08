using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BubbleProjectile : MonoBehaviour
{
    private Vector2 velocity;
    private float leftWall;
    private float rightWall;
    private BubbleGrid grid;
    private bool landed;

    public BubbleColor Color { get; private set; }

    public void Launch(BubbleColor color, Vector2 direction, float speed, float leftWall, float rightWall, BubbleGrid grid)
    {
        Color = color;
        velocity = direction.normalized * speed;
        this.leftWall = leftWall;
        this.rightWall = rightWall;
        this.grid = grid;
        landed = false;

        var sr = GetComponent<SpriteRenderer>();
        var bubble = GetComponent<Bubble>();
        if (bubble != null) bubble.SetColor(color);
        else if (sr != null)
        {
            // Bubble 컴포넌트 없이 색만 적용할 경우 대비
        }
    }

    private void Update()
    {
        if (landed) return;

        transform.Translate(velocity * Time.deltaTime, Space.World);

        float x = transform.position.x;
        if (x <= leftWall)
        {
            transform.position = new Vector3(leftWall, transform.position.y, 0);
            velocity.x = Mathf.Abs(velocity.x);
        }
        else if (x >= rightWall)
        {
            transform.position = new Vector3(rightWall, transform.position.y, 0);
            velocity.x = -Mathf.Abs(velocity.x);
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
        velocity = Vector2.zero;

        var (row, col) = grid.FindNearestEmpty(transform.position);
        var bubble = GetComponent<Bubble>();
        if (bubble != null)
        {
            grid.PlaceBubble(bubble, row, col);
            // BubbleProjectile 역할 끝 — Bubble로 전환
            Destroy(this);
        }
    }
}
