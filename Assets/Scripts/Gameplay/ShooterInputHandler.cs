using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnDragging;
    public event Action<Vector2> OnReleased;
    public event Action OnEscapePressed;

    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction escapeAction;

    private bool isDragging;

    private void Awake()
    {
        fireAction = new InputAction("Fire", InputActionType.Button, binding: "<Pointer>/press");
        aimAction = new InputAction("Aim", InputActionType.Value, binding: "<Pointer>/position");
        escapeAction = new InputAction("Escape", InputActionType.Button, binding: "<Keyboard>/escape");

    }

    private void OnEnable()
    {
        fireAction.performed += OnFirePerformed;
        fireAction.canceled += OnFireCanceled;
        aimAction.performed += OnAimPerformed;
        escapeAction.performed += OnEscapePerformed;
        fireAction.Enable();
        aimAction.Enable();
        escapeAction.Enable();
    }

    private void OnDisable()
    {
        fireAction.performed -= OnFirePerformed;
        fireAction.canceled -= OnFireCanceled;
        aimAction.performed -= OnAimPerformed;
        escapeAction.performed -= OnEscapePerformed;
        fireAction.Disable();
        aimAction.Disable();
        escapeAction.Disable();
    }

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        isDragging = true;
    }
    private void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        var pos = aimAction.ReadValue<Vector2>();
        OnReleased?.Invoke(pos);
        isDragging = false;

    }
    private void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        var ctxpos = ctx.ReadValue<Vector2>();
        if (isDragging == true)
        {
            OnDragging?.Invoke(ctxpos);
        }
    }
    private void OnEscapePerformed(InputAction.CallbackContext ctx)
    {
        OnEscapePressed?.Invoke();
    }
}
