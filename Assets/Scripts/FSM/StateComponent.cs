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
    private void Start()
    {
        currentState = new Idle(this, Vector3.zero, PlayerStats.Movement_Ground);
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
