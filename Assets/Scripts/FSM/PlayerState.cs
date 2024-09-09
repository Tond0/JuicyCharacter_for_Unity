using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Any class inheriting this will be a player state
/// </summary>
public abstract class PlayerState
{
    //The components that handles the states
    [SerializeField] protected StateComponent stateComponent;

    //The state, this state, want to transition to
    protected PlayerState nextState;

    //A stopwatch used to keep track of how much time this state is being active for. It resets itself each time the state enter 
    protected Stopwatch stopwatch_state;
    //For how much time is the state running?
    public float StateDuration { get { return (float)stopwatch_state.Elapsed.TotalSeconds; } }

    /// <summary>
    /// It runs once when the state begin
    /// </summary>
    public virtual void Enter()
    {
        stopwatch_state = new();
        stopwatch_state.Start();
        nextState = null;
    }

    /// <summary>
    /// It runs in the fixedUpdate
    /// </summary>
    public abstract void FixedRun();

    /// <summary>
    /// It runs in the Update
    /// </summary>
    /// <returns></returns>
    public virtual PlayerState Run() { return nextState; }
    
    /// <summary>
    /// It runs once before the state end
    /// </summary>
    public abstract void Exit();
}
