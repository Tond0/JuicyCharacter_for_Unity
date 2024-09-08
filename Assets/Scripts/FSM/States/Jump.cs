using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[Serializable]
public class Jump : Air
{
    [Space(10)]
    [SerializeField] private float jumpForce;
    [SerializeField] private float airtTime_AccelMultiplaier;
    [SerializeField] private float coyoteTime = 0.3f;
    [SerializeField] private float maxJumpDuration;
    [SerializeField] private float minJumpDuration;
    [Space(10)]
    [SerializeField] private float gravityMultiplaier_Ascending;
    [SerializeField] private float gravityMultiplaier_InputReleased;
    [SerializeField] private float gravityMultiplaier_TopHeight;
    [SerializeField] private float gravityMultiplaier_Descending;

    private enum JumpState { Ascending, Top, Descending  };
    private JumpState jumpState = JumpState.Ascending;

    private bool wantToJump;
    public float CoyoteTime { get => coyoteTime; }

    public override void Enter()
    {
        base.Enter();

        wantToJump = true;

        InputManager.OnJumpReleased += Handle_JumpReleased;

        jumpState = JumpState.Ascending;

        //Do not check the ground while we're ascending
        checkGround = false;

        //First set of gravity
        gravityMultiplaier = gravityMultiplaier_Ascending;

        //Let's Jump! Force up!
        Vector3 app_velocity = rb.velocity;
        app_velocity.y = jumpForce;
        rb.velocity = app_velocity;
    }

    private void Handle_JumpReleased()
    {
        wantToJump = false;
        InputManager.OnJumpReleased -= Handle_JumpReleased;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpReleased -= Handle_JumpReleased;
    }

    public override PlayerState Run()
    {
        base.Run();

        switch (jumpState)
        {
            case JumpState.Ascending:

                if (StateDuration >= minJumpDuration && !wantToJump
                    || StateDuration >= maxJumpDuration)
                {
                    //Boost speed
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_InputReleased;
                }

                if(rb.velocity.y < 2)
                {
                    jumpState = JumpState.Top;
                    gravityMultiplaier = gravityMultiplaier_TopHeight;
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;

                    checkGround = true;
                }

                break;

            case JumpState.Top:


                if (rb.velocity.y < 0)
                {
                    jumpState = JumpState.Descending;
                    acceleration_Multiplaier = 1;
                    gravityMultiplaier = gravityMultiplaier_Descending;
                }

                break;

            //Nothing to do, just wait to fully descend
            case JumpState.Descending:
                break;
        }

        return nextState;
    }
}
