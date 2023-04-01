using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_RoverController : MonoBehaviour
{
    public Rigidbody rB;

    private float speedTorque = 70f;

    public List<WheelCollider> frontWheels;
    public List<WheelCollider> backWheels;

    private int wheelMem = 0;
    private Vector2 playerMov;
    private bool jumpReload = true;
    private float jumpRestraint = 1f;
    private bool jumpCoyotee = false;

    private void Update()
    {
        playerMov = PlayerInput();
    }
    void FixedUpdate()
    {
        PlayerMovement(playerMov);
    }

    private Vector2 PlayerInput()
    {
        Vector2 playerMovement = Vector2.zero;
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z))
        {
            playerMovement.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerMovement.y -= 1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
        {
            playerMovement.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerMovement.x += 1;
        }
        if (jumpReload)
        {
            if(wheelMem < 4)
            {
                StartCoroutine(WaitCoyoteeJump());
            }

            if (Input.GetKeyDown(KeyCode.Space) && jumpCoyotee)
            {
                PlayerJump();
            }
        }
        return playerMovement;
    }

    private void PlayerMovement(Vector2 movement)
    {
            rB.maxAngularVelocity = 12f;

            float clampValueRot = 35f;
            float signRot = 1f;

            float TorqueMult = 1.33f;
            if(movement.y < 0)
            {
                clampValueRot = 5f; 
                signRot = -1f;
            }

            foreach (WheelCollider wheel in frontWheels)
            {
                wheel.steerAngle = movement.x*30f;
                wheel.motorTorque = TorqueMult * movement.y * 3f;

                ApplyLocalPositionToVisuals(wheel);
            }                  
            foreach(WheelCollider wheel in backWheels)
            {
                wheel.motorTorque = TorqueMult * movement.y*3f;

                ApplyLocalPositionToVisuals(wheel);
            }

        rB.AddTorque(new Vector3(-movement.y, 0, -movement.x) * speedTorque * 0.1f, ForceMode.Force);
    }

    private void PlayerJump()
    {
        StartCoroutine(WaitJump());
        rB.AddForce((Vector3.up * 2f) * 12f * jumpRestraint, ForceMode.Impulse);
    }

    private void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private IEnumerator WaitCoyoteeJump()
    {
        jumpCoyotee = true;
        yield return new WaitForSeconds(0.1f);
        jumpCoyotee = false;
    }

    private IEnumerator WaitJump()
    {
        jumpReload = false;
        yield return new WaitForSeconds(0.50f);
        jumpReload = true;
    }
}
