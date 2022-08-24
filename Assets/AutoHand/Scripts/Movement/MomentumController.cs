using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Autohand.Demo;
using UnityEngine.UI;

public class MomentumController : MonoBehaviour
{
    public AutoHandPlayer player;
    public XRHandPlayerControllerLink link;

    public float maxSpeedScale;
    public float momentumScale;

    public Text debugText;

    private float startSpeed;
    private float startMomentum;
    private float deadzone = 0.1f;
    private Vector2 moveAxis;
    private float counter = 0;
    private Vector2 lastMoveDir;

    void Start() {
        startSpeed = player.maxMoveSpeed;
        startMomentum = player.moveAcceleration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveAxis = link.moveController.GetAxis2D(link.moveAxis);
        // one or both axis are registering input
        if ((Mathf.Abs(moveAxis.x) > deadzone) || (Mathf.Abs(moveAxis.y) > deadzone)) {
            // Get the direction of the vector, compare it to the direction camera is facing (using dot product?) to determine if momentum should be increased
            Vector3 dir = transform.forward;
            Vector3 move = new Vector3(moveAxis.x, 0f, moveAxis.y);
            dir.y = 0f;
            if (Vector3.Dot(dir, move) > 0 && counter < 900) {
                counter += 1;
            } else {
                counter = 0;
                player.maxMoveSpeed = startSpeed;
                player.moveAcceleration = startMomentum;
            }
        } else {
            counter = 0;
            player.maxMoveSpeed = startSpeed;
            player.moveAcceleration = startMomentum;
        }
        if (counter >= 270) {
            float diff1 = maxSpeedScale - startSpeed;
            float diff2 = momentumScale - startMomentum;
            if (diff1 <= 0 || diff2 <= 0) {
                Debug.Log("Error: maxspeedscale and momentumscale cannot be lower than the starting values in AutoHandPlayer");
                return;
            }
            player.maxMoveSpeed = startSpeed + ((maxSpeedScale / 14) * ((counter >= 900 ? 630 : counter - 270) / 45));
            player.moveAcceleration = startMomentum + ((momentumScale / 14) * ((counter >= 900 ? 630 : counter - 270) / 45));
        }
        debugText.text = string.Format("Speed: {0}\nAccel: {1}\nCounter: {2}", player.maxMoveSpeed, player.moveAcceleration, counter);
    }
}
