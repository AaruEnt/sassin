using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Autohand;
using NaughtyAttributes;

public class PrimaryButton : MonoBehaviour
{
    public Rigidbody rb;
    public AutoHandPlayer player;
    public MomentumController momentum;

    // if false, defaults to slide
    public bool jumpOnPress;
    [ShowIf("jumpOnPress")]
    public float jumpHeight = 40;
    [ShowIf("jumpOnPress")]
    public float maxJumpHeight = 50;
    [HideIf("jumpOnPress")]
    public float slideForce = 10f;

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
