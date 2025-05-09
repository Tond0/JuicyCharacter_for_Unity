using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Standing : Grounded, IMoveableState, ILookableState
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

        InputManager.OnJumpFiredRef.Delegate += Handle_JumpFired;
        InputManager.OnCrouchFiredRef.Delegate += Handle_CrouchFired;
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
    #endregion

    public override void FixedRun()
    {
        base.FixedRun();

        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(movementComponent.MovementDirection, Camera.main.transform);
        movementComponent.Move(stateMachine, rb, movementDirection);

        lookComponent.Look(stateMachine, rb);

        // Sprint Check
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);

        if(InputManager.current.WantsToSprint && localVelocity.z >= minSpeedToSprint)
            nextState = stateMachine.State_Sprint;
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
        base.Run();

        return nextState;
    }   
}
