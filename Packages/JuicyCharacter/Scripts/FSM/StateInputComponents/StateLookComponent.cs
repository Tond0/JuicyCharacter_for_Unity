using System;
using UnityEngine;

[Serializable]
public class StateLookComponent : StateInputComponent
{
    private Vector2 lookDirection;
    public Vector2 LookDirection => lookDirection;
    public override void BindInput()
    {
        InputManager.OnLookFired += SetLookDirection;
    }

    public override void UnbindInput()
    {
        InputManager.OnLookFired -= SetLookDirection;
    }

    /// <summary>
    /// Rotate the player accordingly to the camera rotation.
    /// </summary>
    public void Look(StateMachine stateMachine, Rigidbody rb)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Keep the rotation on the horizontal plane
        cameraForward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        stateMachine.transform.rotation = targetRotation;
    }

    private void SetLookDirection(Vector2 inputDirection) => lookDirection = inputDirection;
}