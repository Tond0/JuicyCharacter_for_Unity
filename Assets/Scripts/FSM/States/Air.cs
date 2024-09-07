using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public abstract class Air : Controllable
{
    [Space(15)]
    [SerializeField] private float gravityForce = -9.81f;
    protected bool useGravity = true;
    protected bool checkGround = true;
    protected float gravityMultiplaier = 1;

    public override void FixedRun()
    {
        base.FixedRun();

        if (checkGround && CheckGround())
            nextState = stateComponent.State_Stand;

        CustomGravity();
    }

    private void CustomGravity()
    {
        if (!useGravity) return;

        rb.AddForce(gravityForce * gravityMultiplaier * Vector3.up, ForceMode.Acceleration);
    }
}
