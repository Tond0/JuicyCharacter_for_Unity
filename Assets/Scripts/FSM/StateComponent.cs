using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerStats;

public class StateComponent : MonoBehaviour
{
    //The state is currently in use!
    private PlayerState currentState;
    //Can be useful to know in which state is the player in
    public PlayerState CurrentState { get { return currentState; } }

    //Whenever we change state this event will scream out loud the state we're transitioning from and the state we're transition to!
    public static event Action<PlayerState, PlayerState> OnStateChange;

    [Header("States")]
    [SerializeField] private Stand state_Stand;
    [SerializeField] private Sprint state_Sprint;
    [SerializeField] private Crouch state_Crouch;
    [SerializeField] private Slide state_Slide;
    [SerializeField] private Falling state_Falling;
    [SerializeField] private Jump state_Jump;

    [Header("Debug")]
    [SerializeField, Tooltip("The TMP_text we'll show the current player state as a debug, leaving it null won't cause any problem")] private TextMeshProUGUI txt_StateDebug;
    [SerializeField, Tooltip("Do you want to see the gizmo showing how the ground is being checked? Runtime only.")] private bool debug_ShowGroundCheck;

    #region State getter
    public Stand State_Stand { get => state_Stand; }
    public Sprint State_Sprint { get => state_Sprint; }
    public Slide State_Slide { get => state_Slide; }
    public Jump State_Jump { get => state_Jump; }
    public Air State_Falling { get => state_Falling; }
    public Crouch State_Crouch { get => state_Crouch; }
    #endregion

    private void Start()
    {
        //We assign the first state
        TransitionState(State_Stand);
    }

    //Let's update the current state, through the private method. 
    private void Update() => RunCurrentState();
    private void FixedUpdate() => currentState.FixedRun();
    
    /// <summary>
    /// Update the current state and check for any transition happening
    /// </summary>
    private void RunCurrentState()
    {
        //The state the current state wants to transition to
        PlayerState nextState = currentState.Run();
        
        //If null or if itself, there's no state it wants to be on, other that itself! (what an attention seeker!)
        if (nextState == null) return;
        if (nextState == currentState) return;

        //Transition to the new state
        TransitionState(nextState);
    }

    /// <summary>
    /// Handle the trasition to the new state
    /// </summary>
    /// <param name="newState"></param>
    private void TransitionState(PlayerState newState)
    {
        //Old state exit
        currentState?.Exit();

        //New state enter
        newState.Enter();

        //Screaming out loud from and to whitch state we're transitioning!
        OnStateChange?.Invoke(currentState, newState);

        //This is now the current state!
        currentState = newState;
        
        //DEBUG
        //If a text is assigned we show the current state
        if (txt_StateDebug != null)
            txt_StateDebug.text = newState.ToSafeString();
    }

    //DEBUG (ofc)
    private void OnDrawGizmosSelected()
    {
        //Do we actually want to see the debug?
        if (!debug_ShowGroundCheck) return;

        //Is this state controllable? (non controllable state won't have ground detection)
        if (CurrentState is not Controllable) return;

        Controllable controllableState = currentState as Controllable;
        //The current groundCheck settings
        var groundCheck_Stats = controllableState.Stats_GroundCheck;


        Vector3 origin = transform.position + (groundCheck_Stats.HeightOffset * Vector3.up);
        //Direction of the spring
        Vector3 springDir = transform.up;


        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, -springDir * groundCheck_Stats.HeightCheckBuffer);

        /* DEPRECATED 5 raycast method
        Gizmos.DrawRay(origin + Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        */

        /* DEPRECATED Spherecast method
        Vector3 sphereOrigin = origin + (groundCheck_Stats.HeightCheckBuffer * Vector3.down);
        Gizmos.DrawWireSphere(sphereOrigin, groundCheck_Stats.WideCheckBuffer);
        */

        //Boxcast method
        
        //Size of the boxcast
        Vector3 size = new(groundCheck_Stats.WideCheckBuffer, 0.01f * 2, groundCheck_Stats.WideCheckBuffer);

        Vector3 boxOrigin;

        if (Physics.BoxCast(origin, size / 2, -springDir, out RaycastHit hitInfo, Quaternion.identity, groundCheck_Stats.HeightCheckBuffer))
            boxOrigin = origin + (hitInfo.distance * Vector3.down);
        else
            boxOrigin = origin + (groundCheck_Stats.HeightCheckBuffer * Vector3.down);

        Gizmos.DrawWireCube(boxOrigin, size);
    }

}
