using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[Serializable]
public class Jump : Air
{
    [Space(10)]
    [SerializeField, Tooltip("The force the jump will have ONCE, not continuous")] private float jumpForce;
    [SerializeField, Tooltip("When the player is at the top of the jump do we boost the acceleration? This is useful so the player can decide where to land mid air easier")] private float airtTime_AccelMultiplaier;
    [SerializeField, Tooltip("When we change from StandState to FallingState a timer will start and if we press jump before the timer ends, we jump even if we're not on the ground. This decides the timer lenght")] private float coyoteTime = 0.3f;
    [SerializeField, Tooltip("What's the max time that takes (without releasing jump) to reach the top height?")] private float maxJumpDuration;
    [SerializeField, Tooltip("What's the min time that we can jump (even if we release the jump button instantaneously)?")] private float minJumpDuration;
    [Space(10)]
    [SerializeField, Tooltip("Gravity when aiming for the top height")] private float gravityMultiplaier_Ascending;
    [SerializeField, Tooltip("Gravity when jump button is released. Higher value = more responsive cut off of the jump")] private float gravityMultiplaier_InputReleased;
    [SerializeField, Tooltip("Graivity when we reach the top of the jump")] private float gravityMultiplaier_TopHeight;
    [SerializeField, Tooltip("Gravity when going from the top of the jump to the ground again")] private float gravityMultiplaier_Descending;

    //The 3 possible jump state
    private enum JumpState { Ascending, Top, Descending  };
    private JumpState jumpState = JumpState.Ascending;

    //Do we want to keep jumping?
    private bool wantToJump;

    //Getter so the air state knows when it can and can't transition to jump state if button is pressed
    public float CoyoteTime { get => coyoteTime; }

    public override void Enter()
    {
        base.Enter();

        //We sure want to jump!s
        wantToJump = true;

        //If we release we dont want anymore to jump
        InputManager.OnJumpReleased += Handle_JumpReleased;

        //We're trying to reach the top of the jump now
        jumpState = JumpState.Ascending;

        //Do not check the ground while we're ascending!
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

        //Check jump behaviour
        switch (jumpState)
        {
            case JumpState.Ascending:

                //If we released the jump button, is the min duration elapsed?
                //OR
                //If we didn't release the jump button yet is the max duration elapsed?
                if (StateDuration >= minJumpDuration && !wantToJump
                    || StateDuration >= maxJumpDuration)
                {
                    //Boost speed
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_InputReleased;
                }

                //If we're close to 0 with the velocity...
                if(rb.velocity.y < 2)
                {
                    //We're at the top of the jump
                    jumpState = JumpState.Top;

                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_TopHeight;
                    //Apply boost to the acceleration
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;

                    //We can start check the ground again
                    checkGround = true;
                }

                break;

            case JumpState.Top:

                //We're now descending?...
                if (rb.velocity.y < 0)
                {
                    //We're trying to reach the ground again
                    jumpState = JumpState.Descending;

                    //Reset acceleration boost
                    acceleration_Multiplaier = 1;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_Descending;
                }

                break;

            //Nothing to do, just wait to fully descend and touch grass again
            case JumpState.Descending:
                break;
        }

        return nextState;
    }
}
