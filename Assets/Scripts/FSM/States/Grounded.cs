using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// Any state that inherit from this abstract class is going to be on the ground
/// </summary>
public abstract class Grounded : Controllable
{
    [Space(15)]
    [SerializeField, Tooltip("How much should the head move up and down?"), Range(0, 5)] private float headBobbingFrequency = 1;
    //Getter so that CinemachineHeadbobber.cs can adjust the frequency of the noise effect
    public float HeadBobbingFrequency { get => headBobbingFrequency; }

    public override void FixedRun()
    {
        base.FixedRun();

        //Check if we're still touching the ground
        if (CheckGround(out RaycastHit rayHit))
            //If we are, then float!
            Float(stateComponent.transform, rb, rayHit, Stats_GroundCheck.Height, Stats_GroundCheck.DampingForce, Stats_GroundCheck.SpringStrength);
        else
            //If we are not then we're falling
            nextState = stateComponent.State_Falling;
    }

    /// <summary>
    /// Method that handle the spring float force, this approach kills any problem with the slopes and the friction that the character may have moving on the ground!
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="rb"></param>
    /// <param name="rayHit"></param>
    /// <param name="height"></param>
    /// <param name="dampingForce"></param>
    /// <param name="springStrength"></param>
    protected void Float(Transform transform, Rigidbody rb, RaycastHit rayHit, float height, float dampingForce, float springStrength)
    {
        //We apply the spring formula ( springForce = offset - damping; )
        //offset = how far is between the position it should be and the current position.
        float offset = height - rayHit.distance;

        //Let's calculate the velocity relative to the direction of the spring
        float rayDirVel = Vector3.Dot(transform.up, rb.velocity);

        //Let's apply the formula ( damping = velocity * dampingForce )
        float damping = rayDirVel * -dampingForce;

        //Always the same formula ( springForce = offset - damping; )
        float springForce = offset * springStrength - damping;

        //Let's add this force
        rb.AddForce(transform.up * springForce);
    }
}
