using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : PlayerState
{
    [SerializeField] protected PlayerStats.GroundCheck_Stats stats_GroundCheck;
    [SerializeField] protected PlayerStats.MovementStats stats_Movement;

    protected Vector2 direction;
    protected float acceleration_Multiplaier = 1;

    [SerializeField] protected Rigidbody rb;


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

        Vector3 origin = stateComponent.transform.position + (stats_GroundCheck.HeightOffset * Vector3.up);

        //Old version
        //Vector3 cubeCenter = new(origin.x, origin.y - groundCheck_stats.HeightCheckBuffer / 2, origin.z);
        //Vector3 size = new(wideCheckBuffer, heightCheckBuffer, wideCheckBuffer);
        //Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;

        //bool isGrounded = Physics.BoxCast(origin, size / 2, -springDir, out rayHit, Quaternion.identity, heightCheckBuffer);
        //bool isGrounded = Physics.OverlapBox(cubeCenter, size / 2, Quaternion.identity, groundMask, QueryTriggerInteraction.Ignore);

        return false;
    }

    protected bool CheckGround()
    {
        //Siccome deve stare in piedi la forza andrà verso l'alto
        Vector3 springDir = stateComponent.transform.up;

        Vector3 origin = stateComponent.transform.position + (stats_GroundCheck.HeightOffset * Vector3.up);

        //Old version
        //Vector3 cubeCenter = new(origin.x, origin.y - groundCheck_stats.HeightCheckBuffer / 2, origin.z);
        //Vector3 size = new(wideCheckBuffer, heightCheckBuffer, wideCheckBuffer);
        //Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out RaycastHit rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * stats_GroundCheck.WideCheckBuffer / 2, -springDir, out rayHit, stats_GroundCheck.HeightCheckBuffer)) return true;

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
        Vector3 desireVelocity = new Vector3(cameraRelativeDirection.x, 0, cameraRelativeDirection.z) * stats_Movement.MaxSpeed;

        float maxAcceleration;
        if (direction != Vector2.zero)
        {
            // Calcola il prodotto scalare tra la velocità corrente e quella desiderata
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            maxAcceleration = stats_Movement.MaxAcceleration * stats_Movement.AccelerationFactor.Evaluate(velDot);
        }
        else
        {
            maxAcceleration = stats_Movement.MaxDeceleration;
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

    private Vector3 RelateTo(Vector2 inputDirection, Transform relativeTransform)
    {
        Vector3 camForward = relativeTransform.forward;
        Vector3 camRight = relativeTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 cameraRelativeForward = inputDirection.y * camForward;
        Vector3 cameraRelativeRight = inputDirection.x * camRight;

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
