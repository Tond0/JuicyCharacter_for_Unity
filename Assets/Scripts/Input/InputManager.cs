using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class InputManager : MonoBehaviour, Controls.IGameplayActions
{
    //The input action asset class we defined.
    private Controls inputAction;

    //Input events
    public static event Action<Vector2> OnMoveFired;
    public static event Action<Vector2> OnLookFired;

    public static event Action OnSprintFired;
    public static event Action OnSprintReleased;

    public static event Action OnJumpFired;
    public static event Action OnJumpReleased;

    public static event Action OnCrouchFired;
    public static event Action OnCrouchReleased;

    public static event Action OnPauseFired;
    public static event Action OnPauseReleased;

    //We save and share the direction (only readable) so that even when we're changing state we know whitch direction we're moving!
    private Vector2 direction;
    public Vector2 Direction { get => direction; }

    [Header("Input Buffer")]
    [SerializeField, Tooltip("How many inputs can be stored in the buffer? Newer input will take the place of the oldest input in the buffer.")] private int inputBuffering_MaxInputs;
    [SerializeField, Tooltip("For how long is the input going to stay in the buffer?")] private float inputBuffering_MaxInputTime;

    //The buffer queue!
    private readonly List<InputEvent> inputBufferInvoker = new();

    [Header("Input interactions")]
    [SerializeField] private InputTriggerType itt_Jump;
    [SerializeField] private InputTriggerType itt_Sprint;
    [SerializeField] private InputTriggerType itt_CrouchSlide;
    [SerializeField] private InputTriggerType itt_Pause;

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
        //Input setup
        inputAction = new();
        inputAction.Enable();
        inputAction.Gameplay.SetCallbacks(this);

        //Cursor settings
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Each time a state is changed we check if we have any input that the new state can use!
        StateComponent.OnStateChange += CallBuffer;
    }

    private void OnDisable()
    {
        //Input won't be processed anymore!
        inputAction.Disable();

        StateComponent.OnStateChange -= CallBuffer;
    }

    #region Input Handler Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
        
        //Not good for gamepad! 
        //direction.Normalize();

        OnMoveFired?.Invoke(direction);
    }

    //Cinemachine will automatically handle the camera movement, but whit this event call we can adjust the player facing direction
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();

        //FIXME: Check if this cause problems with gamepad
        direction.Normalize();

        OnLookFired?.Invoke(direction);
    }

    //Input that CAN and MAY be buffered
    public void OnJump(InputAction.CallbackContext context) => Handle_GenericInput(context, OnJumpFired, OnJumpReleased, itt_Jump, InputType.Jump);
    public void OnSprint(InputAction.CallbackContext context) => Handle_GenericInput(context, OnSprintFired, OnSprintReleased, itt_Sprint);
    public void OnCrouch(InputAction.CallbackContext context) => Handle_GenericInput(context, OnCrouchFired, OnCrouchReleased, itt_CrouchSlide, InputType.Crouch);
    public void OnPause(InputAction.CallbackContext context) => Handle_GenericInput(context, OnPauseFired, OnPauseReleased, itt_Pause);

    #region Handle_GenericInput method variants
    private void Handle_GenericInput(InputAction.CallbackContext context, Action OnInputFired, Action OnInputReleased, InputTriggerType performedMethod, InputType inputType)
    {
        switch (performedMethod)
        {
            case InputTriggerType.Toggle:
                HandleInput_Toggle(context, OnInputFired, inputType);
                break;
            case InputTriggerType.HoldNdRelease:
                HandleInput_HoldRelease(context, OnInputFired, OnInputReleased, inputType);
                break;
        }
    }

    private void Handle_GenericInput(InputAction.CallbackContext context, Action OnInputFired, Action OnInputReleased, InputTriggerType performedMethod)
    {
        switch (performedMethod)
        {
            case InputTriggerType.Toggle:
                HandleInput_Toggle(context, OnInputFired);
                break;
            case InputTriggerType.HoldNdRelease:
                HandleInput_HoldRelease(context, OnInputFired, OnInputReleased);
                break;
        }
    }
    #endregion
    
    #endregion

    #region Buffer Methods
    /// <summary>
    /// Function that call each action in the buffer until we get a reaction
    /// </summary>
    private void CallBuffer(PlayerState oldState, PlayerState newState)
    {
        for (int i = 0; i < inputBufferInvoker.Count; i++)
        {
            //The current InputEvent we're checking
            InputEvent bufferedEvent = inputBufferInvoker[i];

            //Check if the inputEvent is still on time to be executed or it has been expired.
            if (bufferedEvent.Validate())
            {
                //If its in time but there's no binding to the action (the state doesn't listen to those inputs) therefore we loop hoping to find another usable input
                if (!bufferedEvent.TryExecute()) continue;

                //If it's been execute we remove it from the queue
                inputBufferInvoker.Remove(bufferedEvent);

                //Let's save space!
                inputBufferInvoker.TrimExcess();

                //No more action will be called until the next OnStateChange
                return;
            }

            //Out of time!
            inputBufferInvoker.Remove(bufferedEvent);
            i--;
            continue;
        }
    }
    /// <summary>
    /// Crete an inputEvent with the inputType and the inputPhase given, and then it adds the event to the buffer queue.
    /// </summary>
    /// <param name="inputType"></param>
    /// <param name="inputPhase"></param>
    private void AddToBuffer(InputType inputType, InputActionPhase inputPhase)
    {
        //Max input buffer check
        if (inputBufferInvoker.Count > inputBuffering_MaxInputs)
            //Remove the oldest inputEvent in queue
            inputBufferInvoker.RemoveAt(0);

        //Create inputEvent
        InputEvent currentInputEvent = new(inputType, inputPhase, Time.time, inputBuffering_MaxInputTime);
        //Add inputEvent
        inputBufferInvoker.Add(currentInputEvent);
    }
    #endregion

    #region Input Handler Method
    /// <summary>
    /// Handle the input cancelling the action if phase is canceled, if added the inputType this input will be saved in the buffer
    /// </summary>
    /// <param name="context"></param>
    /// <param name="onPerformed"></param>
    /// <param name="onCanceled"></param>
    /// <param name="boolToChange"></param>
    private void HandleInput_HoldRelease(InputAction.CallbackContext context, Action onPerformed, Action onCanceled)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                onPerformed?.Invoke();
                break;

            case InputActionPhase.Canceled:
                onCanceled?.Invoke();
                break;

            default:
                return;
        }
    }

    /// <summary>
    /// Handle the input cancelling the action only if phase is canceled, if added the inputType this input will be saved in the buffer if it failed
    /// </summary>
    /// <param name="context"></param>
    /// <param name="onPerformed"></param>
    /// <param name="onCanceled"></param>
    /// <param name="boolToChange"></param>
    private void HandleInput_HoldRelease(InputAction.CallbackContext context, Action onPerformed, Action onCanceled, InputType inputType)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:

                if (onPerformed != null)
                {
                    onPerformed.Invoke();
                    return;
                }

                break;

            case InputActionPhase.Canceled:

                if (onCanceled != null)
                {
                    onCanceled?.Invoke();
                    return;
                }

                break;

            default: return;
        }

        AddToBuffer(inputType, context.phase);
    }

    /// <summary>
    /// Handle the input cancelling the action only if phase is performed again, if added the inputType this input will be saved in the buffer if it failed
    /// </summary>
    /// <param name="context"></param>
    /// <param name="onPerformed"></param>
    /// <param name="onCanceled"></param>
    /// <param name="boolToChange"></param>
    private void HandleInput_Toggle(InputAction.CallbackContext context, Action onPerformed)
    {
        if (context.phase != InputActionPhase.Performed) return;

        onPerformed?.Invoke();
    }

    /// <summary>
    /// Handle the input cancelling the action only if phase is performed again, if added the inputType this input will be saved in the buffer
    /// </summary>
    /// <param name="context"></param>
    /// <param name="onPerformed"></param>
    /// <param name="onCanceled"></param>
    /// <param name="boolToChange"></param>
    private void HandleInput_Toggle(InputAction.CallbackContext context, Action onPerformed, InputType inputType)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (onPerformed != null)
        {
            onPerformed.Invoke();
            return;
        }

        AddToBuffer(inputType, InputActionPhase.Performed);
    }

    #endregion

    #region Class Struct Enum
    
    #region Enums
    //Type of interaction
    private enum InputTriggerType { Toggle, HoldNdRelease }

    //Type of input that can be buffered
    //Each time we want a new input to be buffer we need to assign it a type! (not that flexible but, this way, we can still manage to use an input system based on events! :) )
    private enum InputType { Jump, Crouch }
    #endregion

    private readonly struct InputEvent
    {
        private readonly InputType type;
        private readonly InputActionPhase interaction;

        //FIXME: Only for debug, remove this two!
        public InputActionPhase Interaction => interaction;
        public InputType Type => type;


        private readonly float timeStamp;
        private readonly float maxInputLife;


        public InputEvent(InputType type, InputActionPhase interaction, float timeStamp, float maxInputLife)
        {
            this.type = type;
            this.interaction = interaction;
            this.timeStamp = timeStamp;
            this.maxInputLife = maxInputLife;
        }


        //FIXME: Compare time inside this and dont use timer for buffering.
        public bool Validate()
        {
            if (timeStamp + maxInputLife < Time.time) return false;

            return true;
        }

        public bool TryExecute()
        {
            return Type switch
            {
                InputType.Jump => InvokeInputEvent(OnJumpFired, OnJumpReleased),
                InputType.Crouch => InvokeInputEvent(OnCrouchFired, OnCrouchReleased),
                _ => false,
            };
        }

        //Basically just a macro for the method on top of this 
        private readonly bool InvokeInputEvent(Action performed, Action canceled)
        {
            switch (Interaction)
            {
                case InputActionPhase.Performed:
                    if (performed == null) return false;
                    performed.Invoke();
                    return true;

                case InputActionPhase.Canceled:
                    if (canceled == null) return false;
                    canceled.Invoke();
                    return true;

                default:
                    return false;
            }
        }
    }

    #endregion
}
