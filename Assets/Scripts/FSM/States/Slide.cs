using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Slide : Grounded
{
    [SerializeField] private float startForce;
    [SerializeField] private float continuosForce;
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float slideDuration;
    public override void Enter()
    {
        base.Enter();

        rb.velocity = rb.transform.forward * startForce;

        InputManager.OnJumpFired += Handle_JumpFired;
    }

    private void Handle_JumpFired()
    {
        nextState = stateComponent.State_Jump;
    }

    public override void FixedRun()
    {
        if (!CheckSlope())
        {
            if (StateDuration >= slideDuration)
            {
                //What if it goes to crouch????
                nextState = stateComponent.State_Stand;
            }
        }
        else if (rb.velocity.y > 2)
            nextState = stateComponent.State_Stand;

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
