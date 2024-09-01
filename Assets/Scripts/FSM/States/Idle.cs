using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Idle : Grounded
{
    private bool wantToSprint = false;
    private bool canSprint = false;

    public Idle(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        InputManager.OnJumpFired += () => nextState = new Jump(stateComponent, direction, stats.Movement_Air);
        InputManager.OnSprintFired += () => wantToSprint = true;
        InputManager.OnSprintReleased += () => wantToSprint = false; 
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= () => nextState = new Jump(stateComponent, direction, stats.Movement_Air);
        InputManager.OnSprintFired -= () => wantToSprint = true;
        InputManager.OnSprintReleased -= () => wantToSprint = false;
    }

    public override PlayerState Run()
    {
        base.Run();

        canSprint = stats.Rb.velocity.z >= stats.MinSpeedToSprint;

        if (wantToSprint && canSprint)
        {
            movementStats = stats.Movement_Sprint;
        }
        else
            movementStats = stats.Movement_Ground;


        return nextState;
    }   
}
