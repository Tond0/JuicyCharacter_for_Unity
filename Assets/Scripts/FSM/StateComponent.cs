using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerStats;

public class StateComponent : MonoBehaviour
{
    private PlayerState currentState;
    public PlayerState CurrentState { get { return currentState; } }

    public static event Action OnStateChange;

    [Header("States")]
    [SerializeField] private Stand state_Stand;
    [SerializeField] private Sprint state_Sprint;
    [SerializeField] private Crouch state_Crouch;
    [SerializeField] private Slide state_Slide;
    [SerializeField] private Falling state_Falling;
    [SerializeField] private Jump state_Jump;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI txt_StateDebug;
    [SerializeField] private bool debug_ShowGroundCheck;
    //Getter
    public Stand State_Stand { get => state_Stand; }
    public Sprint State_Sprint { get => state_Sprint; }
    public Slide State_Slide { get => state_Slide; }
    public Jump State_Jump { get => state_Jump; }
    public Air State_Falling { get => state_Falling; }
    public Crouch State_Crouch { get => state_Crouch; }

    private void Start()
    {
        currentState = State_Stand;
        currentState.Enter();
    }

    private void Update()
    {
        RunCurrentState();
        txt_StateDebug.text = currentState.ToSafeString();
    }

    private void RunCurrentState()
    {
        PlayerState nextState = currentState.Run();
        
        if (nextState == null) return;
        if (nextState == currentState) return;

        TransitionState(nextState);
    }

    private void FixedUpdate()
    {
        currentState.FixedRun();
    }

    private void TransitionState(PlayerState newState)
    {
        currentState.Exit();

        currentState = newState;

        currentState.Enter();

        OnStateChange?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;


        if (CurrentState is not Controllable) return;
        var groundCheck_Stats = CurrentState.Stats_GroundCheck;

        Vector3 origin = transform.position + (groundCheck_Stats.HeightOffset * Vector3.up);
        Vector3 springDir = transform.up;

        Gizmos.DrawRay(origin, -springDir * groundCheck_Stats.HeightCheckBuffer);
        /*
        Gizmos.DrawRay(origin + Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        */

        /* SphereCast visualization
        Vector3 sphereOrigin = origin + (groundCheck_Stats.HeightCheckBuffer * Vector3.down);
        Gizmos.DrawWireSphere(sphereOrigin, groundCheck_Stats.WideCheckBuffer);
        */

        if (!debug_ShowGroundCheck) return;

        Vector3 size = new(groundCheck_Stats.WideCheckBuffer, 0.01f * 2, groundCheck_Stats.WideCheckBuffer);

        Vector3 boxOrigin;
        if (Physics.BoxCast(origin, size / 2, -springDir, out RaycastHit hitInfo, Quaternion.identity, groundCheck_Stats.HeightCheckBuffer))
            boxOrigin = origin + (hitInfo.distance * Vector3.down);
        else
            boxOrigin = origin + (groundCheck_Stats.HeightCheckBuffer * Vector3.down);

        Gizmos.DrawWireCube(boxOrigin, size);
    }

}
