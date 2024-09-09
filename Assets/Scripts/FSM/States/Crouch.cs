using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;


[Serializable]
public class Crouch : Grounded
{
    [Header("Top head detection")]
    [SerializeField, Tooltip("This transform will be use to check if the player can stand, so place in on the head of the player!")] private Transform topHead;
    [SerializeField, Tooltip("How big is going to be the boxcast for the detection?")] private float topWideCheck = 1;
    [SerializeField, Tooltip("How far is going to be the boxcast for the detection?")] private float topHeightCheck = 0.4f;
    public override void Enter()
    {
        base.Enter();

        //Events (they all go to stand)
        InputManager.OnCrouchFired += Handle_CrochFireNdReleased;
        InputManager.OnCrouchReleased += Handle_CrochFireNdReleased;
        InputManager.OnJumpFired += Handle_CrochFireNdReleased;
    }

    private void Handle_CrochFireNdReleased()
    {
        //If we can stand...
        if (!CheckTop()) return;
        //We stand!
        nextState = stateComponent.State_Stand;
    }

    /// <summary>
    /// Perform a boxcast to check if something is on top of the topHead transform
    /// </summary>
    /// <returns></returns>
    private bool CheckTop()
    {
        Vector3 origin = topHead.position;
        Vector3 halfExtends = new(1 / 2, 0.01f, 1 / 2);

        return !Physics.BoxCast(origin, halfExtends, Vector3.up, Quaternion.identity, topHeightCheck);
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnCrouchFired -= Handle_CrochFireNdReleased;
        InputManager.OnCrouchReleased -= Handle_CrochFireNdReleased;
        InputManager.OnJumpFired -= Handle_CrochFireNdReleased;
    }
}
