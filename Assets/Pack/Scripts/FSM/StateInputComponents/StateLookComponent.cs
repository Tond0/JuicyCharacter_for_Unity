using System;
using UnityEngine;

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
    protected virtual void Look()
    {
        Quaternion playerRot = stateMachine.transform.localRotation;
        playerRot.y = Camera.main.transform.localRotation.y;
        playerRot.Normalize();
        rb.rotation = playerRot;
    }

    private void SetLookDirection(Vector2 inputDirection) => lookDirection = inputDirection;
}