using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any state that inherit from this will be a controllable state with the capability to move
/// </summary>
public abstract class Controllable : PlayerState
{
    //Let's make space in the inspector
    [Space(15)]

    //The movements variables
    [SerializeField] protected MovementStats stats_Movement;
    //We share a COPY, so we don't care if some1 change the variables in the struct outside this class, because the behaviour of this class won't change.
    public MovementStats Stats_Movement => stats_Movement;
    //The settings to check wherever there's a ground under the player
    [SerializeField] protected GroundCheck_Stats stats_GroundCheck;
    //We share a COPY, so we don't care if some1 change the variables in the struct outside this class, because the behaviour of this class won't change.
    public GroundCheck_Stats Stats_GroundCheck => stats_GroundCheck;

    //Where are we moving to? this is also stored in the inputmanager but I thought thay having a dedicated variable could cause less confusion
    protected Vector2 inputDirection;

    //This is needed for the jump and could be prove useful to other effect that might be implemented without touching the MovementStats class
    protected float acceleration_Multiplaier = 1;

    //We need it for moving and other things (like floating, jumping, sliding
    [SerializeField] protected Rigidbody rb;


    public override void Enter()
    {
        base.Enter();

        //Anytime we press the movements keys we assign it the direction
        InputManager.OnMoveFired += AssignDirection;

        //Anytime we transition the direction will be transitioning as well, so that we don't have to repeat the movement input to move the character again
        inputDirection = InputManager.current.Direction;
    }

    public override void Exit()
    {
        InputManager.OnMoveFired -= AssignDirection;
    }

    /// <summary>
    /// Pretty much what the name says
    /// </summary>
    /// <param name="direction"></param>
    private void AssignDirection(Vector2 direction) => this.inputDirection = direction;

    public override void FixedRun()
    {
        //Direction relative to the camera
        Vector3 cameraRelativeDirection = RelateTo(inputDirection, Camera.main.transform);
        Move(stateComponent, cameraRelativeDirection);
    }

    public override PlayerState Run()
    {
        //Handle the look inputs
        Look(stateComponent);

        return base.Run();
    }

    #region GroundCheck method variants

    /// <summary>
    /// Checks the ground returning info on the ground (if hitted)
    /// </summary>
    /// <returns></returns>
    protected bool CheckGround(out RaycastHit rayHit)
    {
        GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends);

        if (Physics.BoxCast(origin, halfExtends, -stateComponent.transform.up, out rayHit, Quaternion.identity, stats_GroundCheck.heightCheckBuffer))
            return true;

        return false;
    }

    /// <summary>
    /// Checks the ground without returning any info on the ground
    /// </summary>
    /// <returns></returns>
    protected bool CheckGround()
    {
        GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends);

        if (Physics.BoxCast(origin, halfExtends, -stateComponent.transform.up, Quaternion.identity, stats_GroundCheck.heightCheckBuffer))
            return true;

        return false;
    }

    /// <summary>
    /// Return everything needed for a boxcast to check the ground
    /// </summary>
    /// <param name="springDir"></param>
    /// <param name="origin"></param>
    /// <param name="halfExtends"></param>
    protected void GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends)
    {
        //Origin of the ground check depends on the heightOffset we give to it
        origin = stateComponent.transform.position + (Stats_GroundCheck.heightOffset * Vector3.up);

        //Half size
        halfExtends = new(stats_GroundCheck.wideCheckBuffer / 2, 0.01f, stats_GroundCheck.wideCheckBuffer / 2);

        /* DEPRECATED 5 Raycast method
        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        */

        /* DEPRECATED Spherecast method
        Ray ray = new (origin, -springDir);
        if (Physics.SphereCast(ray, stats_GroundCheck.WideCheckBuffer, out rayHit, Stats_GroundCheck.HeightCheckBuffer))
            return true;
        */
    }

    #endregion

    #region Move

    /// <summary>
    /// Moves the player with acceleration / deceleration to the desire direction
    /// </summary>
    /// <param name="stateComponent"></param>
    protected virtual void Move(StateComponent stateComponent, Vector3 direction)
    {
        //The current velocity
        Vector3 currentVelocity = rb.velocity;

        //Desire velocity relative to the camera
        Vector3 desireVelocity = new Vector3(direction.x, 0, direction.z) * Stats_Movement.maxSpeed;

        float maxAcceleration;
        //If we're moving
        if (inputDirection != Vector2.zero)
        {
            //Get the dot product of where we want to go and where we are actually going.
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            //Use the velocityDot to know how much we need to boost acceleration to instantly (or almost) go to the opposide direction without sliding
            maxAcceleration = Stats_Movement.maxAcceleration * Stats_Movement.accelerationFactor.Evaluate(velDot);
        }
        //If we're not moving we just use the deceleration variable
        else
        {
            maxAcceleration = Stats_Movement.maxDeceleration;
        }

        //The max acceleration that can be handle this frame
        float maxSpeedChange = maxAcceleration * Time.deltaTime * acceleration_Multiplaier;

        //Update the velocity
        Vector3 finalVelocity = Vector3.MoveTowards(currentVelocity, desireVelocity, maxSpeedChange);

        //We don't want to edit the y value of the velocity
        finalVelocity.y = rb.velocity.y;

        //Apply new velocity
        rb.velocity = finalVelocity;

        //DEBUG
        //The visual rapresentation of where we'd like to go
        Debug.DrawRay(stateComponent.transform.position, desireVelocity, Color.red);
        //The visual rapresentation of where we're actually moving to
        Debug.DrawRay(stateComponent.transform.position, finalVelocity, Color.green);
    }

    /// <summary>
    /// Calculate the relative camera direction, (I know transform.InverseTransformDirection does exist, I'm just testing myself here)
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="relativeTransform"></param>
    /// <returns></returns>
    protected Vector3 RelateTo(Vector2 direction, Transform relativeTransform)
    {
        Vector3 camForward = relativeTransform.forward;
        Vector3 camRight = relativeTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 cameraRelativeForward = direction.y * camForward;
        Vector3 cameraRelativeRight = direction.x * camRight;

        Vector3 cameraRelativeMovement = cameraRelativeForward + cameraRelativeRight;
        return cameraRelativeMovement;
    }
    #endregion

    /// <summary>
    /// Rotate the player accordingly to the camera rotation
    /// </summary>
    /// <param name="stateComponent"></param>
    private void Look(StateComponent stateComponent)
    {
        Quaternion playerRot = stateComponent.transform.localRotation;
        playerRot.y = Camera.main.transform.localRotation.y;
        playerRot.Normalize();
        rb.rotation = playerRot;
    }

    #region Class Struct
    [Serializable]
    public struct MovementStats
    {
        [SerializeField] public float maxSpeed;
        [SerializeField] public float maxAcceleration;
        [SerializeField] public float maxDeceleration;
        [SerializeField, Tooltip("The curve which decides how much boost to the acceleration we need to apply relate to the direction we want to move to and we're actually moving to. (t = 0 => we want to move in the opposite direction, t = 1 => we're moving in the same direction")] public AnimationCurve accelerationFactor;
    }

    [Serializable]
    public struct GroundCheck_Stats
    {
        [SerializeField, Tooltip("The distance we want between the player and the ground. WARNING: This should be tuned with the heightCheckBuffer and the heightOffset, cause they both influence the distance between the player and the ground")] public float height;
        [SerializeField, Tooltip("Should the check begin higher? This improve slope detection and climb when falling")] public float heightOffset;
        [SerializeField, Tooltip("How down the check should run? This impreve sticking to slopes when going down one")] public float heightCheckBuffer;
        [SerializeField, Tooltip("How big is the check for the ground? This improve step detection and when falling can help the player standing on the platform even if is not actually on the platform, a value too high can mess with slope detection")] public float wideCheckBuffer;
        [SerializeField, Tooltip("How fast should we reach the desire height?")] public float springStrength;
        [SerializeField, Tooltip("How much should we damp before reaching the desire height?")] public float dampingForce;

        //FIXME: We dont need getters beacause we'll only share a copy of this struct with other classes, even if they edit variables the behaviour in this class won't change.
        // //Getters
        // public float Height { get => height; }
        // public float HeightOffset { get => heightOffset; }
        // public float HeightCheckBuffer { get => heightCheckBuffer; }
        // public float WideCheckBuffer { get => wideCheckBuffer; }
        // public float SpringStrength { get => springStrength; }
        // public float DampingForce { get => dampingForce; }
    }
    #endregion
}
