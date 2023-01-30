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

    [Header("Variables")]
    [HideIf("jumpOnPress")]
    [SerializeField, Tooltip("The additional force applied while sliding")]
    private float slideForce = 10f;

    private bool isSliding = false;

    void Update() {
        //if (!isSliding && player.IsCrouching())
        //    Slide();
    }

    public void OnPrimaryButton() {
        if (jumpOnPress) {
            Jump();
        } else {
            Slide();
        }
    }

    private void Jump() {
        float blendJumpHeight = jumpHeight + (((maxJumpHeight - jumpHeight) / 14) * ((momentum.counter >= 900 ? 630 : momentum.counter - 270) / 45));
        if (player.IsGrounded()) {
            rb.AddForce(new Vector3(0, blendJumpHeight, 0), ForceMode.Impulse);
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
}
