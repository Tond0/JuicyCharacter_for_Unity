using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Jump : Air
{
    private bool wantToJump = true;
    private Timer timer_jumpForceDuration;
    public Jump(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpReleased += () => wantToJump = false;

        checkGround = false;

        //Timer
        timer_jumpForceDuration = new Timer(stats.JumpForceDuration * 1000);
        timer_jumpForceDuration.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                wantToJump = false; 
                checkGround = true; 
            };
        timer_jumpForceDuration.Start();
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpReleased -= () => wantToJump = false;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        DoJump();
        DoCustomGravity();
    }

    private void DoCustomGravity()
    {
        
    }

    private void DoJump()
    {
        if (!wantToJump) return;

        Vector3 velocity = stats.Rb.velocity;

        velocity.y += stats.JumpForce;

        stats.Rb.velocity = velocity;
    }
}
