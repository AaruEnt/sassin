using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distraction : MonoBehaviour
{
    // Radius of sound ping
    public float overlapSphereRadius = 30f;
    // Max suspicion added
    public float addedSuspicion = 1.1f;
    public LayerMask mask;
    // cooldown before can ping again.
    private float cooldown = 0f;

    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
        else
            cooldown = 0f;
    }

    void OnCollisionEnter (Collision col) {
        if ((col.gameObject.tag == "Wall" || col.gameObject.tag == "Ground") && cooldown == 0f) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, overlapSphereRadius);
            foreach (var collider in hitColliders) {
                if (collider.gameObject.tag == "Enemy") {
                    Enemy e = collider.gameObject.GetComponent<Enemy>();
                    if (!e) {
                        Debug.LogError(string.Format("No enemy script found on enemy {0}", e.gameObject.name));
                        return;
                    }
                    RaycastHit hitinfo;
                    float distance = Vector3.Distance(e.transform.position, transform.position);
                    bool halfDistance = false;
                    Physics.Linecast(transform.position, e.transform.position, out hitinfo, mask);
                    if (hitinfo.rigidbody == null || hitinfo.rigidbody != e.gameObject.GetComponent<Rigidbody>()) {
                        halfDistance = true;
                    }
                    float susMod = 1f - (distance / overlapSphereRadius);
                    //Debug.Log(string.Format("Distance: {0}, Susmod: {1}, Sus added: {2}", distance, susMod, addedSuspicion * susMod));
                    if (halfDistance && distance >= overlapSphereRadius / 2)
                        e.attractAttention(transform, addedSuspicion * (susMod / 2f));
                    else {
                        e.attractAttention(transform, addedSuspicion * susMod);
                    }
                } else if (collider.gameObject.tag == "Civillian") {
                    Civillian c = collider.gameObject.GetComponent<Civillian>();
                    if (!c) 
                        Debug.LogError(string.Format("No civillian script found on civillian {0}", c.gameObject.name));
                    RaycastHit hitinfo;
                    float distance = Vector3.Distance(c.transform.position, transform.position);
                    bool halfDistance = false;
                    Physics.Linecast(transform.position, c.transform.position, out hitinfo, mask);
                    if (hitinfo.rigidbody == null || hitinfo.rigidbody != c.gameObject.GetComponent<Rigidbody>()) {
                        halfDistance = true;
                    }
                    float susMod = 1f - (distance / overlapSphereRadius);
                    if (halfDistance && distance >= overlapSphereRadius / 2)
                        c.attractAttention(transform, addedSuspicion * (susMod / 2));
                    else
                        c.attractAttention(transform, addedSuspicion * susMod); // Needs to calculate based on distance
                }
            }
        }
    }
}
