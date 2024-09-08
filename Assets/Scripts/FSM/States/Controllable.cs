using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : PlayerState
{

    [SerializeField] private PlayerState.MovementStats stats_Movement;
    public MovementStats Stats_Movement { get => stats_Movement; }

    protected Vector2 direction;
    protected float acceleration_Multiplaier = 1;

    [SerializeField] protected Rigidbody rb;

    //Inputs
    protected List<Action> actionToListen;


    public override void Enter()
    {
        base.Enter();

        InputManager.OnMoveFired += (Vector2 direction) => this.direction = direction;
        direction = InputManager.current.Direction;
    }

    public override void Exit()
    {
        base.Exit();

        InputManager.OnMoveFired -= (Vector2 direction) => this.direction = direction;
    }

    public override void FixedRun()
    {
        Move(stateComponent);
    }

    public override PlayerState Run()
    {
        Look(stateComponent);

        return nextState;
    }

    #region GroundCheck
    protected bool CheckGround(out RaycastHit rayHit)
    {
        //Siccome deve stare in piedi la forza andrà verso l'alto
        Vector3 springDir = stateComponent.transform.up;

        Vector3 origin = stateComponent.transform.position + (Stats_GroundCheck.HeightOffset * Vector3.up);

        //Cube version
        //Vector3 cubeCenter = new(origin.x, origin.y - groundCheck_stats.HeightCheckBuffer / 2, origin.z);
        //Vector3 size = new(wideCheckBuffer, heightCheckBuffer, wideCheckBuffer);
        //Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        /* 5 Raycast method
        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        */

        /* Sphere cast method
        Ray ray = new (origin, -springDir);
        if (Physics.SphereCast(ray, stats_GroundCheck.WideCheckBuffer, out rayHit, Stats_GroundCheck.HeightCheckBuffer))
            return true;
        */
        //Boxcast method
        Vector3 halfExtends = new(stats_GroundCheck.WideCheckBuffer / 2, 0.01f, stats_GroundCheck.WideCheckBuffer / 2);
        if(Physics.BoxCast(origin, halfExtends, -springDir, out rayHit, Quaternion.identity, stats_GroundCheck.HeightCheckBuffer))
            return true;

        //bool isGrounded = Physics.BoxCast(origin, size / 2, -springDir, out rayHit, Quaternion.identity, heightCheckBuffer);
        //bool isGrounded = Physics.OverlapBox(cubeCenter, size / 2, Quaternion.identity, groundMask, QueryTriggerInteraction.Ignore);

        return false;
    }

    protected bool CheckGround()
    {
        //Siccome deve stare in piedi la forza andrà verso l'alto
        Vector3 springDir = stateComponent.transform.up;

        Vector3 origin = stateComponent.transform.position + (Stats_GroundCheck.HeightOffset * Vector3.up);

        //Old version
        //Vector3 cubeCenter = new(origin.x, origin.y - groundCheck_stats.HeightCheckBuffer / 2, origin.z);
        //Vector3 size = new(wideCheckBuffer, heightCheckBuffer, wideCheckBuffer);
        //Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        /* 5 raycast method
        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out RaycastHit rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * Stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, Stats_GroundCheck.HeightCheckBuffer)) return true;
        */

        /* SphereCast method
        Ray ray = new(origin, -springDir);
        if (Physics.SphereCast(ray, stats_GroundCheck.WideCheckBuffer, Stats_GroundCheck.HeightCheckBuffer))
            return true;
        */

        //Boxcast method
        Vector3 halfExtends = new(stats_GroundCheck.WideCheckBuffer / 2, 0.01f, stats_GroundCheck.WideCheckBuffer / 2);
        if (Physics.BoxCast(origin, halfExtends, -springDir, Quaternion.identity, stats_GroundCheck.HeightCheckBuffer))
            return true;

        //bool isGrounded = Physics.BoxCast(origin, size / 2, -springDir, out rayHit, Quaternion.identity, heightCheckBuffer);
        //bool isGrounded = Physics.OverlapBox(cubeCenter, size / 2, Quaternion.identity, groundMask, QueryTriggerInteraction.Ignore);

        return false;
    }
    #endregion

    #region Move
    private void Move(StateComponent stateComponent)
    {
        Vector3 currentVelocity = rb.velocity;

        // Calcola la direzione relativa alla telecamera
        Vector3 cameraRelativeDirection = RelateTo(direction, Camera.main.transform);

        // Calcola la velocità desiderata in base alla direzione della telecamera
        Vector3 desireVelocity = new Vector3(cameraRelativeDirection.x, 0, cameraRelativeDirection.z) * Stats_Movement.MaxSpeed;

        float maxAcceleration;
        if (direction != Vector2.zero)
        {
            // Calcola il prodotto scalare tra la velocità corrente e quella desiderata
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            maxAcceleration = Stats_Movement.MaxAcceleration * Stats_Movement.AccelerationFactor.Evaluate(velDot);
        }
        else
        {
            maxAcceleration = Stats_Movement.MaxDeceleration;
        }

        // Calcola la variazione massima della velocità in questo frame
        float maxSpeedChange = maxAcceleration * Time.deltaTime * acceleration_Multiplaier;

        // Aggiorna la velocità corrente verso la velocità desiderata
        Vector3 finalVelocity = Vector3.MoveTowards(currentVelocity, desireVelocity, maxSpeedChange);

        finalVelocity.y = rb.velocity.y;

        Debug.DrawRay(stateComponent.transform.position, desireVelocity, Color.red);
        Debug.DrawRay(stateComponent.transform.position, finalVelocity, Color.green);

        // Assegna la nuova velocità al rigidbody
        //Dynamic rb 
        rb.velocity = finalVelocity;
        //rb.MovePosition(stateComponent.transform.position +  finalVelocity);
    }

    protected Vector3 RelateTo(Vector2 direction, Transform relativeTransform)
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
    #endregion

    private void Look(StateComponent stateComponent)
    {
        Quaternion playerRot = stateComponent.transform.rotation;
        playerRot.y = Camera.main.transform.rotation.y;
        stateComponent.transform.rotation = playerRot;
    }
}
