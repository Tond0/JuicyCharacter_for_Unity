using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The cinemachine extension that will handle the camera dutch rotation relative to the X velocity of the player.
/// </summary>
public class CinemachineDutchLerp : CinemachinePlayerExtension
{
    private float targetDutch;
    private float stateMaxSpeed;
    private float stateCurrentSpeed;
    protected override void OnEnable()
    {
        base.OnEnable();

        StateMachine.OnStateChange += Handle_StateDutch;
    }

    void OnDisable()
    {
        StateMachine.OnStateChange -= Handle_StateDutch;
    }

    private void Handle_StateDutch(PlayerState newState, PlayerState oldState)
    {
        if (newState is not IMoveableState moveableState) return;

        //Get the maxDutch we want to reach.
        float maxDutch = moveableState.MovementComponent.maxCameraDutch;

        //We don't want to lerp the dutch for the wallrunning state.
        if (moveableState is WallRunning) 
        {
            //FIXME: Forced to be applied each frame because we don't know the CameraState, Bummer.
            targetDutch = maxDutch;
            return;
        };

        //Let's get the max speed
        stateMaxSpeed = moveableState.MovementComponent.maxSpeed;

        //Target dutch
        targetDutch = maxDutch;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if(stateComponent.CurrentState is WallRunning WallRunningState)
        {
            state.Lens.Dutch = targetDutch * WallRunningState.GetDutchWallrunningDirection();
            return;
        }

        //What's our target tilt for the camera? (left = -maxDutch, right = maxDutch)
        targetDutch = Mathf.Abs(targetDutch) * Mathf.Sign(-stateCurrentSpeed);

        //What's the current player horizontal velocity based on the camera direction?
        stateCurrentSpeed = Camera.main.transform.InverseTransformDirection(rb.velocity).x;

        //The speed normalized from 0 to 1
        float normalizedSpeed = Mathf.Abs(stateCurrentSpeed) / stateMaxSpeed;

        //Let's apply the tilt
        state.Lens.Dutch = LerpDutch(state.Lens.Dutch, targetDutch, normalizedSpeed);
    }

    /// <summary>
    /// Lerp the camera to dutch in the desire direction.
    /// This is not ACTUALLY a Lerp but it does the job.
    /// </summary>
    /// <param name="startDutch"></param>
    /// <param name="targetDutch"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private float LerpDutch(float startDutch, float targetDutch, float t)
    {
        //The max delta change we want to see in the transition
        float maxDelta = t * Mathf.Abs(targetDutch - startDutch);

        //Let's smoothly (based on how fast can the player reach max speed) tilt the camera
        startDutch = Mathf.MoveTowards(startDutch, targetDutch, maxDelta);

        return startDutch;
    }
}
