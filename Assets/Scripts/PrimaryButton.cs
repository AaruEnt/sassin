using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Autohand;

public class PrimaryButton : MonoBehaviour
{
    // if false, defaults to slide
    public bool jumpOnPress;
    public Rigidbody rb;
    public AutoHandPlayer player;
    public MomentumController momentum;
    public float jumpHeight = 40;
    public float maxJumpHeight = 50;

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
        Debug.Log("Slide");
    }
}
