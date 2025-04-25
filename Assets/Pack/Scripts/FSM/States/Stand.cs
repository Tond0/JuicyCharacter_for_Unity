using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Stand : Grounded, IMoveableState, ILookableState
{
    [Header("Components")]
    [SerializeField] protected StateMovementComponent movementComponent;
    public StateMovementComponent MovementComponent => movementComponent;

    [SerializeField] protected StateLookComponent lookComponent;
    public StateLookComponent LookComponent => lookComponent;

    [Space(15)]
    [SerializeField,Tooltip("How fast should the player be to sprint?")] private float minSpeedToSprint;


    public override void Enter()
    {
        base.Enter();

        // Inputs
        movementComponent.BindInput();
        lookComponent.BindInput();

        InputManager.OnJumpFired += Handle_JumpFired;
        InputManager.OnCrouchFired += Handle_CrouchFired;
        InputManager.OnSprintFired += Handle_SprintFired;
    }

    #region Events Handler
    private void Handle_CrouchFired()
    {
        nextState = stateMachine.State_Crouch;
    }

    private void Handle_JumpFired()
    {
        nextState = stateMachine.State_Jump;
    }

    private void Handle_SprintFired()
    {
        // Sprint Check
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);

        if (localVelocity.z >= minSpeedToSprint)
            nextState = stateMachine.State_Sprint;
    }
    #endregion

    public override void FixedRun()
    {
        base.FixedRun();

        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(movementComponent.MovementDirection, Camera.main.transform);
        movementComponent.Move(stateMachine, rb, movementDirection);

        lookComponent.Look(stateMachine, rb);
    }

    public override void Exit()
    {
        movementComponent.UnbindInput();
        lookComponent.UnbindInput();

        InputManager.OnJumpFired -= Handle_JumpFired;
        InputManager.OnCrouchFired -= Handle_CrouchFired;
        InputManager.OnSprintFired -= Handle_SprintFired;
    }

    public override PlayerState Run()
    {       
        base.Run();

        return nextState;
    }   
}
