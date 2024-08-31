using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Air : Controllable
{
    protected float gravityMultiplaier = 1;
    public Air(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void FixedRun()
    {
        base.FixedRun();

        if (checkGround && CheckGround(stats.transform, stats.HeightOffset, stats.WideCheckBuffer, stats.HeightCheckBuffer))
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
