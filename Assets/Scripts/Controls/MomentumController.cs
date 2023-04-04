using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Autohand.Demo;
using UnityEngine.UI;
using NaughtyAttributes;
using Valve.VR;

namespace Autohand {
    public class MomentumController : MonoBehaviour
    {
        [SerializeField, Tooltip("The player component, used to check/set current max movespeed and if the player is climbing")]
        private AutoHandPlayer player;

        [SerializeField, Tooltip("Used to check current speed and prevent falling while wall running")]
        private Rigidbody rb;

        [SerializeField, Tooltip("The max speed at max momentum")]
        internal float maxSpeedScale;

        [SerializeField, Tooltip("The max acceleration at max momentum")]
        private float momentumScale;

        [Header("Particles")]
        public bool displayParticleAtSpeed = false;

        [ShowIf("displayParticleAtSpeed")]
        public float speedThreshhold = 3f;

        [ShowIf("displayParticleAtSpeed")]
        public GameObject particleObject;

        [ShowIf("displayParticleAtSpeed")]
        public ParticleSystem windEffect;
        [ShowIf("displayParticleAtSpeed")]
        public float maxParticlesPerSecond = 0.85f;

        [SerializeField, Tooltip("Used to check movement axis")]
        private SteamVR_Action_Vector2 moveAction;


        private float startSpeed;
        private float startMomentum;
        private float deadzone = 0.1f;
        private Vector2 moveAxis;
        [ShowNonSerializedField]
        internal float counter = 0;
        private Vector2 lastMoveDir;
        private bool overrideParticle = false;
        [ShowIf("overrideParticle")]
        public float debugSpeed = 6f;

        private float magnitudePercentThreshhold = 0.95f;

        internal bool isWallRunning = false;

        private GameObject runningWall;

        internal bool isWallJumping = false;

        void Start()
        {
            startSpeed = player.maxMoveSpeed;
            startMomentum = player.moveAcceleration;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (isWallRunning)
            {
                if (counter < 270 || player.IsGrounded())
                    isWallRunning = false;
                else if (!player.IsClimbing() && !isWallJumping)
                {
                    Vector3 newVel = rb.velocity;
                    newVel.y = 0.15f;
                    rb.velocity = newVel;
                }
            }
            if (!player.IsClimbing())
            {
                moveAxis = moveAction.axis;
                // one or both axis are registering input
                if ((Mathf.Abs(moveAxis.x) > deadzone) || (Mathf.Abs(moveAxis.y) > deadzone))
                {
                    // Get the direction of the vector, compare it to the direction camera is facing (using dot product?) to determine if momentum should be increased
                    Vector3 dir = transform.forward;
                    Vector3 move = new Vector3(moveAxis.x, 0f, moveAxis.y);
                    dir.y = 0f;
                    // If moving roughly forward
                    if (Vector3.Dot(dir, move) > 0 && counter < 900)
                    {
                        counter += 1;
                    }
                    else // If moving roughly backwards or at max momentum
                    {
                        counter -= 4;
                    }
                }
                else // If not moving at all
                {
                    counter -= 2;
                }
                // If speed is magnitudePercentThreshhold of max speed or less
                if (rb.velocity.magnitude < player.maxMoveSpeed * magnitudePercentThreshhold)
                    counter -= 3;
                // clamp counter to prevent negative values
                if (counter <= 0)
                    counter = 0;
                // If momentum has built up for roughly 3 seconds of continuous forward movement or an equivalent
                if (counter >= 270)
                {
                   float diff1 = maxSpeedScale - startSpeed;
                   float diff2 = momentumScale - startMomentum;
                   if (diff1 <= 0 || diff2 <= 0)
                    {
                        Debug.LogWarning("Error: maxspeedscale and momentumscale cannot be lower than the starting values in AutoHandPlayer");
                        return;
                    }
                    player.maxMoveSpeed = startSpeed + ((maxSpeedScale / 14) * ((counter >= 900 ? 630 : counter - 270) / 45));
                    player.moveAcceleration = startMomentum + ((momentumScale / 14) * ((counter >= 900 ? 630 : counter - 270) / 45));
                }
                else // reset movespeed if momentum lost
                {
                    player.maxMoveSpeed = startSpeed;
                    player.moveAcceleration = startMomentum;
                }
                if (!overrideParticle)
                {
                    if (displayParticleAtSpeed && (rb.velocity.magnitude >= player.maxMoveSpeed * 0.95 && player.maxMoveSpeed >= speedThreshhold))
                    {
                        particleObject.SetActive(true);
                        var e = windEffect.emission;
                       e.rateOverTime = CalculateEmissionRate();
                    }
                    else
                    {
                        particleObject.SetActive(false);
                        var e = windEffect.emission;
                        e.rateOverTime = 0f;
                    }
                }
                else
                {
                    DebugWind(true);
                }
            }
            else
            {
                counter = counter < 0 ? 0 : counter - 0.5f;
            }
        }

        float CalculateEmissionRate(float speed = -1f)
        {
           float final;
            float currSpeed;
                if (speed == -1)
                currSpeed = rb.velocity.magnitude - speedThreshhold;
            else
                currSpeed = speed - speedThreshhold;
            float maxSpeed = maxSpeedScale + startSpeed;
            maxSpeed -= speedThreshhold;
            final = currSpeed / maxSpeed;

            return final * maxParticlesPerSecond;
        }

        [ShowIf("displayParticleAtSpeed"), Button]
        private void DebugWind()
        {
            overrideParticle = !overrideParticle;
            particleObject.SetActive(!particleObject.activeSelf);
            var e = windEffect.emission;
            e.rateOverTime = CalculateEmissionRate(debugSpeed);
        }

        private void DebugWind(bool scriptCall)
        {
            var e = windEffect.emission;
            e.rateOverTime = CalculateEmissionRate(debugSpeed);
        }
        
        public void LowerMagnitudeThreshhold()
        {
            magnitudePercentThreshhold = 0.5f;
        }

        private void OnCollisionEnter(Collision col)
        {
            // If player has collided with a wall
            if (Mathf.Abs(Vector3.Dot(col.GetContact(0).normal, Vector3.up)) < 0.1f)
            {
                //Wallrun Start
                isWallJumping = false;
                Vector3 newVel = rb.velocity;
                newVel.y = 0f;
                rb.velocity = newVel;
                isWallRunning = true;
                runningWall = col.collider.gameObject;
            }
        }

        private void OnCollisionExit(Collision col)
        {
            // Wallrun end
            if (runningWall == col.collider.gameObject)
            {
                runningWall = null;
                isWallRunning = false;
            }
        }

        public float GetMomentum()
        {
            return counter;
        }

        public void SetMomentum(float newMomentum)
        {
            counter = newMomentum;
        }
    }
}
