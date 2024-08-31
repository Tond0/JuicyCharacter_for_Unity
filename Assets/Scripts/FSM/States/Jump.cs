using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor.SceneTemplate;
using UnityEngine;

public class Jump : Air
{
    private bool wantToJump = true;
    private Timer timer_jumpForceDuration;

    private enum JumpState { Ascending, Top, Descending  };
    private JumpState jumpState = JumpState.Ascending;
    public Jump(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpReleased += () => wantToJump = false;


        //Do not check the ground while we're ascending
        checkGround = false;

        //Timer for input jumpForce duration
        timer_jumpForceDuration = new Timer(stats.MaxJumpForceDuration * 1000);
        timer_jumpForceDuration.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                wantToJump = false;
                checkGround = true;
            };
        timer_jumpForceDuration.Start();

        //First set of gravity
        gravityMultiplaier = stats.GravityMultiplaier_Ascending;

        //Let's Jump! Force up!
        Vector3 app_velocity = stats.Rb.velocity;
        app_velocity.y = stats.JumpForce;
        stats.Rb.velocity = app_velocity;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpReleased -= () => wantToJump = false;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        switch (jumpState)
        {
            case JumpState.Ascending:

                if (StateDuration >= stats.MaxJumpForceDuration
                    || (StateDuration >= stats.MinJumpForceDuration && !wantToJump))
                    jumpState = JumpState.Top;

                break;

            case JumpState.Top:

                acceleration_Multiplaier = 1.4f;
                gravityMultiplaier = stats.GravityMultiplaier_TopHeight;
                checkGround = true;

                if (stats.Rb.velocity.y < -1)
                    jumpState = JumpState.Descending;

                break;

            case JumpState.Descending:

                acceleration_Multiplaier = 1;
                gravityMultiplaier = stats.GravityMultiplaier_Descending;

                break;
        }
    }
}
