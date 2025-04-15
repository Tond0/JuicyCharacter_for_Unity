using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallRunning : Air
{
    [Header("Wallrun detection")]
    [SerializeField, Tooltip("Starting from the player transform, what's the min height from the ground to perform a wallrun?")] private float minHeight = .7f;
    public float Wr_minHeight => minHeight;
    [SerializeField, Tooltip("Starting from the transform of the player (+ height), how far are we checking for a wall to wallrun?")] private float detectionDistance = 1;
    public float Wr_detectionDistance => detectionDistance;
    [SerializeField, Tooltip("How wide is the size of the box checking for a wall to wallrun with?")] private float detectionSize = 0.5f;
    public float Wr_detectionSize => detectionSize;
    [SerializeField, Tooltip("How wide is the size of the box checking for a wall to wallrun with?")] private float jumpForce = 20;

    [Header("Wallrunning settings")]
    [SerializeField] private float wallrunning_GravityMultiplaier;

    //Variables
    RaycastHit wallHit;
    public override void Enter()
    {
        base.Enter();

        //Inputs
        InputManager.OnMoveFired += TryStopWallrun;
        InputManager.OnJumpFired += JumpOffWall;

        gravityMultiplaier = wallrunning_GravityMultiplaier;
        
        //Cancel up momentum
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        //FIXME: Really ugly
        overrideMovement = true;
    }

    private void JumpOffWall()
    {
        rb.velocity += (stateComponent.transform.forward + stateComponent.transform.up).normalized * jumpForce;
        nextState = stateComponent.State_Jump;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        if(!CheckWallRun())
        {
            nextState = stateComponent.State_Falling;
            return;
        }

        Vector3 wallNormal = wallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, stateComponent.transform.up);

        if((stateComponent.transform.forward - wallForward).magnitude > (stateComponent.transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        Move(stateComponent, wallForward);
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnMoveFired -= TryStopWallrun;
        InputManager.OnJumpFired -= JumpOffWall;
    }

    public bool CheckWallRun()
    {
        //Ground min height detection.
        //Check if we are height enought
        GetGroundCheckDetectionInfo(out Vector3 origin, out Vector3 halfExtends);
        //If we spot the ground, then we're too close to it and we can't wallrun.
        if(Physics.BoxCast(origin, halfExtends, -stateComponent.transform.up, Quaternion.identity, minHeight)) return false;

        bool isLeftRunnable = CheckWallRunnableWall(-stateComponent.transform.right, out RaycastHit leftHit);
        bool isRightRunnable = CheckWallRunnableWall(stateComponent.transform.right, out RaycastHit rightHit);

        if (isLeftRunnable && isRightRunnable)
            // Get closer wall
            wallHit = leftHit.distance > rightHit.distance ? rightHit : leftHit;
        else if (isLeftRunnable)
            wallHit = leftHit;
        else if (isRightRunnable)
            wallHit = rightHit;
        
        return isLeftRunnable || isRightRunnable;
    }

    private bool CheckWallRunnableWall(Vector3 direction, out RaycastHit hitInfo)
    {
        Vector3 origin = stateComponent.transform.position + Vector3.up * stats_GroundCheck.height;
        Vector3 halfExtends = new(0.01f, detectionSize / 2, detectionSize / 2);

        return Physics.BoxCast(origin, halfExtends, direction, out hitInfo, Quaternion.identity, detectionDistance);
    }
    
    private void TryStopWallrun(Vector2 inputDirection)
    {
        if(inputDirection.y > 0) return;

        nextState = stateComponent.State_Falling;
    }
}