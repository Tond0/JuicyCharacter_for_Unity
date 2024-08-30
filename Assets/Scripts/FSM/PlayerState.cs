using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class PlayerState
{
    protected PlayerState nextState;
    protected StateComponent stateComponent;

    protected PlayerState(StateComponent stateComponent)
    {
        this.stateComponent = stateComponent;
    }

    public abstract void Enter();
    public abstract void FixedRun();
    public abstract PlayerState Run();
    public abstract void Exit();
}
