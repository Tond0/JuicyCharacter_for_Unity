using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;


[Serializable]
public class Crouch : Grounded, IMoveableState, ILookableState
{
    [Header("Components")]
    [SerializeField] private StateMovementComponent movementComponent;
    public StateMovementComponent MovementComponent => movementComponent;

    [SerializeField] private StateLookComponent lookComponent;
    public StateLookComponent LookComponent => lookComponent;

    [Header("Top head detection")]
    [SerializeField, Tooltip("This transform will be use to check if the player can stand, so place in on the head of the player!")] private Transform topHead;
    [SerializeField, Tooltip("How big is going to be the boxcast for the detection?")] private float topWideCheck = 1;
    [SerializeField, Tooltip("How far is going to be the boxcast for the detection?")] private float topHeightCheck = 0.4f;

    public override void Enter()
    {
        base.Enter();

        movementComponent.BindInput();
        lookComponent.BindInput();

        //Events (they all go to stand lol)
        InputManager.OnCrouchFired += Handle_CrouchFired;
        InputManager.OnCrouchReleased += Handle_CrouchReleased;
        InputManager.OnJumpFired += Handle_JumpFired;
    }

    private void Handle_CrouchFired() => TryStand();
    private void Handle_CrouchReleased() => TryStand();
    private void Handle_JumpFired() => TryStand();

    private void TryStand()
    {
        //If we can stand...
        if (!CheckTop()) return;
        //We stand!
        nextState = stateMachine.State_Stand;
    }

    public override void FixedRun()
    {
        base.FixedRun();

        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(movementComponent.MovementDirection, Camera.main.transform);
        movementComponent.Move(stateMachine, rb, movementDirection);

        lookComponent.Look(stateMachine, rb);
    }

    /// <summary>
    /// Perform a boxcast to check if something is on top of the topHead transform
    /// </summary>
    /// <returns></returns>
    private bool CheckTop()
    {
        Vector3 origin = topHead.position;
        Vector3 halfExtends = new(topWideCheck / 2, 0.01f, topWideCheck / 2);

        return !Physics.BoxCast(origin, halfExtends, Vector3.up, Quaternion.identity, topHeightCheck);
    }

    public override void Exit()
    {
        //Unbind inputs
        movementComponent.UnbindInput();
        lookComponent.UnbindInput();

        InputManager.OnCrouchFired -= Handle_CrouchFired;
        InputManager.OnCrouchReleased -= Handle_CrouchReleased;
        InputManager.OnJumpFired -= Handle_JumpFired;
    }
}
