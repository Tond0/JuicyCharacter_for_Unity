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

    public override void Enter()
    {
        base.Enter();
        movementComponent.BindInput();
        lookComponent.BindInput();

        InputManager.OnSprintFired += Handle_SprintFireNdReleased;
        InputManager.OnSprintReleased += Handle_SprintFireNdReleased;
        InputManager.OnJumpFired += Handle_JumpFired;
        InputManager.OnCrouchFired += Handle_CrouchFired;
    }

    private void Handle_CrouchFired()
    {
        nextState = stateMachine.State_Slide;
    }

    private void Handle_JumpFired()
    {
        nextState = stateMachine.State_Jump;
    }

    private void Handle_SprintFireNdReleased()
    {
        nextState = stateMachine.State_Stand;
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

        InputManager.OnSprintFired -= Handle_SprintFireNdReleased;
        InputManager.OnSprintReleased -= Handle_SprintFireNdReleased;
        InputManager.OnJumpFired -= Handle_JumpFired;
        InputManager.OnCrouchFired -= Handle_CrouchFired;
    }

    public override PlayerState Run()
    {
        // We always check if we're running enough fast to be sprinting or walking!
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        bool canSprint = localVelocity.z >= minSpeedToSprint;

        if (!canSprint)
            nextState = stateMachine.State_Stand;

        // Other input check (Higher priority)
        base.Run();

        return nextState;
    }
}
