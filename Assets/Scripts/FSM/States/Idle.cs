using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Idle : Grounded
{
    public Idle(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpFired += () => nextState = new Jump(stateComponent, direction, stats.Movement_Air);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override PlayerState Run()
    {
        base.Run();
        return nextState;
    }
}
