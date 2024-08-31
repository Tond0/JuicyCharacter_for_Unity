using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class PlayerState
{
    protected PlayerState nextState;
    protected StateComponent stateComponent;
    protected Stopwatch stopwatch_state = new();
    public float StateDuration { get { return (float)stopwatch_state.Elapsed.Milliseconds; } }
    protected PlayerState(StateComponent stateComponent)
    {
        this.stateComponent = stateComponent;
    }

    public virtual void Enter()
    {
        stopwatch_state.Start();
    }

    public abstract void FixedRun();
    public abstract PlayerState Run();
    public abstract void Exit();
}
