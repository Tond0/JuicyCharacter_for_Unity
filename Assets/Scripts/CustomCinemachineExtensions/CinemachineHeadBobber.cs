using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cinemachine extension to let the camera simulate the head bob when walking
/// </summary>
public class CinemachineHeadBobber : CinemachinePlayerExtension
{
    //The noise cinemachine component we want to edit
    private CinemachineBasicMultiChannelPerlin noiseVCam;

    //The noise preset we want to apply
    [SerializeField] private NoiseSettings customNoise;

    protected override void OnEnable()
    {
        base.OnEnable();

        //Events!
        //The event that decide, based on the new state that just changed, the frequency of the noise movement
        StateComponent.OnStateChange += Handle_NoiseFrequency;

        //The event that decide, based on the input direction, if we're moving
        InputManager.OnMoveFired += Handle_Noise;
    }

    private void Start()
    {
        //Let's get the cinemachine component for the noise
        CinemachineVirtualCamera cinemachineVirtualCamera = VirtualCamera as CinemachineVirtualCamera;
        noiseVCam = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    /// <summary>
    /// Change the frequency of the noise based on what the new Ground state tell us.
    /// </summary>
    /// <param name="state1"></param>
    /// <param name="state2"></param>
    private void Handle_NoiseFrequency(PlayerState state1, PlayerState state2)
    {
        //If the state is not grounded we don't want the head bob!
        if (state2 is not Grounded)
        {
            //let's cancel the head bob by setting the frequency to 0
            noiseVCam.m_FrequencyGain = 0;
            return;
        }
        
        //let's get what the frequency is for this grounded state
        Grounded groundedState = state2 as Grounded;
        noiseVCam.m_FrequencyGain = groundedState.HeadBobbingFrequency;
    }

    /// <summary>
    /// Enable or disable the noise based on if the player's moving
    /// </summary>
    /// <param name="direction"></param>
    private void Handle_Noise(Vector2 direction)
    {
        //If is not moving (we're also checking the y but who care cause we're checking if we are grounded also!) or is not grounded...
        if (direction.magnitude <= 0)
        {
            //No noise please!
            noiseVCam.m_NoiseProfile = null;
            return;
        }

        //Apply the custom noise preset
        noiseVCam.m_NoiseProfile = customNoise;
    }

    //None is happening here
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) { }
}
