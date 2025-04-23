using System;
using UnityEngine;

public class StateGroundCheckComponent : StateComponent
{
    [SerializeField, Tooltip("The distance we want between the player and the ground. WARNING: This should be tuned with the heightCheckBuffer and the heightOffset, cause they both influence the distance between the player and the ground")] protected float height;
    [SerializeField, Tooltip("Should the check begin higher? This improve slope detection and climb when falling")] protected float heightOffset;
    [SerializeField, Tooltip("How down the check should run? This impreve sticking to slopes when going down one")] protected float heightCheckBuffer;
    [SerializeField, Tooltip("How big is the check for the ground? This improve step detection and when falling can help the player standing on the platform even if is not actually on the platform, a value too high can mess with slope detection")] protected float wideCheckBuffer;
    [SerializeField, Tooltip("How fast should we reach the desire height?")] protected float springStrength;
    [SerializeField, Tooltip("How much should we damp before reaching the desire height?")] protected float dampingForce;
    public bool IsGrounded { get; private set; } = false;
    public RaycastHit RayHit { get; private set; } = new RaycastHit();

    /// <summary>
    /// Checks the ground returning info on the ground (if hitted)
    /// </summary>
    /// <returns></returns>
    protected bool CheckGround(out RaycastHit rayHit)
    {
        //Origin of the ground check depends on the heightOffset we give to it
        Vector3 origin = stateMachine.transform.position + (heightOffset * Vector3.up);

        //Half size
        Vector3 halfExtends = new(wideCheckBuffer / 2, 0.01f, wideCheckBuffer / 2);

        if (Physics.BoxCast(origin, halfExtends, -stateMachine.transform.up, out rayHit, Quaternion.identity, heightCheckBuffer))
            return true;

        return false;
    }

    /// <summary>
    /// Checks the ground without returning any info on the ground
    /// </summary>
    /// <returns></returns>
    protected bool CheckGround()
    {
        //Origin of the ground check depends on the heightOffset we give to it
        Vector3 origin = stateMachine.transform.position + (heightOffset * Vector3.up);

        //Half size
        Vector3 halfExtends = new(wideCheckBuffer / 2, 0.01f, wideCheckBuffer / 2);

        if (Physics.BoxCast(origin, halfExtends, -stateMachine.transform.up, Quaternion.identity, heightCheckBuffer))
            return true;

        return false;
    }
}