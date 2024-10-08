using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Stand : Grounded
{
    [Space(15)]
    [SerializeField,Tooltip("How fast should the player be to sprint?")] private float minSpeedToSprint;
    public override void Enter()
    {
        base.Enter();

        InputManager.OnJumpFired += Handle_JumpFired;
        InputManager.OnCrouchFired += Handle_CrouchFired;
        InputManager.OnSprintFired += TrySprint;
    }

    #region Events Handler
    private void Handle_CrouchFired()
    {
        nextState = stateComponent.State_Crouch;
    }

    private void Handle_JumpFired()
    {
        nextState = stateComponent.State_Jump;
    }

    private void TrySprint()
    {
        //Sprint Check
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);

        if (localVelocity.z >= minSpeedToSprint)
            nextState = stateComponent.State_Sprint;
    }
    #endregion

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= Handle_JumpFired;
        InputManager.OnCrouchFired -= Handle_CrouchFired;
        InputManager.OnSprintFired -= TrySprint;
    }

    public override PlayerState Run()
    {       
        base.Run();

        return nextState;
    }   

    
}
