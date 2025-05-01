using System;
using UnityEngine;

[Serializable]
public class StateGroundCheckComponent : StateComponent
{
    [SerializeField, Tooltip("Should the check begin higher? This improve slope detection and climb when falling")] protected float heightOffset;
    [SerializeField, Tooltip("How down the check should run? This impreve sticking to slopes when going down one")] protected float heightCheckBuffer;
   [SerializeField, Tooltip("How big is the check for the ground? This improve step detection and when falling can help the player standing on the platform even if is not actually on the platform, a value too high can mess with slope detection")] protected float wideCheckBuffer;

    public bool IsGrounded { get; private set; } = false;
    public RaycastHit RayHit { get; private set; } = new RaycastHit();

    
    public float HeightOffset => heightOffset;
    public float HeightCheckBuffer => heightCheckBuffer;
    public float WideCheckBuffer => wideCheckBuffer;
   

    /// <summary>
    /// Checks the ground returning info on the ground (if hitted)
    /// </summary>
    /// <returns></returns>
    public bool CheckGround(StateMachine stateMachine, out RaycastHit rayHit)
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
    public bool CheckGround(StateMachine stateMachine)
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