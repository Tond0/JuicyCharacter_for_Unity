using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//FIXME: Integrare con CinemachineDutchLerp
public class CinemachineHeadBobber : CinemachineExtension
{
    private CinemachineBasicMultiChannelPerlin noiseVCam;
    [SerializeField] private NoiseSettings customNoise;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private StateComponent stateComponent;

    private Vector2 direction;
    protected override void OnEnable()
    {
        base.OnEnable();

        CinemachineVirtualCamera cinemachineVirtualCamera = VirtualCamera as CinemachineVirtualCamera;
        noiseVCam = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        StateComponent.OnStateChange += ChangeNoiseFrequency;
        InputManager.OnMoveFired += TryNoise;
    }


    private void ChangeNoiseFrequency(PlayerState state1, PlayerState state2)
    {
        if (state2 is not Grounded)
        {
            noiseVCam.m_FrequencyGain = 0;
            return;
        }
        
        Grounded groundedState = state2 as Grounded;
        noiseVCam.m_FrequencyGain = groundedState.HeadBobbingFrequency;
    }

    private void TryNoise(Vector2 direction)
    {
        this.direction = direction;

        var currentState = stateComponent.CurrentState;

        //If is not moving (we're also checking the y but who care cause we're checking if we are grounded also!) or is not grounded...
        if (direction.magnitude <= 0)
        {
            noiseVCam.m_NoiseProfile = null;
            return;
        }

        noiseVCam.m_NoiseProfile = customNoise;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        return;
    }

    /*
     * var currentState = stateComponent.CurrentState;

        //If is not moving on the x or z axis...
        if ((rb.velocity.x == 0 && rb.velocity.z == 0)
            || currentState is not Grounded)
        {
            noiseVCam.m_NoiseProfile = null;
            return;
        }

        //If is not grounded
        Grounded currentGroundState = (Grounded)currentState;

        noiseVCam.m_NoiseProfile = customNoise;
     * */
}
