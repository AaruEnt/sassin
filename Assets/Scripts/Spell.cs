using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public bool npcSpell = true;
    public float chargeTime = 3f;
    public Vector3 targetPosition;
    public float timeOut = 15f;
    public GameObject body;
    public GameObject effectParticle;
    public GameObject collisionParticle;

    internal Enemy origin;
    internal float force = 15f;

    private bool isThrown = false;
    private float cd = 0f;

    void Update() {
        if (isThrown) {
            cd += Time.deltaTime;
        } else {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    internal IEnumerator ThrowSpell() {
        yield return new WaitForSeconds(chargeTime);
        var r = GetComponent<Rigidbody>();
        r.isKinematic = false;
        GetComponent<FollowObjectWithOffset>().enabled = false;
        transform.parent = null;
        isThrown = true;
        Vector3 BA = targetPosition - transform.position;
        r.AddForce(BA.normalized * force, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col) {
        if (!isThrown)
            return;
        body.SetActive(false);
        effectParticle.SetActive(false);
        collisionParticle.SetActive(true);
    }
}
