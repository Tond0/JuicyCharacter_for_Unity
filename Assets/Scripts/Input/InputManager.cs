using System;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Controls.IGameplayActions
{
    public static InputManager current;

    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Destroy(this);
    }

    private Controls inputAction;

    public static event Action<Vector2> OnMoveFired;
    public static event Action<Vector2> OnLookFired;

    public static event Action OnSprintFired;
    //private InputEvent ie_SprintFired;
    public static event Action OnSprintReleased;
    //private InputEvent ie_SprintReleased;

    public static event Action OnJumpFired;
    //private InputEvent ie_JumpFired;
    public static event Action OnJumpReleased;
    //private InputEvent ie_JumpReleased;

    public static event Action OnCrouchFired;
    //private InputEvent ie_CrouchFired;
    public static event Action OnCrouchReleased;
    //private InputEvent ie_CrouchReleased;

    private Vector2 direction;
    public Vector2 Direction { get => direction; }

    //private bool wantToSprint;
    //private bool wantToJump;
    //private bool wantToCrouch;

    //public bool WantToSprint { get => wantToSprint; }
    //public bool WantToJump { get => wantToJump; }
    //public bool WantToCrouch { get => wantToCrouch; }

    private enum InputPerformedMethod { Toggle, HoldNdRelease }

    //Buffer
    private enum InputType { Jump, Crouch }
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
                    //Debug.LogError(interaction.ToString() + "is not supported as an interaction!");
                    return false;
            }
        }
    }

    [Header("Input interactions")]
    [SerializeField] private InputPerformedMethod inputInteraction_CrouchSlide;
    [SerializeField] private InputPerformedMethod inputInteraction_Sprint;

    private void OnEnable()
    {
        inputAction = new();
        inputAction.Enable();
        inputAction.Gameplay.SetCallbacks(this);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StateComponent.OnStateChange += CallBuffer;
    }

    private void OnDisable()
    {
        inputAction.Disable();

        StateComponent.OnStateChange -= CallBuffer;
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
        switch (inputInteraction_Sprint)
        {
            case InputPerformedMethod.Toggle:
                HandleInput_Toggle(context, OnSprintFired);
                break;
            case InputPerformedMethod.HoldNdRelease:
                HandleInput_HoldRelease(context, OnSprintFired, OnSprintReleased);
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        HandleInput_HoldRelease(context, OnJumpFired, OnJumpReleased, InputType.Jump);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (inputInteraction_CrouchSlide)
        {
            case InputPerformedMethod.Toggle:
                HandleInput_Toggle(context, OnCrouchFired, InputType.Crouch);
                break;
            case InputPerformedMethod.HoldNdRelease:
                HandleInput_HoldRelease(context, OnCrouchFired, OnCrouchReleased, InputType.Crouch);
                break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        direction.Normalize();

        OnLookFired?.Invoke(direction);
    }
    #endregion


    //FIXME: Da spostare sopra tra le variabili!
    //Buffer
    [SerializeField] private int inputBuffering_MaxInputs;
    [SerializeField] private float inputBuffering_MaxInputTime;

    private readonly List<InputEvent> inputBufferInvoker = new();


    /// <summary>
    /// Function that call each action in the buffer until we get a reaction
    /// </summary>
    private void CallBuffer(PlayerState oldState, PlayerState newState)
    {
        for (int i = 0; i < inputBufferInvoker.Count; i++)
        {
            InputEvent bufferedEvent = inputBufferInvoker[i];

            //Then we call it
            if (bufferedEvent.Validate())
            {
                //If its in time but its not the action we are lookin for we keep checking the list.
                if (!bufferedEvent.TryExecute()) continue;

                //If it's been execute we remove it
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

    private void AddToBuffer(InputType inputType, InputActionPhase inputPhase)
    {
        if (inputBufferInvoker.Count > inputBuffering_MaxInputs)
            inputBufferInvoker.RemoveAt(0);

        InputEvent currentInputEvent = new(inputType, inputPhase, Time.time, inputBuffering_MaxInputTime);
        inputBufferInvoker.Add(currentInputEvent);

        /* Old timer method, doesn't work cause is async and could remove an InputEvent from the list while we are looping it!
        Timer timer_Life = new (inputBuffering_MaxInputTime * 1000);
        timer_Life.Elapsed += Handle_LifeTimerEnd;
        timer_Life.Start ();
        */

        //FIXME: Remember remove this
        Debug_PrintList(inputBufferInvoker);
    }

    /* Deprecated function used with the timer method in the method on top of this one
    private void Handle_LifeTimerEnd(object sender, ElapsedEventArgs e)
    {
        inputBufferInvoker.RemoveAt(0);
    }
    */

    //FIXME: Da eliminare o da spostare in una classe Utility statica
    private void Debug_PrintList(List<InputEvent> list)
    {
        foreach (var item in list)
        {
            Debug.Log(item.Type + " " + item.Interaction);
        }
        Debug.Log(" ");
    }


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

}
