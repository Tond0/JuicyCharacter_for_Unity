using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Controls.IGameplayActions
{
    public static InputManager current;

    private void Awake()
    {
        if(current == null)
            current = this;
        else
            Destroy(this);
    }

    private Controls inputAction;

    public static event Action<Vector2> OnMoveFired;
    public static event Action<Vector2> OnLookFired;

    public static event Action OnSprintFired;
    public static event Action OnSprintReleased;

    public static event Action OnJumpFired;
    public static event Action OnJumpReleased;

    public static event Action OnCrouchFired;
    public static event Action OnCrouchReleased;

    private Vector2 direction;
    private bool wantToJump;
    private bool wantToSprint;
    private bool wantToCrouch;

    public Vector2 Direction { get => direction; }
    public bool WantToJump { get => wantToJump; }
    public bool WantToSprint { get => wantToSprint; }
    public bool WantToCrouch { get => wantToCrouch; }

    private void OnEnable()
    {
        inputAction = new();
        inputAction.Enable();
        inputAction.Gameplay.SetCallbacks(this);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    #region Input Handler
    public void OnMove(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
        //direction.Normalize();
        OnMoveFired?.Invoke(direction);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnSprintFired, OnSprintReleased, out wantToSprint);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnJumpFired, OnJumpReleased, out wantToJump);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        ChecInputPhase(context, OnCrouchFired, OnCrouchReleased, out wantToCrouch);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        direction.Normalize(); 

        OnLookFired?.Invoke(direction);
    }
    #endregion

    private void ChecInputPhase(InputAction.CallbackContext context, Action onPerformed, Action onCanceled, out bool boolToChange)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                boolToChange = true;
                onPerformed?.Invoke();
                break;

            case InputActionPhase.Canceled:
                boolToChange = false;
                onCanceled?.Invoke();
                break;

            default:
                boolToChange = false;
                break;
        }
    }

}
