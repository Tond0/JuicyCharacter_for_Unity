using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Controls.IGameplayActions
{
    private Controls inputAction;

    public static event Action<Vector2> OnMoveFired;
    public static event Action<Vector2> OnLookFired;

    public static event Action OnSprintFired;
    public static event Action OnSprintReleased;

    public static event Action OnJumpFired;
    public static event Action OnJumpReleased;

    public static event Action OnCrouchFired;
    public static event Action OnCrouchReleased;


    private void OnEnable()
    {
        inputAction = new();
        inputAction.Enable();
        inputAction.Gameplay.SetCallbacks(this);
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    #region Input Handler
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        direction.Normalize();

        OnMoveFired?.Invoke(direction);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnSprintFired, OnSprintReleased);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnJumpFired, OnJumpReleased);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnCrouchFired, OnCrouchReleased);
    }
    #endregion

    private void ChecInputPhase(InputAction.CallbackContext context, Action onPerformed, Action onCanceled)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                onPerformed?.Invoke();
                break;

            case InputActionPhase.Canceled:
                onCanceled?.Invoke();
                break;

            default: return;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        direction.Normalize(); 

        OnLookFired?.Invoke(direction);
    }
}
