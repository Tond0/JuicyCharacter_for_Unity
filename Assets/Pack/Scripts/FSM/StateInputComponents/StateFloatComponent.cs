using System;
using UnityEngine;

public class StateFloatComponent : StateGroundCheckComponent
{
    /// <summary>
    /// Method that handle the spring float force, this approach kills any problem with the slopes and the friction that the character may have moving on the ground.
    /// </summary>
    /// <param name="rayHit"></param>
    public void Float(RaycastHit rayHit)
    {
        //We apply the spring formula ( springForce = offset - damping; )
        //offset = how far is between the position it should be and the current position.
        float offset = height - rayHit.distance;

        //Let's calculate the velocity relative to the direction of the spring
        float rayDirVel = Vector3.Dot(stateMachine.transform.up, rb.velocity);

        //Let's apply the formula ( damping = velocity * dampingForce )
        float damping = rayDirVel * -dampingForce;

        //Always the same formula ( springForce = offset - damping; )
        float springForce = offset * springStrength - damping;

        //Let's add this force
        rb.AddForce(stateMachine.transform.up * springForce);
    }
}