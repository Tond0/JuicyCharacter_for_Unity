using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[Serializable]
public class Falling : Air
{
    /// <summary>
    /// Can it takes advantages of the coyote jump? 
    /// </summary>
    private bool canCoyoteJump;

    public override void Enter()
    {
        base.Enter();

        canCoyoteJump = true;
        
        //If we're jumping while falling we check the coyote time
        InputManager.OnJumpFired += CheckCoyoteTime;

        //After this timer we won't be able to use the coyote jump
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

    /// <summary>
    /// Method played by the timer to deny the jump while falling
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Handle_CoyoteTimeEnd(object sender, ElapsedEventArgs e)
    {
        canCoyoteJump = false;
        InputManager.OnJumpFired -= CheckCoyoteTime;
    }

    /// <summary>
    /// Method to check if we can jump even if we're falling
    /// </summary>
    private void CheckCoyoteTime()
    {
        //Can we coyote jump?...
        if (!canCoyoteJump) return;
        //We jump!
        nextState = stateComponent.State_Jump;
    }
}
