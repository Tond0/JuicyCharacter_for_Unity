using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Jump : Air
{
    private bool wantToJump = true;
    private Timer timer_jumpForceDuration;

    private Coroutine co_changeGravityMultiplaier;
    public Jump(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpReleased += () => wantToJump = false;

        Vector3 app_velocity = stats.Rb.velocity;
        app_velocity.y = 0;
        stats.Rb.velocity = app_velocity;

        checkGround = false;

        //Timer for input jumpForce duration
        timer_jumpForceDuration = new Timer(stats.JumpForceDuration * 1000);
        timer_jumpForceDuration.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                wantToJump = false; 
                checkGround = true; 
            };
        timer_jumpForceDuration.Start();

        co_changeGravityMultiplaier = stateComponent.StartCoroutine(TimedGravityChange(stats.GravityChange));
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpReleased -= () => wantToJump = false;

        stateComponent.StopCoroutine(co_changeGravityMultiplaier);
    }

    private IEnumerator TimedGravityChange(PlayerStats.GravityStats[] gravityChanges)
    {
        for(int i = 0; i < gravityChanges.Length; i++)
        {
            yield return new WaitForSeconds(gravityChanges[i].Duration);
            gravityMultiplaier = gravityChanges[i].GravityMultiplaier;
        }
    }

    public override PlayerState Run()
    {
        return base.Run();
    }

    public override void FixedRun()
    {
        base.FixedRun();

        DoJump();
    }

    private void DoJump()
    {
        if (!wantToJump) return;

        Vector3 velocity = stats.Rb.velocity;

        velocity.y += stats.JumpForce;

        stats.Rb.velocity = velocity;
    }
}
