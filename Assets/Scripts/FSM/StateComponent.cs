using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class StateComponent : MonoBehaviour
{
    private PlayerState currentState;
    public PlayerState CurrentState { get { return currentState; } }

    [SerializeField] private PlayerStats playerStats;
    public PlayerStats PlayerStats { get { return playerStats; } }


    [Header("States")]
    [SerializeField] private Stand state_Stand;
    [SerializeField] private Stand state_Sprint;
    [SerializeField] private Air state_Air;
    [SerializeField] private Jump state_Jump;
    //Getter
    public Stand State_Stand { get => state_Stand; }
    public Jump State_Jump { get => state_Jump; }
    public Air State_Air { get => state_Air; }
    public Stand State_Sprint { get => state_Sprint; }

    private void Start()
    {
        currentState = State_Stand;
        currentState.Enter();
    }

    private void Update()
    {
        RunCurrentState();
        Debug.Log(CurrentState.ToSafeString());
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
    }

}
