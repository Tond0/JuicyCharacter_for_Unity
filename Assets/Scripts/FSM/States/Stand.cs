using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Stand : Grounded
{
    [SerializeField] private float minSpeedToSprint;
    private bool canSprint;

    public override void Enter()
    {
        base.Enter();

        canSprint = false;

        InputManager.OnJumpFired += () => nextState = stateComponent.State_Jump;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnJumpFired -= () => nextState = stateComponent.State_Jump;
    }

    public override PlayerState Run()
    {
        base.Run();

        if (nextState != null) return nextState;

        canSprint = rb.velocity.z >= minSpeedToSprint;

        if (InputManager.current.WantToSprint && canSprint)
            nextState = stateComponent.State_Sprint;
        else
            nextState = stateComponent.State_Stand;


        return nextState;
    }   
}
