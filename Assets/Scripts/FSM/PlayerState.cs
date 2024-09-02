using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class PlayerState
{
    protected PlayerState nextState;
    [SerializeField] protected StateComponent stateComponent;
    protected Stopwatch stopwatch_state;
    public float StateDuration { get { return (float)stopwatch_state.Elapsed.TotalSeconds; } }

    public virtual void Enter()
    {
        stopwatch_state = new();
        stopwatch_state.Start();
    }

    public abstract void FixedRun();
    public abstract PlayerState Run();
    public virtual void Exit()
    {
        stopwatch_state.Reset();
        nextState = null;
    }
}
