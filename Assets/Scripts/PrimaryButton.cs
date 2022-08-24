using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class PrimaryButton : MonoBehaviour
{
    // if false, defaults to slide
    public bool jumpOnPress;
    public Rigidbody rb;
    public AutoHandPlayer player;

    public void OnPrimaryButton() {
        if (jumpOnPress) {
            Jump();
        } else {
            Slide();
        }
    }

    private void Jump() {
        if (player.IsGrounded()) {
            rb.AddForce(new Vector3(0, 40, 0), ForceMode.Impulse);
        }
    }

    private void Slide() {
        Debug.Log("Slide");
    }
}
