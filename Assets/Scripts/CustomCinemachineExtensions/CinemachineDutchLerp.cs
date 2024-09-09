using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The cinemachine extension that will handle the camera dutch rotation relative to the X velocity of the player.
/// </summary>
public class CinemachineDutchLerp : CinemachinePlayerExtension
{
    [SerializeField, Tooltip("What's the max tilt the camera's going to have when reached max speed horizontally?")] private float maxDutch;

    
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        //Let's get the max speed
        float playerMaxSpeed = GetCurrentMaxSpeed();
        //No maxspeed means no movement, which  means no tilting
        if (playerMaxSpeed == 0) return;

        //How tilted is the camera in this moment?
        float currentDutch = state.Lens.Dutch;

        //What's the current player horizontal velocity based on the camera direction?
        float currentHorizontalSpeed = Camera.main.transform.InverseTransformDirection(rb.velocity).x;

        //What's our target tilt for the camera? (left = -maxDutch, right = maxDutch)
        float targetDutch = maxDutch * Mathf.Sign(-currentHorizontalSpeed);

        //The speed normalized from 0 to 1
        float normalizedSpeed = Mathf.Abs(currentHorizontalSpeed) / playerMaxSpeed;
        //The max delta change we want to see in the transition
        float maxDelta = normalizedSpeed * Mathf.Abs(targetDutch - currentDutch);

        //Let's smoothly (based on how fast can the player reach max speed) tilt the camera
        currentDutch = Mathf.MoveTowards(currentDutch, targetDutch, maxDelta);

        //Let's apply the tilt
        state.Lens.Dutch = currentDutch;
    }

    /// <summary>
    /// Gets the current max speed of the player
    /// </summary>
    /// <returns></returns>
    private float GetCurrentMaxSpeed()
    {
        //If the current state is not controllable than player doesn't have a speed at all!
        var currentState = stateComponent.CurrentState;
        if (currentState is not Controllable) return 0;

        //Get the state max speed
        Controllable currentControllableState = (Controllable)currentState;
        return currentControllableState.Stats_Movement.MaxSpeed;
    }
}
