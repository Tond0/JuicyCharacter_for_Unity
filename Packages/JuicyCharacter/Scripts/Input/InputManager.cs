using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Wrapper reference-type for the Action delegate.
/// Forced to make this because C# doen't allow to have ref fields.
/// </summary>
public class ActionRef
{
    public Action Delegate;
}

public class InputManager : MonoBehaviour
{   
    // Input events wrapper
    public static event Action<Vector2> OnMoveFired;

    // Shared movement direction
    private Vector2 movingDirection;
    public Vector2 MovingDirection => movingDirection;

    public static event Action<Vector2> OnLookFired;

    public static readonly ActionRef OnSprintFiredRef = new ActionRef();
    public static readonly ActionRef OnSprintReleasedRef = new ActionRef();
    [SerializeField] private InputInteractionType sprintInteractionType;
    private bool wantsToSprint = false;
    public bool WantsToSprint => wantsToSprint;

    public static readonly ActionRef OnJumpFiredRef = new ActionRef();
    public static readonly ActionRef OnJumpReleasedRef = new ActionRef();

    public static readonly ActionRef OnCrouchFiredRef = new ActionRef();
    public static readonly ActionRef OnCrouchReleasedRef = new ActionRef();
    [SerializeField] private InputInteractionType crouchInteractionType;

    public static readonly ActionRef OnWallRunFiredRef = new ActionRef();
    public static readonly ActionRef OnWallRunReleasedRef = new ActionRef();

    public static readonly ActionRef OnPauseFiredRef = new ActionRef();
    public static readonly ActionRef OnPauseReleasedRef = new ActionRef();

    [Header("Input Buffer")]
    [SerializeField, Tooltip("How many inputs can be stored in the buffer? Newer input will take the place of the oldest input in the buffer.")]
    private int inputBuffering_MaxInputs;
    [SerializeField, Tooltip("For how long is the input going to stay in the buffer?")]
    private float inputBuffering_MaxInputTime;

    // The buffer list
    private readonly List<InputEvent> inputBufferInvoker = new();

    #region Singleton pattern
    public static InputManager current;
    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Destroy(this);
    }
    #endregion

    private void OnEnable()
    {
        // Cursor settings
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Listen for state changes to flush the buffer
        StateMachine.OnStateChange += CallBuffer;
    }

    private void OnDisable()
    {
        StateMachine.OnStateChange -= CallBuffer;
    }

    #region Input Handler Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        movingDirection = context.ReadValue<Vector2>();
        OnMoveFired?.Invoke(movingDirection);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        dir.Normalize();
        OnLookFired?.Invoke(dir);
    }

    public void OnJump(InputAction.CallbackContext context)
        => HandleInput(context, OnJumpFiredRef, OnJumpReleasedRef, true);
    public void OnSprint(InputAction.CallbackContext context) 
        => HandleInput(context, OnSprintFiredRef, OnSprintReleasedRef, false, sprintInteractionType, ref wantsToSprint);
    public void OnCrouch(InputAction.CallbackContext context)
        => HandleInput(context, OnCrouchFiredRef, OnCrouchReleasedRef, true, crouchInteractionType);
    public void OnWallRun(InputAction.CallbackContext context)
        => HandleInput(context, OnWallRunFiredRef, OnWallRunReleasedRef, false);
    public void OnPause(InputAction.CallbackContext context)
        => HandleInput(context, OnPauseFiredRef, OnPauseReleasedRef, false);
    #endregion

    #region Buffer Methods
    private void CallBuffer(PlayerState oldState, PlayerState newState)
    {
        for (int i = 0; i < inputBufferInvoker.Count; i++)
        {
            InputEvent evt = inputBufferInvoker[i];
            if (!evt.Validate())
            {
                inputBufferInvoker.RemoveAt(i);
                i--;
                continue;
            }

            if (!evt.TryExecute())
                continue;

            inputBufferInvoker.RemoveAt(i);
            inputBufferInvoker.TrimExcess();
            return;
        }
    }

    private void AddToBuffer(ActionRef actionRef)
    {
        if (inputBufferInvoker.Count >= inputBuffering_MaxInputs)
            inputBufferInvoker.RemoveAt(0);

        var evt = new InputEvent(actionRef, Time.time, inputBuffering_MaxInputTime);
        inputBufferInvoker.Add(evt);
    }
    #endregion

    #region Core Input Handling
    private void HandleInput(InputAction.CallbackContext context, ActionRef onPerformedRef, ActionRef onCanceledRef, bool buffer)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (onPerformedRef.Delegate == null)
                {
                    if (buffer) AddToBuffer(onPerformedRef);
                }
                else
                {
                    onPerformedRef.Delegate.Invoke();
                }
                break;
            case InputActionPhase.Canceled:

                if (onCanceledRef.Delegate == null)
                {
                    if (buffer) AddToBuffer(onCanceledRef);
                }
                else
                {
                    onCanceledRef.Delegate.Invoke();
                }
                break;
            default: return;
        }
    }

    private void HandleInput(InputAction.CallbackContext context, ActionRef onPerformedRef, ActionRef onCanceledRef, bool buffer, InputInteractionType interactionType)
    {
        if (context.canceled && interactionType == InputInteractionType.Toggle) return;
        HandleInput(context, onPerformedRef, onCanceledRef, buffer);
    }

    private void HandleInput(InputAction.CallbackContext context, ActionRef onPerformedRef, ActionRef onCanceledRef, bool buffer, InputInteractionType interactionType, ref bool wantsToBool)
    {
        if(context.started) return;
        if (context.canceled && interactionType == InputInteractionType.Toggle) return;
        wantsToBool = !wantsToBool;
        HandleInput(context, onPerformedRef, onCanceledRef, buffer);
    }
    #endregion

    private readonly struct InputEvent
    {
        private readonly ActionRef _actionRef;
        private readonly float _timeStamp;
        private readonly float _maxInputLife;

        public InputEvent(ActionRef actionRef, float timeStamp, float maxInputLife)
        {
            _actionRef = actionRef;
            _timeStamp = timeStamp;
            _maxInputLife = maxInputLife;
        }

        public bool Validate()
            => (_timeStamp + _maxInputLife) >= Time.time;

        public bool TryExecute()
        {
            var del = _actionRef.Delegate;
            if (del == null) return false;
            del.Invoke();
            return true;
        }
    }

    private enum InputInteractionType { Toggle, Hold }
}
