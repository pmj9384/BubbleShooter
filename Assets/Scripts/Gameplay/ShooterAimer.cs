using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShooterAimer : MonoBehaviour
{
    private const float LEFT_WALL = -4.0f;
    private const float RIGHT_WALL = 4.0f;
    private const float MIN_ANGLE_FROM_HORIZONTAL = 10f;
    private const float REFLECT_LINE_LENGTH = 20f;

    public event Action<Vector2> OnAimDirectionChanged;

    private LineRenderer lineRenderer;
    private ShooterInputHandler inputHandler;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        inputHandler = GetComponent<ShooterInputHandler>();
    }

    private void OnEnable()
    {
        inputHandler.OnDragging += HandleDragging;
        inputHandler.OnReleased += HandleReleased;
    }

    private void OnDisable()
    {
        inputHandler.OnDragging -= HandleDragging;
        inputHandler.OnReleased -= HandleReleased;
    }

    private void HandleDragging(Vector2 screenPos)
    {
        var dir = GetAimDirection(screenPos);
        if (dir != Vector2.zero)
            UpdateAimLine(dir);
        else
            lineRenderer.enabled = false;
        OnAimDirectionChanged?.Invoke(dir);
    }

    private void HandleReleased(Vector2 screenPos)
    {
        lineRenderer.enabled = false;
    }

    private Vector2 GetAimDirection(Vector2 screenPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 dir = ((Vector2)transform.position - (Vector2)worldPos).normalized;

        if (dir.y <= 0) return Vector2.zero;

        float minY = Mathf.Sin(MIN_ANGLE_FROM_HORIZONTAL * Mathf.Deg2Rad);
        if (dir.y < minY)
        {
            dir.y = minY;
            dir = dir.normalized;
        }
        return dir;
    }

    private void UpdateAimLine(Vector2 dir)
    {
        lineRenderer.enabled = true;
        Vector2 p0 = transform.position;

        Vector2 p1, p2;
        if (Mathf.Abs(dir.x) < 0.001f)
        {
            p1 = p0 + dir * (REFLECT_LINE_LENGTH * 0.5f);
            p2 = p0 + dir * REFLECT_LINE_LENGTH;
        }
        else
        {
            float wallX = dir.x > 0 ? RIGHT_WALL : LEFT_WALL;
            float t = (wallX - p0.x) / dir.x;
            p1 = p0 + dir * t;
            Vector2 reflectedDir = new Vector2(-dir.x, dir.y).normalized;
            p2 = p1 + reflectedDir * REFLECT_LINE_LENGTH;
        }

        lineRenderer.SetPosition(0, p0);
        lineRenderer.SetPosition(1, p1);
        lineRenderer.SetPosition(2, p2);
    }
}
