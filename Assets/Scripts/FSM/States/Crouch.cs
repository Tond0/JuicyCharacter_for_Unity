using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Crouch : Grounded
{
    public override void Enter()
    {
        base.Enter();

        InputManager.OnCrouchFired += Handle_CrochFireNdReleased;
        InputManager.OnCrouchReleased += Handle_CrochFireNdReleased;
        
        InputManager.OnJumpFired += Handle_CrochFireNdReleased;
    }

    private void Handle_CrochFireNdReleased()
    {
        nextState = stateComponent.State_Stand;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnCrouchFired -= Handle_CrochFireNdReleased;
        InputManager.OnCrouchReleased -= Handle_CrochFireNdReleased;
        InputManager.OnJumpFired -= Handle_CrochFireNdReleased;
    }
}
