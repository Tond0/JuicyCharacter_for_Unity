using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Does literally what stand does but we need it for clarify when it has to go to crouch or to slide!
//(Stand) => C => (Crouch)
//(Sprint) => C => (Slide)
[Serializable]
public class Sprint : Grounded
{
    [SerializeField] private float minSpeedToSprint;
    public override void Enter()
    {
        base.Enter();

        InputManager.OnSprintFired += Handle_SprintFireNdReleased;
        InputManager.OnSprintReleased += Handle_SprintFireNdReleased;
        InputManager.OnJumpFired += Handle_JumpFired;
        InputManager.OnCrouchFired += Handle_CrouchFired;
    }

    private void Handle_CrouchFired()
    {
        nextState = stateComponent.State_Slide;
    }

    private void Handle_JumpFired()
    {
        nextState = stateComponent.State_Jump;
    }

    private void Handle_SprintFireNdReleased()
    {
        nextState = stateComponent.State_Stand;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnSprintFired -= Handle_SprintFireNdReleased;
        InputManager.OnSprintReleased -= Handle_SprintFireNdReleased;
        InputManager.OnJumpFired -= Handle_JumpFired;
        InputManager.OnCrouchFired -= Handle_CrouchFired; 
    }

    public override PlayerState Run()
    {
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        bool canSprint = localVelocity.z >= minSpeedToSprint;

        if (!canSprint)
            nextState = stateComponent.State_Stand;

        //Other input check (Higher priority)
        base.Run();

        return nextState;
    }
}
