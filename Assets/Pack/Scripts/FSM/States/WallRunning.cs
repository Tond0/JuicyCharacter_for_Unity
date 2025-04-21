using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class WallRunning : Controllable
{
    [Header("Wallrun detection")]
    [SerializeField, Tooltip("Starting from the player transform, what's the min height from the ground to perform a wallrun?")] private float minHeight = .7f;
    public float MinHeight => minHeight;
    [SerializeField, Tooltip("Starting from the transform of the player (+ height), how far are we checking for a wall to wallrun?")] private float detectionDistance = 1;
    public float DetectionDistance => detectionDistance;
    [SerializeField, Tooltip("How wide is the size of the box checking for a wall to wallrun with?")] private float detectionSize = 0.5f;
    public float DetectionSize => detectionSize;
    [SerializeField, Tooltip("How wide is the size of the box checking for a wall to wallrun with?")] private float jumpForce = 20;

    [Header("Wallrunning settings")]
    [SerializeField, Tooltip("Default gravity force is -9.81f")] private float gravityForce = -9.81f;
    [SerializeField] private float wallrunning_GravityMultiplaier;

    [Space(15)]
    [SerializeField, Tooltip("How much should the head move up and down?"), Range(0, 5)] private float headBobbingFrequency = 1.2f;
    //Getter so that CinemachineHeadbobber.cs can adjust the frequency of the noise effect
    public float HeadBobbingFrequency { get => headBobbingFrequency; }

    //Variables

    /// <summary>
    /// Used to get the direction of the force, and to understand if the player is trying to wallrun on the same wall twice in a row.
    /// </summary>
    private RaycastHit wallHit;
    /// <summary>
    /// In which direction do we need to move?
    /// </summary>
    private Vector3 wallForward;
    /// <summary>
    /// We're sure that the player has touched the ground, and can wallrun on the same previous wall.
    /// </summary>
    private bool canWallRunSameWall;
    public override void Enter()
    {
        base.Enter();

        //Inputs
        InputManager.OnMoveFired += TryStopWallrun;
        InputManager.OnJumpFired += JumpOffWall;
        
        //Cancel up momentum
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }

    private void JumpOffWall()
    {
        rb.velocity += (Camera.main.transform.forward + stateComponent.transform.up).normalized * jumpForce;
        nextState = stateComponent.State_Jump;
    }

    public override void FixedRun()
    {
        Look(stateComponent);

        if(!CheckWallRun())
        {
            nextState = stateComponent.State_Falling;
            return;
        }

        //Custom Gravity
        //FIXME: This need to be moved
        rb.AddForce(gravityForce * wallrunning_GravityMultiplaier * Vector3.up, ForceMode.Acceleration);

        //Get the normal of the wall
        Vector3 wallNormal = wallHit.normal;
        //Get the direction we should wallrun with.
        wallForward = Vector3.Cross(wallHit.normal, stateComponent.transform.up);

        if(Vector3.Dot(wallForward, stateComponent.transform.forward) < 0)
            wallForward = -wallForward;

        Move(stateComponent, wallForward);

        //Stick to wall
        rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnMoveFired -= TryStopWallrun;
        InputManager.OnJumpFired -= JumpOffWall;

        canWallRunSameWall = false;
    }

    /// <summary>
    /// Check if we can begin to wallrun.
    /// </summary>
    /// <returns></returns>
    public bool CheckWallRunInitializer()
    {
        //Ground min height detection.
        //Check if we are height enought
        GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends);
        //If we spot the ground, then we're too close to it and we can't wallrun.
        //if(Physics.BoxCast(origin, halfExtends, -stateComponent.transform.up, Quaternion.identity, minHeight)) return false;
        if(CheckGround())
        {
            return false;
        }

        bool isLeftRunnable = CheckWallRunnableWall(-stateComponent.transform.right, out RaycastHit leftHit);
        bool isRightRunnable = CheckWallRunnableWall(stateComponent.transform.right, out RaycastHit rightHit);

        RaycastHit currentWallHit = new();

        if (isLeftRunnable && isRightRunnable)
            // Get closer wall
            currentWallHit = leftHit.distance > rightHit.distance ? rightHit : leftHit;
        else if (isLeftRunnable)
            currentWallHit = leftHit;
        else if (isRightRunnable)
            currentWallHit = rightHit;
        
        //Can't run on the same wall twice!
        if(currentWallHit.colliderInstanceID == wallHit.colliderInstanceID 
            && !canWallRunSameWall)
                return false;   

        wallHit = currentWallHit;

        return isLeftRunnable || isRightRunnable;
    }

    /// <summary>
    /// Check if we're still running on the same wall
    /// </summary>
    /// <returns></returns>
    private bool CheckWallRun()
    {
        //Ground min height detection.
        //Check if we are height enought
        GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends);
        //If we spot the ground, then we're too close to it and we can't wallrun.
        //if(Physics.BoxCast(origin, halfExtends, -stateComponent.transform.up, Quaternion.identity, minHeight)) return false;
        if(CheckGround())
        {
            return false;
        }

        //If we're looking the opposite direction we stop the wallrun.
        if(Vector3.Dot(rb.velocity.normalized, stateComponent.transform.forward.normalized) < 0)
            return false;

        bool isLeftRunnable = CheckWallRunnableWall(-stateComponent.transform.right, out RaycastHit leftHit);
        bool isRightRunnable = CheckWallRunnableWall(stateComponent.transform.right, out RaycastHit rightHit);

        RaycastHit currentWallHit = new();

        if (isLeftRunnable && isRightRunnable)
            // Get closer wall
            currentWallHit = leftHit.distance > rightHit.distance ? rightHit : leftHit;
        else if (isLeftRunnable)
            currentWallHit = leftHit;
        else if (isRightRunnable)
            currentWallHit = rightHit;
        
        //Is it the same wall?
        if(currentWallHit.colliderInstanceID == wallHit.colliderInstanceID)
        {
            wallHit = currentWallHit;
            return true;
        }
        else
            return false;
    }

    //protected override void Look(StateComponent stateComponent) { return; }

    /// <summary>
    /// Called when the state grounded Enter().
    /// Then, the player, will be able to wallRun again on the same wall.
    /// </summary>
    public void OnGroundTouched() => canWallRunSameWall = true;

    private bool CheckWallRunnableWall(Vector3 direction, out RaycastHit hitInfo)
    {
        Vector3 origin = stateComponent.transform.position + Vector3.up * stats_GroundCheck.height;
        Vector3 halfExtends = new(0.01f, detectionSize / 2, detectionSize / 2);

        if(Physics.BoxCast(origin, halfExtends, direction, out hitInfo, Quaternion.identity, detectionDistance))
            Debug.DrawRay(origin, direction * detectionDistance, Color.cyan);

        return Physics.BoxCast(origin, halfExtends, direction, out hitInfo, Quaternion.identity, detectionDistance);
    }
    
    private void TryStopWallrun(Vector2 inputDirection)
    {
        if(inputDirection.y > 0) return;

        nextState = stateComponent.State_Falling;
    }

    /// <summary>
    /// In which direction do we need to dutch the camera? 
    /// Used by the DutchCinemachineExtension
    /// </summary>
    /// <returns></returns>
    public float GetDutchWallrunningDirection()
    {
        bool isLeftRunnable = CheckWallRunnableWall(-stateComponent.transform.right, out RaycastHit leftHit);

        return leftHit.colliderInstanceID == wallHit.colliderInstanceID ? -1 : 1;
    }
}