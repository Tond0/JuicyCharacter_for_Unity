using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sliding : Grounded
{
    [SerializeField, Tooltip("Start impulse, applied once")] private float startForce;
    [SerializeField, Tooltip("The impulse applied continuosly")] private float continuosForce;
    [SerializeField, Tooltip("How height can we climb a slope sliding before stopping the slide?")] private float maxSlopeAngle;
    [SerializeField, Tooltip("How long should the slide last?")] private float maxSlideDuration;
    [SerializeField, Tooltip("How long should the slide last?")] private float minSlideDuration;
    private bool slideCancelBuffer = false;

    /// <summary>
    /// The time we have been sliding for.
    /// </summary>
    private float currentSlideDuration = 0;
    public override void Enter()
    {
        base.Enter();

        //Apply slide initial force
        rb.velocity = rb.transform.forward * startForce;

        //Slide cancel if we jump
        InputManager.OnJumpFiredRef.Delegate += Handle_JumpFired;
        InputManager.OnCrouchFiredRef.Delegate += Handle_CrouchReleased;
        InputManager.OnCrouchReleasedRef.Delegate += Handle_CrouchReleased;
    }

    private void Handle_CrouchReleased()
    {
        if (StateDuration < minSlideDuration)
        {
            slideCancelBuffer = true;
            return;
        }

        CancelSlide();
    }

    private void CancelSlide()
    {
        if (stateMachine.State_Crouch.CheckTop())
            //We can stand! 
            nextState = stateMachine.State_Stand;
        else
            //If we can stand...
            nextState = stateMachine.State_Crouch;
    }

    private void Handle_JumpFired() => nextState = stateMachine.State_Jump;

    public override void FixedRun()
    {
        if (CheckSlope())
        {
            if (rb.velocity.y > 5)
            {
                // Stop the slide if moving upwards too fast
                CancelSlide();
            }
            else
            {
                // Apply continuous force while sliding
                rb.velocity += rb.transform.forward * continuosForce;
            }
        }
        else
        {
            // Gradually reduce slide velocity on flat ground or invalid slope
            ReduceSlide();

            //We count the slide       
            currentSlideDuration = StateDuration;
        }

        // Transition to crouch if slide duration exceeds maximum allowed time
        if (currentSlideDuration >= maxSlideDuration)
        {
            nextState = stateMachine.State_Crouch;
        }

        // Handle buffered slide cancel if minimum duration has passed
        if (slideCancelBuffer && StateDuration > minSlideDuration)
        {
            CancelSlide();
        }

        base.FixedRun();
    }

    public override void Exit()
    {
        slideCancelBuffer = false;

        InputManager.OnJumpFiredRef.Delegate -= Handle_JumpFired;

        InputManager.OnCrouchFiredRef.Delegate -= Handle_CrouchReleased;
        InputManager.OnCrouchReleasedRef.Delegate -= Handle_CrouchReleased;
    }

    private void ReduceSlide()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = Vector3.zero;

        // Calcola la velocità con cui vogliamo avvicinarci a zero
        float slideDeceleration = currentVelocity.magnitude / maxSlideDuration;

        // Applica il MoveTowards verso zero
        Vector3 newVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, slideDeceleration * Time.deltaTime);

        rb.velocity = newVelocity;
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
