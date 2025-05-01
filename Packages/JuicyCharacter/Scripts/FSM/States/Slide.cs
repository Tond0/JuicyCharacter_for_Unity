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
    [SerializeField, Tooltip("How long should the slide last?")] private float maxSlideDuration;
    [SerializeField, Tooltip("How long should the slide last?")] private float minSlideDuration;
    private bool wantsToSlideCance = false;
    public override void Enter()
    {
        base.Enter();

        //Apply slide initial force
        rb.velocity = rb.transform.forward * startForce;

        //Slide cancel if we jump
        InputManager.OnJumpFiredRef.Delegate += Handle_JumpFired;
        InputManager.OnCrouchReleasedRef.Delegate += Handle_CrouchReleased;
    }

    private void Handle_CrouchReleased() 
    {
        if(StateDuration < minSlideDuration)
        { 
            wantsToSlideCance = true;
            return;
        }

        SlideCancel();
    }

    private void SlideCancel()
    {
        if(stateMachine.State_Crouch.CheckTop())
            //We can stand! 
            nextState = stateMachine.State_Stand;
        else
            //If we can stand...
            nextState = stateMachine.State_Crouch;
    }

    private void Handle_JumpFired() => nextState = stateMachine.State_Jump;

    public override void FixedRun()
    {
        //If there's no slope...
        if (!CheckSlope())
        {
            //We count the duration
            if (StateDuration >= maxSlideDuration)
            {
                //Duration ended we transition to crouch
                nextState = stateMachine.State_Crouch;
            }
        }
        //Even if we're not on a slope we check if we're are going upwards
        else if (rb.velocity.y > 5)
            //We stop the slide
            nextState = stateMachine.State_Crouch;

        //Keep apply the slide
        rb.velocity += rb.transform.forward * continuosForce;

        if(wantsToSlideCance)
            SlideCancel();

        base.FixedRun();
    }

    public override void Exit()
    {
        wantsToSlideCance = false;

        InputManager.OnJumpFiredRef.Delegate -= Handle_JumpFired;
        InputManager.OnCrouchReleasedRef.Delegate -= Handle_CrouchReleased;
    }
    
    private bool CheckSlope()
    {
        if (groundCheckComponent.CheckGround(stateMachine, out RaycastHit hitInfo))
        {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        
        return false;
    }
}
