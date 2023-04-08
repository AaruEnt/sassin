using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Autohand;
using NaughtyAttributes;

public class PrimaryButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Reference to the player rigidbody")]
    private Rigidbody rb;

    [SerializeField, Tooltip("Reference to the 'tracked objects' or similar gameObject, used to determine the direction the player is facing")]
    private GameObject playerFacingTransform;

    [SerializeField, Tooltip("Reference to the player")]
    private AutoHandPlayer player;

    [SerializeField, Tooltip("Reference to the momentum manager")]
    private MomentumController momentum;


    [Header("Boolean Toggles")]
    [SerializeField, Tooltip("If false, defaults to slide")]
    private bool jumpOnPress;


    [Header("Variables")]
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The base jump height")]
    private float jumpHeight = 40;
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The maximum jump height after momentum")]
    private float maxJumpHeight = 50;
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The mask for detecting wall direction")]
    private LayerMask wallJumpMask;
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The outward force used when wall jumping")]
    private float wallJumpForce = 5f;
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The audiosource for jump sounds")]
    private AudioSource jumpSound;
    [ShowIf("jumpOnPress")]
    [SerializeField, Tooltip("The audiosource for jump sounds")]
    private AudioSource landSound;

    [Header("Variables")]
    [HideIf("jumpOnPress")]
    [SerializeField, Tooltip("The additional force applied while sliding")]
    private float slideForce = 10f;

    private bool isSliding = false;

    internal bool canJump = false;

    private bool jumpRoutineRunning = false;
    internal float jumpCD = 0f;
    private bool hasWallRunJumped = false;

    private bool hasJumped = false;

    private float fallTracker = 0f;

    void Update() {
        //if (!isSliding && player.IsCrouching())
        //    Slide();
        if (momentum.isWallRunning)
            canJump = true;
        else if (jumpRoutineRunning == false)
        {
            jumpRoutineRunning = true;
            StartCoroutine(JumpRoutine());
        }

        if (player.IsGrounded() && hasJumped)
        {
            hasWallRunJumped = false;
            momentum.isWallJumping = false;
            hasJumped = false;
        }

        if (!player.IsGrounded() && jumpOnPress)
        {
            fallTracker += Time.deltaTime;
        }
        else
        {
            if (fallTracker >= 1f)
                landSound.Play();
            fallTracker = 0f;
        }
        if (jumpCD > 0f)
            jumpCD -= Time.deltaTime;
    }

    public void OnPrimaryButton() {
        if (jumpOnPress) {
            Jump();
        } else {
            Slide();
        }
    }

    private void Jump() {
        if (jumpCD > 0)
            return;
        float blendJumpHeight = jumpHeight + (((maxJumpHeight - jumpHeight) / 14) * ((momentum.counter >= 900 ? 630 : momentum.counter - 270) / 45));
        if (player.IsGrounded()) {
            rb.AddForce(new Vector3(0, blendJumpHeight, 0), ForceMode.Impulse);
            jumpSound.Play();
            hasJumped = true;
            jumpCD = 0.5f;
            //rb.velocity = Vector3.MoveTowards(rb.velocity, new Vector3(rb.velocity.x, blendJumpHeight, rb.velocity.z), 40f);
        } else if (canJump && !hasWallRunJumped)
        {
            hasWallRunJumped = true;
            momentum.isWallJumping = true;
            rb.AddForce(new Vector3(0, blendJumpHeight * 0.9f, 0), ForceMode.Impulse);
            jumpSound.Play();
            hasJumped = true;
            jumpCD = 1f;
        }
        else if (false) // replace canJump above to enable wall jumping
        {
            if (Physics.Raycast(playerFacingTransform.transform.position, playerFacingTransform.transform.right, 1f, wallJumpMask))
            {
                rb.AddForce(new Vector3(0, 1f, 0) + (playerFacingTransform.transform.right * -wallJumpForce) + (playerFacingTransform.transform.forward * wallJumpForce), ForceMode.Impulse);
            }
            if (Physics.Raycast(playerFacingTransform.transform.position, -playerFacingTransform.transform.right, 1f, wallJumpMask))
            {
                rb.AddForce(new Vector3(0, blendJumpHeight / 4, 0) + (playerFacingTransform.transform.right * wallJumpForce) + (playerFacingTransform.transform.forward * wallJumpForce), ForceMode.Impulse);
            }
            if (Physics.Raycast(playerFacingTransform.transform.position, playerFacingTransform.transform.forward, 1f, wallJumpMask))
            {
                rb.AddForce(new Vector3(0, blendJumpHeight / 4, 0) + (playerFacingTransform.transform.forward * -wallJumpForce) + (playerFacingTransform.transform.forward * wallJumpForce), ForceMode.Impulse);
            }
        }
    }

    private void Slide() {
        if (!isSliding && momentum.counter >= 800) {
            isSliding = true;
            StartCoroutine(DoSlide());
        }
    }

    private IEnumerator DoSlide() {
        player.crouching = true;
        float tmp = momentum.maxSpeedScale;
        momentum.maxSpeedScale = tmp + 2;
        rb.AddRelativeForce(Vector3.forward * slideForce, ForceMode.Impulse);
        yield return new WaitForSeconds(1);
        isSliding = false;
        player.crouching = false;
        momentum.maxSpeedScale = tmp;
    }

    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        jumpRoutineRunning = false;
        canJump = false;
    }
}
