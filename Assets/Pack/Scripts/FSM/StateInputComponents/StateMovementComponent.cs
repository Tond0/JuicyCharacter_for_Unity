using System;
using Unity.VisualScripting;
using UnityEngine;

public class StateMovementComponent : StateInputComponent
{
    [SerializeField] public float maxSpeed;
    [SerializeField] public float maxAcceleration;
    [SerializeField] public float maxDeceleration;
    [SerializeField, Tooltip("The curve which decides how much boost to the acceleration we need to apply relate to the direction we want to move to and we're actually moving to. (t = 0 => we want to move in the opposite direction, t = 1 => we're moving in the same direction")] public AnimationCurve accelerationFactor;

    //The current direction we want to move to.
    protected Vector2 movementDirection;
    public Vector2 MovementDirection => movementDirection;
    public override void BindInput()
    {
        //Anytime we press the movements keys we assign it the direction
        InputManager.OnMoveFired += SetInputDirection;
    }
    public override void UnbindInput()
    {
        InputManager.OnMoveFired -= SetInputDirection;
    }

    public void Move(Vector3 direction)
    {
        //Direction relative to the camera
        Vector3 cameraRelativeDirection = GetCameraRelativeDirection(movementDirection, Camera.main.transform);

        //The current velocity
        Vector3 currentVelocity = rb.velocity;

        //Desire velocity relative to the camera
        Vector3 desireVelocity = new Vector3(cameraRelativeDirection.x, 0, cameraRelativeDirection.z) * maxSpeed;

        float maxStepAcceleration;
        //If we're moving
        if (movementDirection != Vector2.zero)
        {
            //Get the dot product of where we want to go and where we are actually going.
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            //Use the velocityDot to know how much we need to boost acceleration to instantly (or almost) go to the opposide direction without sliding
            maxStepAcceleration = maxAcceleration * accelerationFactor.Evaluate(velDot);
        }
        //If we're not moving we just use the deceleration variable
        else
        {
            maxStepAcceleration = maxDeceleration;
        }

        //The max acceleration that can be handle this frame
        float maxSpeedChange = maxStepAcceleration * Time.deltaTime /** acceleration_Multiplaier*/;

        //Update the velocity
        Vector3 finalVelocity = Vector3.MoveTowards(currentVelocity, desireVelocity, maxSpeedChange);

        //We don't want to edit the y value of the velocity
        finalVelocity.y = rb.velocity.y;

        //Apply new velocity
        rb.velocity = finalVelocity;

        //DEBUG
        //The visual rapresentation of where we'd like to go
        Debug.DrawRay(stateMachine.transform.position, desireVelocity, Color.red);
        //The visual rapresentation of where we're actually moving to
        Debug.DrawRay(stateMachine.transform.position, finalVelocity, Color.green);
    }


    /// <summary>
    /// Pretty much what the name says
    /// </summary>
    /// <param name="inputDirection"></param>
    private void SetInputDirection(Vector2 inputDirection) => movementDirection = inputDirection;

    /// <summary>
    /// Calculate the relative camera direction, (I know transform.InverseTransformDirection does exist, I'm just testing myself here)
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="relativeTransform"></param>
    /// <returns></returns>
    protected Vector3 GetCameraRelativeDirection(Vector2 direction, Transform relativeTransform)
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
}