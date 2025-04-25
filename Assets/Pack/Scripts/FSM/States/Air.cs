using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Timers;
using UnityEngine;

public abstract class Air : PlayerState, IGroundCheckableState
{
    [Header("Components")]
    [SerializeField] protected StateGroundCheckComponent groundCheckComponent;
    public StateGroundCheckComponent GroundCheckComponent => groundCheckComponent;

    [Space(15)]
    [SerializeField, Tooltip("Default gravity force is -9.81f")] private float gravityForce = -9.81f;

    //Really useful to change gravity in runtime without messing with the actual gravityForce (used by the jump state)
    protected float gravityMultiplaier = 1;

    //should we check the ground?
    protected bool checkGround = true;


    public override void Enter()
    {
        base.Enter();

        InputManager.OnWallRunFired += Handle_WallRunFired;
    }

    //Check for any wall to wallrun
    private void Handle_WallRunFired()
    {
        if(stateMachine.CurrentState is WallRunning) return;

        //If we are not on the ground and we are not wallrunning, we can check for wallrun
        if(stateMachine.State_Wallrunning.CheckWallRunInitializer())
            nextState = stateMachine.State_Wallrunning;
    }

    public override void FixedRun()
    {
        //If we want to check the ground we check it...
        if (checkGround && groundCheckComponent.CheckGround(stateMachine))
            //If we're on ground we stand!
            nextState = GetLastGroundState();

        //If we are not on the ground we use custom gravity
        CustomGravity();
    }

    public override void Exit()
    {
        InputManager.OnWallRunFired -= Handle_WallRunFired;
    }

    /// <summary>
    /// Get the last grounded state, if the previous state is not grounded then we return the Stand state.
    /// </summary>
    /// <returns></returns>
    protected virtual Grounded GetLastGroundState()
    { 
        if(stateMachine.PreviousState is Grounded grounded)
            return grounded;
        else
            return stateMachine.State_Stand;
            
    }

    /// <summary>
    /// Just apply gravity, to edit this gravity force please use the gravityMultiplaier variable
    /// </summary>
    private void CustomGravity()
    {
        rb.AddForce(gravityForce * gravityMultiplaier * Vector3.up, ForceMode.Acceleration);
    }
}
        
