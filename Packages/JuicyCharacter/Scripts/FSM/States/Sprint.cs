using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Does literally what stand does but we need it for clarify when it has to go to crouch or to slide!
//(Stand) => C => (Crouch)
//(Sprint) => C => (Slide)
[Serializable]
public class Sprint : Grounded, IMoveableState, ILookableState
{
    [Header("Components")]
    [SerializeField] private StateMovementComponent movementComponent;
    public StateMovementComponent MovementComponent => movementComponent;

    [SerializeField] private StateLookComponent lookComponent;
    public StateLookComponent LookComponent => lookComponent;

    [Space(15)]
    [Header("Sprint Settings")]
    [SerializeField] private float minSpeedToSprint;
    [SerializeField] private float minSpeedToSlide = 5;

    public override void Enter()
    {
        base.Enter();
        movementComponent.BindInput();
        lookComponent.BindInput();

        InputManager.OnJumpFiredRef.Delegate += Handle_JumpFired;
        InputManager.OnCrouchFiredRef.Delegate += Handle_CrouchFired;
    }

    private void Handle_CrouchFired()
    {
        if(rb.velocity.magnitude < minSpeedToSlide) return;

        nextState = stateMachine.State_Slide;
    }

    private void Handle_JumpFired()
    {
        nextState = stateMachine.State_Jump;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(movementComponent.MovementDirection, Camera.main.transform);
        movementComponent.Move(stateMachine, rb, movementDirection);

        lookComponent.Look(stateMachine, rb);

        // Sprint Check
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
    }

    public override void Exit()
    {
        movementComponent.UnbindInput();
        lookComponent.UnbindInput();

        InputManager.OnJumpFiredRef.Delegate -= Handle_JumpFired;
        InputManager.OnCrouchFiredRef.Delegate -= Handle_CrouchFired;
    }

    public override PlayerState Run()
    {
        // We always check if we're running enough fast to be sprinting or walking!
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        bool canSprint = Mathf.Abs(localVelocity.z) >= minSpeedToSprint;

        if(movementComponent.MovementDirection.magnitude <= 0)
            canSprint = false;

        if(!InputManager.current.WantsToSprint)
            canSprint = false;

        if (!canSprint)
            nextState = stateMachine.State_Stand;


        // Other input check (Higher priority)
        base.Run();

        return nextState;
    }
}
