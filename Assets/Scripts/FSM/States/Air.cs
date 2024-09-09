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

    public override void FixedRun()
    {
        base.FixedRun();

        //If we want to check the ground we check it...
        if (checkGround && CheckGround())
            //If we're on ground we stand!
            nextState = stateComponent.State_Stand;

        //If we are not on the ground we use custom gravity
        CustomGravity();
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
