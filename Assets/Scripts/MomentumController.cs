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
        public AutoHandPlayer player;
        public SteamVRHandPlayerLink link;
        public Rigidbody rb; //Debug

        public float maxSpeedScale;
        public float momentumScale;

        public Text debugText;
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

        public SteamVR_Action_Vector2 moveAction;


        private float startSpeed;
        private float startMomentum;
        private float deadzone = 0.1f;
        private Vector2 moveAxis;
        internal float counter = 0;
        private Vector2 lastMoveDir;
        private bool overrideParticle = false;
        [ShowIf("overrideParticle")]
        public float debugSpeed = 6f;

        void Start()
        {
            startSpeed = player.maxMoveSpeed;
            startMomentum = player.moveAcceleration;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //Debug.Log(rb.velocity.magnitude);
            if (!player.IsClimbing())
            {
                moveAxis = moveAction.axis;
                //Debug.Log(moveAxis);
                // one or both axis are registering input
                if ((Mathf.Abs(moveAxis.x) > deadzone) || (Mathf.Abs(moveAxis.y) > deadzone))
                {
                    // Get the direction of the vector, compare it to the direction camera is facing (using dot product?) to determine if momentum should be increased
                    Vector3 dir = transform.forward;
                    Vector3 move = new Vector3(moveAxis.x, 0f, moveAxis.y);
                    dir.y = 0f;
                    if (Vector3.Dot(dir, move) > 0 && counter < 900)
                    {
                        counter += 1;
                    }
                    else
                    {
                        counter -= 4;
                    }
                }
                else
                {
                    counter -= 2;
                }
                if (rb.velocity.magnitude < player.maxMoveSpeed * 0.95)
                    counter -= 3;
                if (counter <= 0)
                    counter = 0;
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
                else
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
            debugText.text = string.Format("Speed: {0}\nAccel: {1}\nCounter: {2}\nVelocity: {3}", player.maxMoveSpeed, player.moveAcceleration, counter, rb.velocity.magnitude);
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
    }
}
