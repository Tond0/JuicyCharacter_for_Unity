using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    //State Automata pattern.
    //Save 3 states at the time
    static int maxStateQueueSize = 3;
    private Queue<PlayerState> stateQueue = new(maxStateQueueSize);
    //Can be useful to know in which state is the player in
    public PlayerState CurrentState => stateQueue.Last();
    public PlayerState PreviousState
    {
        get
        {
            if (stateQueue.Count > 1)
                return stateQueue.ElementAt(1);
            else
                return null;
        }
    }

    //Whenever we change state this event will scream out loud the state we're transitioning from and the state we're transition to!
    public static event Action<PlayerState, PlayerState> OnStateChange;

    [Header("States")]
    [SerializeField] private Stand state_Stand;
    [SerializeField] private Sprint state_Sprint;
    [SerializeField] private Crouch state_Crouch;
    [SerializeField] private Slide state_Slide;
    [SerializeField] private Falling state_Falling;
    [SerializeField] private Jump state_Jump;
    [SerializeField] private WallRunning state_Wallrunning;

    [Header("Debug")]
    [SerializeField, Tooltip("The TMP_text we'll show the current player state as a debug, leaving it null won't cause any problem")] private TextMeshProUGUI txt_StateDebug;
    [SerializeField] private Rigidbody rb_debug;
    [SerializeField, Tooltip("The TMP_text we'll show the current player state as a debug, leaving it null won't cause any problem")] private TextMeshProUGUI txt_VelocityDebug;
    [SerializeField, Tooltip("Do you want to see the gizmo showing how the ground is being checked? Runtime only.")] private bool debug_ShowGroundCheck;
    [SerializeField, Tooltip("Do you want to see the gizmo showing how the ground is being checked? Runtime only.")] private bool debug_ShowWallRunCheck;

    #region State getter
    public Stand State_Stand { get => state_Stand; }
    public Sprint State_Sprint { get => state_Sprint; }
    public Slide State_Slide { get => state_Slide; }
    public Jump State_Jump { get => state_Jump; }
    public Air State_Falling { get => state_Falling; }
    public Crouch State_Crouch { get => state_Crouch; }
    public WallRunning State_Wallrunning { get => state_Wallrunning; }
    #endregion

    private void Start()
    {
        //We assign the first state
        TransitionState(State_Stand);
    }

    //Let's update the current state, through the private method. 
    private void Update() => RunCurrentState();
    private void FixedUpdate()
    {
        CurrentState.FixedRun();

        //Debug
        txt_VelocityDebug.SetText(rb_debug.velocity.ToString());
    }

    /// <summary>
    /// Update the current state and check for any transition happening
    /// </summary>
    private void RunCurrentState()
    {
        //The state the current state wants to transition to
        PlayerState nextState = CurrentState.Run();

        //If null or if itself, there's no state it wants to be on, other that itself! (what an attention seeker!)
        if (nextState == null) return;
        if (nextState == CurrentState) return;

        //Transition to the new state
        TransitionState(nextState);
    }

    /// <summary>
    /// Handle the trasition to the new state
    /// </summary>
    /// <param name="newState"></param>
    private void TransitionState(PlayerState newState)
    {
        //This is now the current state!
        stateQueue.Enqueue(newState);

        if (stateQueue.Count > maxStateQueueSize)
        {
            stateQueue.Dequeue();
            stateQueue.TrimExcess();
        }

        //Old state exit
        PreviousState?.Exit();

        //New state enter
        CurrentState.Enter();

        //Screaming out loud from and to whitch state we're transitioning!
        OnStateChange?.Invoke(CurrentState, PreviousState);

        //DEBUG
        //If a text is assigned we show the current state
        if (txt_StateDebug != null)
            txt_StateDebug.text = newState.ToSafeString();
    }

    //DEBUG (ofc)
    private void OnDrawGizmosSelected()
    {
        if (debug_ShowGroundCheck) DrawGroundCheck();

        if (debug_ShowWallRunCheck) DrawWallRunCheck();
    }
    private void DrawGroundCheck()
    {
        if(!EditorApplication.isPlaying) return;

        //Is this state controllable? (non controllable state won't have ground detection)
        if (CurrentState is not Controllable) return;

        Controllable controllableState = CurrentState as Controllable;
        //The current groundCheck settings
        Controllable.GroundCheck_Stats groundCheck_Stats = controllableState.Stats_GroundCheck;


        Vector3 origin = transform.position + (groundCheck_Stats.heightOffset * Vector3.up);
        //Direction of the spring
        Vector3 springDir = transform.up;


        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, -springDir * groundCheck_Stats.heightOffset);

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
        Vector3 size = new(groundCheck_Stats.wideCheckBuffer, 0.01f * 2, groundCheck_Stats.wideCheckBuffer);

        Vector3 boxOrigin;

        if (Physics.BoxCast(origin, size / 2, -springDir, out RaycastHit hitInfo, Quaternion.identity, groundCheck_Stats.heightCheckBuffer))
            boxOrigin = origin + (hitInfo.distance * Vector3.down);
        else
            boxOrigin = origin + (groundCheck_Stats.heightCheckBuffer * Vector3.down);

        Gizmos.DrawWireCube(boxOrigin, size);
    }

    private void DrawWallRunCheck()
    {
        Vector3 origin = transform.position + Vector3.up * state_Jump.Stats_GroundCheck.height;
        Vector3 halfExtends = new(0.01f, state_Wallrunning.Wr_detectionSize / 2, state_Wallrunning.Wr_detectionSize / 2);

        Gizmos.color = Color.blue;

        Vector3 boxOrigin;

        Vector3 direction = -transform.right;
        if (Physics.BoxCast(origin, halfExtends, direction, out RaycastHit leftWall, transform.rotation, state_Wallrunning.Wr_detectionDistance))
            boxOrigin = origin + (leftWall.distance * direction);
        else
            boxOrigin = origin + (state_Wallrunning.Wr_detectionDistance * direction);

        Gizmos.DrawLine(origin, boxOrigin);
        Gizmos.DrawWireCube(boxOrigin, halfExtends * 2);

        direction = transform.right;
        if (Physics.BoxCast(origin, halfExtends, direction, out RaycastHit rightWall, transform.rotation, state_Wallrunning.Wr_detectionDistance))
            boxOrigin = origin + (rightWall.distance * direction);
        else
            boxOrigin = origin + (state_Wallrunning.Wr_detectionDistance * direction);

        Gizmos.DrawLine(origin, boxOrigin);
        Gizmos.DrawWireCube(boxOrigin, halfExtends * 2);
    }
}
