using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class Grounded : Controllable
{
    protected Grounded(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent, startDirection, movementStats)
    {
    }

    public override void FixedRun()
    {
        base.FixedRun();

        if (CheckGround(stats.transform, stats.HeightOffset, stats.WideCheckBuffer, stats.HeightCheckBuffer, out RaycastHit rayHit))
            Float(stats.transform, stats.Rb, rayHit, stats.Height, stats.DampingForce, stats.SpringStrength);
        else
            nextState = new Air(stateComponent, direction, stats.Movement_Air);
    }

    protected void Float(Transform transform, Rigidbody rb, RaycastHit rayHit, float height, float dampingForce, float springStrength)
    {
            //Per applicare la forza di una molla usiamo la formula springForce = offset - damping;
            //offset = differenza di distanza su quanto distante è dal terreno e quanto distante dovrebbe essere.
            float offset = height - rayHit.distance;
            //calcoliamo la velocity relativa alla direzione in cui il player deve applicare la forza della molla (verso giù) 
            float rayDirVel = Vector3.Dot(transform.up, rb.velocity);
            //damping = velocity * forzaDelDamping
            float damping = rayDirVel * -dampingForce;

            float springForce = offset * springStrength - damping;

            rb.AddForce(transform.up * springForce);
    }
}
