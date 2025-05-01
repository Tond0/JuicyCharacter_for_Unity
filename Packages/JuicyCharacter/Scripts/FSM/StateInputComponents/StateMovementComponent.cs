using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class StateMovementComponent : StateInputComponent
{
    [SerializeField, Tooltip("If this is set to 0, it will inherit the maxSpeed from the previous State")] public float maxSpeed;
    [SerializeField] public float maxAcceleration;
    [SerializeField] public float maxDeceleration;
    [SerializeField, Tooltip("The curve which decides how much boost to the acceleration we need to apply relate to the direction we want to move to and we're actually moving to. (t = 0 => we want to move in the opposite direction, t = 1 => we're moving in the same direction")] public AnimationCurve accelerationFactor;
    [SerializeField, Tooltip("Max camera angle when turning left and right")] public float maxCameraDutch = 1;

    [NonSerialized] public float accelerationMultiplier = 1;

    //The current direction we want to move to.
    protected Vector2 movementDirection;
    public Vector2 MovementDirection => movementDirection;
    public override void BindInput()
    {
        //Anytime we press the movements keys we assign it the direction
        InputManager.OnMoveFired += SetInputDirection;
        movementDirection = InputManager.current.MovingDirection;
    }
    public override void UnbindInput()
    {
        InputManager.OnMoveFired -= SetInputDirection;
    }

    public void Move(StateMachine stateMachine, Rigidbody rb, Vector3 direction)
    {
        //FIXME: This should not be updated every frame, but only when the state changes.
        //If the direction is 0, then we just set it to the previous state MaxSpeed
        if(maxSpeed == 0)
            maxSpeed = GetPreviousMaxSpeed(stateMachine);

        //The current velocity
        Vector3 currentVelocity = rb.velocity;

        //Desire velocity relative to the camera
        Vector3 desireVelocity = new Vector3(direction.x, 0, direction.z) * maxSpeed;

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
        float maxSpeedChange = maxStepAcceleration * Time.deltaTime * accelerationMultiplier;

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
    /// /// Get the maxSpeed of the previous state, if it's 0 then we just return the maxSpeed of the current state.
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <returns></returns>
    private float GetPreviousMaxSpeed(StateMachine stateMachine)
    {
        for(int i = stateMachine.StateQueue.Count - 2; i >= 0; i--)
        {
            if(stateMachine.StateQueue.ElementAt(i) is IMoveableState moveableState && moveableState.MovementComponent.maxSpeed != 0)
                return moveableState.MovementComponent.maxSpeed;
        }

        Debug.LogError("There's no state with maxSpeed in the queue, please increase maxQueueCount in StateMachine.cs!");
        return 0;
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
    /// <param name="cameraTransform"></param>
    /// <returns></returns>
    public Vector3 GetCameraRelativeDirection(Vector2 direction, Transform cameraTransform)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

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