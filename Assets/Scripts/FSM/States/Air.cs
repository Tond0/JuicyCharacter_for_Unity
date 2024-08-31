using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Air : Controllable
{
    protected float gravityMultiplaier = 1;
    public Air(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpFired += CheckCoyoteTime;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= CheckCoyoteTime;
    }

    private void CheckCoyoteTime()
    {
        if (StateDuration > stats.CoyoteTime) return;

        nextState = new Jump(stateComponent, direction, stats.Movement_Ground);
    }

    public override void FixedRun()
    {
        base.FixedRun();

        if (checkGround && CheckGround(stats.transform, stats.GroundCheck_Air))
            nextState = new Idle(stateComponent, direction, stats.Movement_Ground);

        CustomGravity();
    }

    protected bool useGravity = true;
    protected bool checkGround = true;
    private void CustomGravity()
    {
        if (!useGravity) return;

        stats.Rb.AddForce(stats.GravityForce * gravityMultiplaier * Vector3.up, ForceMode.Acceleration);
    }
}
