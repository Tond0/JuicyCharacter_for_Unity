using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public abstract class Air : Controllable
{
    [Space(15)]
    [SerializeField, Tooltip("Default gravity force is -9.81f")] private float gravityForce = -9.81f;

    //should we use the custom gravity?
    protected bool useGravity = true;
    //Really useful to change gravity in runtime without messing with the actual gravityForce (used by the jump state)
    protected float gravityMultiplaier = 1;

    //should we check the ground?
    protected bool checkGround = true;

    public override void Enter()
    {
        base.Enter();

        //We share the same max speed, so player won't feel punished when jumping.
        stats_Movement.maxSpeed = GetNextGroundedState().Stats_Movement.maxSpeed;

        InputManager.OnWallRunFired += TryWallRun;
    }

    //Check for any wall to wallrun
    private void TryWallRun()
    {
        if(stateComponent.State_Wallrunning.CheckWallRunInitializer())
            nextState = stateComponent.State_Wallrunning;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        //If we want to check the ground we check it...
        if (checkGround && CheckGround())
            //If we're on ground we stand!
            nextState = GetNextGroundedState();

        //If we are not on the ground we use custom gravity
        CustomGravity();
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnWallRunFired -= TryWallRun;
    }

    //The state we will come back to once we're going to touch the ground again.
    protected virtual Grounded GetNextGroundedState()
    { 
        if(stateComponent.PreviousState is Grounded grounded)
            return grounded;
        else
            return stateComponent.State_Stand;
            
    }

    /// <summary>
    /// Just apply gravity, to edit this gravity force please use the gravityMultiplaier variable
    /// </summary>
    private void CustomGravity()
    {
        if (!useGravity) return;

        rb.AddForce(gravityForce * gravityMultiplaier * Vector3.up, ForceMode.Acceleration);
    }
}
        
