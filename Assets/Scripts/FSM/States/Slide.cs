using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Slide : Grounded
{
    [SerializeField, Tooltip("Start impulse, applied once")] private float startForce;
    [SerializeField, Tooltip("The impulse applied continuosly")] private float continuosForce;
    [SerializeField, Tooltip("How height can we climb a slope sliding before stopping the slide?")] private float maxSlopeAngle;
    [SerializeField, Tooltip("How long should the slide last?")] private float slideDuration;
    public override void Enter()
    {
        base.Enter();

        //Apply slide initial force
        rb.velocity = rb.transform.forward * startForce;

        //Slide cancel if we jump
        InputManager.OnJumpFired += Handle_JumpFired;
    }

    private void Handle_JumpFired()
    {
        nextState = stateComponent.State_Jump;
    }

    public override void FixedRun()
    {
        //If there's no slope...
        if (!CheckSlope())
        {
            //We count the duration
            if (StateDuration >= slideDuration)
            {
                //Duration ended we transition to crouch
                nextState = stateComponent.State_Crouch;
            }
        }
        //Even if we're not on a slope we check if we're are going upwards
        else if (rb.velocity.y > 5)
            //We stop the slide
            nextState = stateComponent.State_Crouch;

        //Keep apply the slide
        rb.velocity += rb.transform.forward * continuosForce;

        base.FixedRun();
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= Handle_JumpFired;
    }
    
    private bool CheckSlope()
    {
        if (CheckGround(out RaycastHit hitInfo))
        {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        
        return false;
    }
}
