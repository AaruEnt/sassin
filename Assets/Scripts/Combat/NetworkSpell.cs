using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSpell : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField, Tooltip("The charge time between the spell spawning and firing")]
    internal float chargeTime = 2f;

    [SerializeField, Tooltip("The max time the spell can exist")]
    internal float timeOut = 15f;

    [SerializeField, Tooltip("Damage dealt by the spell/attack. If the spell/attack deals damage it should have a damage value greater than 0")]
    internal float damage = 0f;

    [SerializeField, Tooltip("The vector3 position the spell is aimed at")]
    internal Vector3 targetPosition;
    // IDEA - create variance by having an accuracy value. Scale 0-5, add +- (5 - accuracy) to each value in the vector3


    [Header("Boolean Toggles")]
    [SerializeField, Tooltip("Is this spell cast by an NPC. May end up unused.")]
    internal bool npcSpell = true;


    [Header("References")]
    [SerializeField, Tooltip("The main body of the spell")]
    internal GameObject body;

    [SerializeField, Tooltip("The special effect particles of the spell")]
    internal GameObject effectParticle;

    [SerializeField, Tooltip("The particles activated on collision")]
    internal GameObject collisionParticle;

    internal LayerMask fireMask;
    internal bool waitFireOnSight = false;


    // Unserialized vars
    internal Enemy origin; // origin point for the spell to spawn
    internal float force = 15f; // Force added when fired

    internal bool isThrown = false; // has the spell been fired
    private float cd = 0f; // How long has it been since the spell was thrown

    internal Coroutine? c;

    void Update()
    {
        if (isThrown)
        {
            cd += Time.deltaTime;
        }
        else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (cd >= timeOut)
        {
            body.SetActive(false);
            effectParticle.SetActive(false);
            collisionParticle.SetActive(true);
        }
    }

    // Called by enemy script
    // Sends the spell effect towards target position
    internal IEnumerator ThrowSpell(Transform targetPos)
    {
        yield return new WaitForSeconds(chargeTime);
        targetPosition = targetPos.position;

        if (waitFireOnSight && targetPos != null)
        {
            RaycastHit hitinfo;
            while (!isThrown && Physics.Linecast(transform.position, targetPos.position, out hitinfo, fireMask))
            {
                bool? tmp = hitinfo.rigidbody?.transform.CompareTag(targetPos.tag);
                bool tmp2 = tmp != null ? (bool)tmp : false;
                if (!tmp2)
                {
                    yield return null;
                }
                else
                {
                    isThrown = true;
                    targetPosition = targetPos.position;
                }
            }
        }

        targetPosition.y += (0.25f + (float)Randomizer.GetDouble(0.75));
        var r = GetComponent<Rigidbody>();
        r.isKinematic = false;
        GetComponent<FollowObjectWithOffset>().enabled = false;
        Vector3 BA = targetPosition - transform.position;
        r.AddForce(BA.normalized * force, ForceMode.Impulse);
    }

    // Enables collision particles and disables regular particles and appearance on collision
    void OnCollisionEnter(Collision col)
    {
        if (!isThrown)
            return;
        body.SetActive(false);
        effectParticle.SetActive(false);
        collisionParticle.SetActive(true);
    }

    private void OnDestroy()
    {
        if (c != null)
            StopCoroutine(c);
    }
}
