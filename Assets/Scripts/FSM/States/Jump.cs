using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor.SceneTemplate;
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
    private Timer timer_jumpForceDuration;

    private enum JumpState { Ascending, Top, Descending  };
    private JumpState jumpState = JumpState.Ascending;

    public float CoyoteTime { get => coyoteTime; }

    public override void Enter()
    {
        base.Enter();

        //Do not check the ground while we're ascending
        checkGround = false;

        //Timer for input jumpForce duration
        timer_jumpForceDuration = new Timer(maxJumpDuration * 1000);
        timer_jumpForceDuration.Elapsed += (object sender, ElapsedEventArgs e) => checkGround = true;
        timer_jumpForceDuration.Start();

        //First set of gravity
        gravityMultiplaier = gravityMultiplaier_Ascending;

        //Let's Jump! Force up!
        Vector3 app_velocity = rb.velocity;
        app_velocity.y = jumpForce;
        rb.velocity = app_velocity;
    }

    public override PlayerState Run()
    {
        base.Run();

        switch (jumpState)
        {
            case JumpState.Ascending:

                if(StateDuration >= minJumpDuration && !InputManager.current.WantToJump)
                {
                    //Boost speed
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_InputReleased;
                }
                else if (StateDuration >= maxJumpDuration)
                {
                      jumpState = JumpState.Top;
                    
                    //Boost speed
                    acceleration_Multiplaier = airtTime_AccelMultiplaier;
                    //Apply top height gravity
                    gravityMultiplaier = gravityMultiplaier_TopHeight;
                    
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
