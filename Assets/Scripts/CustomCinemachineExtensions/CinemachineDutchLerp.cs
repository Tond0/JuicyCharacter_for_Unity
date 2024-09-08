using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineDutchLerp : CinemachineExtension
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private StateComponent stateComponent;
    [SerializeField] private float maxDutch;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        float playerMaxSpeed = GetCurrentMaxSpeed();
        if (playerMaxSpeed == 0) return;


        float currentDutch = state.Lens.Dutch;

        float currentHorizontalSpeed = stateComponent.transform.InverseTransformDirection(rb.velocity).x;
        float targetDutch = maxDutch * Mathf.Sign(-currentHorizontalSpeed);

        float normalizedSpeed = Mathf.Abs(currentHorizontalSpeed) / playerMaxSpeed;
        float maxDelta = normalizedSpeed * Mathf.Abs(targetDutch - currentDutch);

        currentDutch = Mathf.MoveTowards(currentDutch, targetDutch, maxDelta);

        state.Lens.Dutch = currentDutch;
    }

    private float GetCurrentMaxSpeed()
    {
        var currentState = stateComponent.CurrentState;
        if (currentState is not Controllable) return 0;

        Controllable currentControllableState = (Controllable)currentState;
        return currentControllableState.Stats_Movement.MaxSpeed;
    }
}
