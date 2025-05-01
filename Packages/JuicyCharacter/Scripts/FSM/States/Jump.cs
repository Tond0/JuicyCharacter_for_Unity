using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[Serializable]
public class Jump : Air, IMoveableState, ILookableState
{
    [Header("Components")]
    [SerializeField] protected StateMovementComponent movementComponent;
    public StateMovementComponent MovementComponent => movementComponent;
    [SerializeField] protected StateLookComponent lookComponent;
    public StateLookComponent LookComponent => lookComponent;

    [Space(10)]
    [SerializeField, Tooltip("The force the jump will have ONCE, not continuous")] private float jumpForce;
    public float JumpForce => jumpForce;
    [SerializeField, Tooltip("When the player is at the top of the jump do we boost the acceleration? This is useful so the player can decide where to land mid air easier")] private float airtTime_AccelMultiplier;
    [SerializeField, Tooltip("When we change from StandState to FallingState a timer will start and if we press jump before the timer ends, we jump even if we're not on the ground. This decides the timer lenght")] private float coyoteTime = 0.3f;
    [SerializeField, Tooltip("What's the max time that takes (without releasing jump) to reach the top height?")] private float maxJumpDuration;
    [SerializeField, Tooltip("What's the min time that we can jump (even if we release the jump button instantaneously)?")] private float minJumpDuration;
    
    [Space(5)]
    [SerializeField, Tooltip("What's the min Y velocity we need to enter the jump phase Top-Height? (Used also for deciding when to move from Top-Height to Descending phase), can roughly be seen as the duration of the Top-Height phase"), Range(0, 15)] private float topHeightThreshold = 5;

    [Space(10)]
    [SerializeField, Tooltip("Gravity when aiming for the top height")] private float gravityMultiplaier_Ascending;
    [SerializeField, Tooltip("Gravity when jump button is released. Higher value = more responsive cut off of the jump")] private float gravityMultiplaier_InputReleased;
    [SerializeField, Tooltip("Graivity when we reach the top of the jump")] private float gravityMultiplaier_TopHeight;
    [SerializeField, Tooltip("Gravity when going from the top of the jump to the ground again")] private float gravityMultiplaier_Descending;

    //The 3 possible jump state
    private enum JumpState { Ascending, Top, Descending  };
    private JumpState jumpState = JumpState.Ascending;

    //Do we want to keep jumping?
    private bool wantToJump;

    //Getter so the air state knows when it can and can't transition to jump state if button is pressed
    public float CoyoteTime { get => coyoteTime; }

    public override void Enter()
    {
        base.Enter();

        //We sure want to jump!
        wantToJump = true;

        //If we release we dont want anymore to jump
        InputManager.OnJumpReleasedRef.Delegate += Handle_JumpReleased;
        movementComponent.BindInput();
        lookComponent.BindInput();

        //We're trying to reach the top of the jump now
        jumpState = JumpState.Ascending;

        //Do not check the ground while we're ascending!
        checkGround = false;

        //First set of gravity
        gravityMultiplaier = gravityMultiplaier_Ascending;

        //Let's work with the velocity
        Vector3 app_velocity = rb.velocity;
        //Let's reset any Y movement
        app_velocity.y = 0;
        rb.velocity = app_velocity;

        //Let's Jump! Force up!
        app_velocity.x = 0;
        app_velocity.y = jumpForce;
        app_velocity.z = 0;

        //Apply the velocity
        rb.velocity += app_velocity;
    }

    private void Handle_JumpReleased()
    {
        wantToJump = false;
        InputManager.OnJumpReleasedRef.Delegate -= Handle_JumpReleased;
    }

    public override void Exit()
    {
        base.Exit();

        movementComponent.UnbindInput();
        lookComponent.UnbindInput();
        InputManager.OnJumpReleasedRef.Delegate -= Handle_JumpReleased;
    }

    public override PlayerState Run()
    {
        base.Run();

        //Check jump behaviour
        switch (jumpState)
        {
            case JumpState.Ascending:

                //If we released the jump button, is the min duration elapsed?
                //OR
                //If we didn't release the jump button yet is the max duration elapsed?
                if ((StateDuration >= minJumpDuration && !wantToJump)
                    || StateDuration >= maxJumpDuration)
                {
                    //Boost speed
                    movementComponent.accelerationMultiplier = airtTime_AccelMultiplier;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_InputReleased;
                }

                //If we're close to 0 with the velocity...
                if(rb.velocity.y < topHeightThreshold)
                {
                    //We're at the top of the jump
                    jumpState = JumpState.Top;

                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_TopHeight;
                    //Apply boost to the acceleration
                    movementComponent.accelerationMultiplier = airtTime_AccelMultiplier;

                    //We can start check the ground again
                    checkGround = true;
                }

                break;

            case JumpState.Top:

                //We're now descending?...
                if (rb.velocity.y < -topHeightThreshold)
                {
                    //We're trying to reach the ground again
                    jumpState = JumpState.Descending;

                    //Reset acceleration boost
                    movementComponent.accelerationMultiplier = 1;
                    //Apply new gravity
                    gravityMultiplaier = gravityMultiplaier_Descending;
                }

                break;

            //Nothing to do, just wait to fully descend and touch grass again
            case JumpState.Descending:
                break;
        }

        return nextState;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(movementComponent.MovementDirection, Camera.main.transform);
        movementComponent.Move(stateMachine, rb, movementDirection);

        lookComponent.Look(stateMachine, rb);
    }

    protected override Grounded GetLastGroundState()
    {
        Grounded nextGroundedState = base.GetLastGroundState();
        
        //Jumping cancel the slide! So we don't want to get back sliding once we touch the ground again.
        if(nextGroundedState is Slide)
            nextGroundedState = stateMachine.State_Sprint;

        return nextGroundedState;
    }
}
