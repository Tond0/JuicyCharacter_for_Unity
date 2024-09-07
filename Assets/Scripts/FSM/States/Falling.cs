using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[Serializable]
public class Falling : Air
{
    private bool canCoyoteJump;
    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpFired += CheckCoyoteTime;

        canCoyoteJump = true;

        Timer coyoteTimer = new(stateComponent.State_Jump.CoyoteTime * 1000);
        coyoteTimer.Elapsed += Handle_CoyoteTimeEnd;
        coyoteTimer.AutoReset = false;
        coyoteTimer.Start();
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= CheckCoyoteTime;
    }

    private void Handle_CoyoteTimeEnd(object sender, ElapsedEventArgs e)
    {
        canCoyoteJump = false;
        InputManager.OnJumpFired -= CheckCoyoteTime;
    }

    private void CheckCoyoteTime()
    {
        if (!canCoyoteJump) return;

        nextState = stateComponent.State_Jump;
    }
}
