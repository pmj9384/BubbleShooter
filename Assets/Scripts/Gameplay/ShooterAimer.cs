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
        if (dir != Vector2.zero) UpdateAimLine(dir);
        else lineRenderer.enabled = false;
        OnAimDirectionChanged?.Invoke(dir);
        lineRenderer.enabled = true;
    }

    private void HandleReleased(Vector2 screenPos)
    {
        // TODO
    }

    private Vector2 GetAimDirection(Vector2 screenPos)
    {
        // TODO
        return Vector2.zero;
    }

    private void UpdateAimLine(Vector2 dir)
    {
        // TODO
    }
}
